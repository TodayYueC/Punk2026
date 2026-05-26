using Punk2026.Actor;
using Punk2026.Player.States;
using Punk2026.Weapon;
using UnityEngine;

namespace Punk2026.Player
{
    /// <summary>
    /// 玩家管理器 —— 3C 系统的核心入口
    /// 继承自 Actor&lt;PlayerManager&gt;，拥有完整的运动系统和状态机驱动能力
    /// 职责：聚合输入读取、配置引用、跳跃/闪避/重力等玩家专属逻辑
    /// 不直接处理动画或相机，由 PlayerAnimator 和 TopDownCamera 各自监听
    /// </summary>
    public class PlayerManager : Actor<PlayerManager>
    {
        /// <summary>玩家事件容器（跳跃/闪避/落地回调）</summary>
        public PlayerEvents playerEvents;

        /// <summary>ScriptableObject 配置资产引用（运动参数、跳跃参数、闪避参数）</summary>
        [SerializeField]
        private PlayerConfig playerConfig;

        /// <summary>输入读取器组件引用</summary>
        public PlayerInputReader input { get; protected set; }

        /// <summary>配置资产的公开访问器</summary>
        public PlayerConfig config => playerConfig;

        // ========== 跳跃状态 ==========

        /// <summary>当前已跳跃次数（0 = 未跳，1 = 第一次跳，2 = 二段跳...）</summary>
        public int jumpCounter { get; protected set; }

        // ========== 闪避状态 ==========

        /// <summary>最后一次执行闪避的时间戳，用于冷却判定</summary>
        public float lastDodgeTime { get; protected set; }

        /// <summary>当前闪避的方向向量（闪避开始时锁定）</summary>
        public Vector3 dodgeDirection { get; protected set; }

        public override void TakeDamage(float damage)
        {
            // TODO: 角色受伤逻辑
        }

        public override void Die()
        {
            // TODO: 角色死亡逻辑
        }

        /// <summary>从同一 GameObject 获取输入读取器</summary>
        protected virtual void InitInput() => input = GetComponent<PlayerInputReader>();

        
        // ========== 武器系统 ==========

        /// <summary>武器管理器引用</summary>
        public PlayerWeaponManager weaponManager { get; protected set; }

        /// <summary>射击朝向锁定（射击瞬间为 true，阻止状态中的 FaceMoveDirection 覆盖瞄准朝向）</summary>
        public bool isShootingRotationLocked { get; protected set; }

        /// <summary>初始化：基础组件 → 输入读取器 → 注册落地事件</summary>
        protected override void Awake()
        {
            base.Awake();
            InitInput();

            // 落地时自动重置跳跃计数（允许多段跳重新开始）
            actorEvents.OnGroundEnter.AddListener(() => ResetJump());
            //获取武器管理器
            weaponManager = GetComponent<PlayerWeaponManager>();
        }

        // ========== 运动方法（封装配置参数，简化状态类调用） ==========

        /// <summary>根据输入方向加速（使用地面运动参数）</summary>
        public virtual void AccelerateToInput()
        {
            var worldDirection = input.GetWorldMoveDirection();
            Accelerate(worldDirection);
        }

        /// <summary>地面加速（使用地面转向阻力和最大速度）</summary>
        public virtual void Accelerate(Vector3 direction)
        {
            Accelerate(direction, config.moveTurningDrag, config.moveAcceleration, config.moveTopSpeed);
        }

        /// <summary>空中加速（使用空中加速度参数，手感更飘）</summary>
        public virtual void AccelerateAir(Vector3 direction)
        {
            Accelerate(direction, config.moveTurningDrag, config.airAcceleration, config.moveTopSpeed);
        }

        /// <summary>减速（松开输入时的急停）</summary>
        public virtual void Decelerate() => Decelerate(config.moveDeceleration);

        /// <summary>摩擦（无输入时的自然减速）</summary>
        public virtual void ApplyFriction() => Decelerate(config.moveFriction);

        // ========== 朝向控制 ==========

        /// <summary>面向鼠标瞄准方向（俯视角双摇杆核心：移动和朝向解耦）</summary>
        public virtual void FaceAimDirection()
        {
            var aimDir = input.GetAimDirection(transformPosition);
            FaceDirection(aimDir, config.rotationSpeed);
        }

        /// <summary>面向移动方向（用于没有鼠标输入时的退化方案）</summary>
        public virtual void FaceMoveDirection()
        {
            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                FaceDirection(horizontalVelocity, config.rotationSpeed);
            }
        }

        // ========== 重力系统 ==========

        /// <summary>
        /// 应用重力 —— 仅在非地面状态下生效
        /// 上升阶段使用较小重力（gravity），下落阶段使用较大重力（fallGravity）
        /// 实现"快落"手感：跳起慢、落下快，提升操作响应感
        /// </summary>
        public virtual void ApplyGravity()
        {
            if (!isGrounded && upwardVelocity.y > -config.maxFallSpeed)
            {
                var speed = upwardVelocity.y;
                // 上升时用普通重力，下落时用加重重力
                var force = upwardVelocity.y > 0 ? config.gravity : config.fallGravity;
                speed -= force * gravityMultiplier * Time.deltaTime;
                speed = Mathf.Max(speed, -config.maxFallSpeed); // 限制最大下落速度
                upwardVelocity = new Vector3(0, speed, 0);
            }
        }

        /// <summary>强制贴地（使用配置中的贴地力）</summary>
        public virtual void SnapToGround() => SnapToGround(config.snapForce);

        // ========== 跳跃系统 ==========

        /// <summary>重置跳跃计数（落地时由事件触发）</summary>
        public virtual void ResetJump() => jumpCounter = 0;

        /// <summary>
        /// 尝试跳跃 —— 包含完整的跳跃判定逻辑：
        ///   1. 地面跳跃：isGrounded 时可跳
        ///   2. 多段跳：jumpCounter > 0 且未超过上限
        ///   3. 土狼跳：离地后 coyoteTimeWindow 内仍可跳
        ///   4. 跳跃缓冲：落地前 bufferDuration 内按的跳会在落地瞬间触发
        /// </summary>
        /// <returns>是否成功执行了跳跃</returns>
        public virtual bool TryJump()
        {
            // 多段跳判定：已经跳过但未超过上限
            var canExtraJump = jumpCounter > 0 && jumpCounter < config.extraJumps;
            // 土狼跳判定：首次跳跃 + 离地时间在窗口内
            var canCoyoteJump = jumpCounter == 0 &&
                                Time.time < lastGroundTime + config.coyoteTimeWindow;

            if (isGrounded || canExtraJump || canCoyoteJump)
            {
                // 消耗跳跃缓冲（如果有的话）
                if (input.ConsumeJumpBuffer())
                {
                    ExecuteJump(config.maxJumpForce);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 变高跳跃 —— 松开跳跃键时截断上升速度
        /// 实现"按得越久跳得越高"的手感：
        ///   - 长按：达到 maxJumpForce 的完整高度
        ///   - 短按：速度被截断到 minJumpForce，跳跃高度降低
        /// </summary>
        public virtual void HandleVariableHeightJump()
        {
            if (input.GetJumpReleased() && jumpCounter > 0 && upwardVelocity.y > config.minJumpForce)
            {
                upwardVelocity = Vector3.up * config.minJumpForce;
            }
        }

        /// <summary>执行跳跃 —— 设置垂直速度、切换状态、触发事件</summary>
        public virtual void ExecuteJump(float force)
        {
            jumpCounter++;
            upwardVelocity = Vector3.up * force;
            stateManager.Change<FallPlayerState>(); // 跳起后立即进入下落状态
            playerEvents.OnJump?.Invoke();
        }

        /// <summary>检测是否离地 —— 离地时切换到下落状态</summary>
        public virtual void CheckFall()
        {
            if (!isGrounded)
            {
                stateManager.Change<FallPlayerState>();
            }
        }

        // ========== 闪避系统 ==========

        /// <summary>
        /// 尝试闪避 —— 检测输入和冷却
        /// 闪避方向：优先使用当前移动输入方向，无输入时使用角色当前朝向
        /// </summary>
        /// <returns>是否成功执行了闪避</returns>
        public virtual bool TryDodge()
        {
            if (!input.GetDodgePressed()) return false;
            // 冷却判定
            if (Time.time < lastDodgeTime + config.dodgeCooldown) return false;

            // 确定闪避方向：有移动输入用输入方向，否则用角色朝前方向
            var moveDir = input.GetWorldMoveDirection();
            dodgeDirection = moveDir.sqrMagnitude > 0 ? moveDir : transform.forward;
            lastDodgeTime = Time.time;
            stateManager.Change<DodgePlayerState>();
            playerEvents.OnDodge?.Invoke();
            return true;
        }

        /// <summary>获取当前瞄准方向（供 TopDownCamera 等外部组件使用）</summary>
        public virtual Vector3 GetAimDirection()
        {
            return input.GetAimDirection(transformPosition);
        }

        /// <summary>
        /// 尝试射击 —— 每帧由状态类调用
        /// 按下：锁定朝向 + 面向瞄准 + 开火
        /// 按住：保持锁定 + 面向瞄准 + 持续开火（武器冷却控制射速）
        /// 松开：解除锁定，恢复面向移动方向
        /// </summary>
        public virtual bool TryFire()
        {
            // 按住射击键期间：锁定 + 转向 + 射击
            if (input.GetFireHeld())
            {
                isShootingRotationLocked = true;
                FaceAimDirection();

                if (weaponManager != null)
                {
                    Vector3 aimDirection = GetAimDirection();
                    if (aimDirection.sqrMagnitude > 0.001f)
                    {
                        weaponManager.TryFireCurrentWeapon(aimDirection);
                    }
                }
                return true;
            }

            // 松开射击键：解除锁定
            isShootingRotationLocked = false;
            return false;
        }
    }
}
