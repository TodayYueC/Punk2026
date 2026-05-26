using UnityEngine;

namespace Punk2026.Actor
{
    /// <summary>
    /// Actor 基类 —— 所有游戏实体的最底层抽象
    /// 负责：CharacterController 管理、地面检测（SphereCast）、基础物理属性
    /// 不包含任何业务逻辑，仅提供引擎级通用功能
    /// </summary>
    public abstract class ActorBase : MonoBehaviour
    {
        /// <summary>实体事件容器（落地/离地回调），由 Inspector 绑定</summary>
        public ActorEvents actorEvents;

        /// <summary>Transform 的世界坐标位置（不受 CharacterController center 影响）</summary>
        public Vector3 transformPosition => transform.position;

        /// <summary>地面检测的额外偏移量，防止 SphereCast 精度问题导致误判</summary>
        protected readonly float groundCheckOffset = 0.1f;

        /// <summary>当前是否站在地面上</summary>
        public bool isGrounded { get; protected set; } = true;

        /// <summary>CharacterController 组件引用</summary>
        public CharacterController controller { get; protected set; }

        /// <summary>角色初始高度，用于地面检测距离计算</summary>
        public float startHeight { get; protected set; }

        /// <summary>最后一次离开地面的时间戳（Time.time），用于土狼跳判定</summary>
        public float lastGroundTime { get; protected set; }

        /// <summary>当前地面法线方向</summary>
        public Vector3 groundNormal { get; protected set; }

        /// <summary>最近一次地面检测的射线命中信息</summary>
        public RaycastHit groundHit;

        /// <summary>角色高度（取自 CharacterController）</summary>
        public float height => controller.height;

        /// <summary>角色碰撞半径（取自 CharacterController）</summary>
        public float radius => controller.radius;

        /// <summary>CharacterController 的本地中心点</summary>
        public Vector3 center => controller.center;

        /// <summary>角色世界空间中心位置 = transform.position + center</summary>
        public Vector3 position => transform.position + center;

        /// <summary>
        /// 球形射线检测（封装 Physics.SphereCast）
        /// 用于地面检测等物理查询
        /// </summary>
        /// <param name="direction">射线方向</param>
        /// <param name="distance">检测距离</param>
        /// <param name="hit">命中结果输出</param>
        /// <param name="layer">检测层级掩码</param>
        /// <param name="queryTriggerInteraction">是否检测 Trigger</param>
        /// <summary>受击 —— 扣除生命值，HP 归零时触发死亡（由子类实现）</summary>
        public abstract void TakeDamage(float damage);

        /// <summary>死亡 —— 触发死亡逻辑（由子类实现，如敌人爆炸、玩家 GameOver）</summary>
        public abstract void Die();

        public virtual bool SphereCast(Vector3 direction, float distance,
            out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            // 有效距离 = 总距离 - 球体半径，确保 SphereCast 范围准确
            var castDistance = Mathf.Abs(distance - radius);
            return Physics.SphereCast(position, radius, direction,
                out hit, castDistance, layer, queryTriggerInteraction);
        }
    }

    /// <summary>
    /// 泛型 Actor 基类 —— 带状态机和运动系统的游戏实体
    /// 继承自 ActorBase，增加：速度管理、状态机驱动、移动/加速/减速/朝向等核心运动能力
    /// T 为子类自身类型（CRTP 模式），确保状态机和组件能以强类型方式引用具体实体
    /// </summary>
    /// <typeparam name="T">具体实体类型（如 PlayerManager）</typeparam>
    public abstract class Actor<T> : ActorBase where T : Actor<T>
    {
        /// <summary>状态机管理器</summary>
        public ActorStateManager<T> stateManager { get; protected set; }

        /// <summary>当前帧的完整三维速度向量</summary>
        public Vector3 velocity { get; set; }

        // ========== 运动倍率系统（用于 Buff/Debuff 动态调整运动参数） ==========
        public float accelerationMultiplier { get; set; } = 1f;
        public float gravityMultiplier { get; set; } = 1f;
        public float topSpeedMultiplier { get; set; } = 1f;
        public float turningDragMultiplier { get; set; } = 1f;
        public float decelerationMultiplier { get; set; } = 1f;

        /// <summary>
        /// 水平速度分量（XZ 平面）
        /// 读取：从完整速度中提取 XZ 分量，Y 归零
        /// 写入：仅修改 XZ 分量，保留 Y 轴速度不变
        /// </summary>
        public Vector3 horizontalVelocity
        {
            get => new Vector3(velocity.x, 0, velocity.z);
            set => velocity = new Vector3(value.x, velocity.y, value.z);
        }

        /// <summary>
        /// 垂直速度分量（Y 轴）
        /// 读取：从完整速度中提取 Y 分量，XZ 归零
        /// 写入：仅修改 Y 分量，保留水平速度不变
        /// </summary>
        public Vector3 upwardVelocity
        {
            get => new Vector3(0, velocity.y, 0);
            set => velocity = new Vector3(velocity.x, value.y, velocity.z);
        }

        /// <summary>驱动当前状态的每帧逻辑</summary>
        protected virtual void HandleStates() => stateManager.Step();

        /// <summary>将速度应用到 CharacterController.Move</summary>
        protected virtual void HandleController()
        {
            if (controller.enabled)
            {
                controller.Move(velocity * Time.deltaTime);
            }
        }

        /// <summary>从同一 GameObject 获取状态机引用</summary>
        protected virtual void InitStateManager() => stateManager = GetComponent<ActorStateManager<T>>();

        /// <summary>初始化顺序：先 CharacterController，再状态机</summary>
        protected virtual void Awake()
        {
            InitController();
            InitStateManager();
        }

        /// <summary>
        /// 初始化 CharacterController
        /// 如果不存在则自动添加，设置 skinWidth 和 minMoveDistance 为极小值避免物理穿透
        /// </summary>
        protected virtual void InitController()
        {
            controller = GetComponent<CharacterController>();
            if (!controller) controller = gameObject.AddComponent<CharacterController>();

            controller.skinWidth = 0.005f;   // 最小皮肤宽度，减少穿墙
            controller.minMoveDistance = 0;   // 允许极小位移，避免卡顿
            startHeight = controller.height;
        }

        // ========== 地面检测生命周期 ==========

        /// <summary>进入地面（刚落地时调用）—— 仅在非地面状态时触发，防止重复调用</summary>
        protected virtual void EnterGround(RaycastHit hit)
        {
            if (!isGrounded)
            {
                groundHit = hit;
                isGrounded = true;
                actorEvents.OnGroundEnter?.Invoke(); // 广播落地事件
            }
        }

        /// <summary>离开地面（刚跳起或掉落时调用）—— 记录离地时间用于土狼跳判定</summary>
        protected virtual void ExitGround()
        {
            if (isGrounded)
            {
                isGrounded = false;
                lastGroundTime = Time.time; // 记录离地时刻，土狼跳窗口从此开始
                // 离地时清除向下的垂直速度，防止负速度叠加
                upwardVelocity = Vector3.Max(upwardVelocity, Vector3.zero);
                actorEvents.OnGroundExit?.Invoke(); // 广播离地事件
            }
        }

        /// <summary>持续站在地面上时，每帧更新地面信息（法线、碰撞点等）</summary>
        protected virtual void UpdateGround(RaycastHit hit)
        {
            if (isGrounded)
            {
                groundHit = hit;
                groundNormal = groundHit.normal;
            }
        }

        /// <summary>
        /// 强制贴地 —— 在地面上且正在下落时，施加向下的力保持贴合地面
        /// 防止下坡时角色弹起
        /// </summary>
        public virtual void SnapToGround(float force)
        {
            if (isGrounded && upwardVelocity.y <= 0)
            {
                upwardVelocity = Vector3.down * force;
            }
        }

        /// <summary>
        /// 地面检测主逻辑 —— 每帧由 Update 调用
        /// 使用 SphereCast 向下检测：
        ///   - 检测到地面 + 正在下落 → 进入地面或更新地面状态
        ///   - 未检测到地面 → 离开地面
        /// </summary>
        protected virtual void HandleGround()
        {
            // 检测距离 = 角色半高 + 额外偏移
            var distance = (height * 0.5f) + groundCheckOffset;

            if (SphereCast(Vector3.down, distance, out var hit) && upwardVelocity.y <= 0)
            {
                if (!isGrounded)
                {
                    // 不在地面上 → 判断是否满足落地条件
                    if (EvaluateLanding(hit))
                    {
                        EnterGround(hit);
                    }
                }
                else
                {
                    // 已在地面上 → 持续更新地面信息
                    UpdateGround(hit);
                }
            }
            else
            {
                // 射线未命中 → 离开地面
                ExitGround();
            }
        }

        /// <summary>判断落地条件：地面角度不超过斜坡限制</summary>
        protected virtual bool EvaluateLanding(RaycastHit hit)
        {
            return Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
        }

        /// <summary>主循环：状态机驱动 → 物理移动 → 地面检测，严格按顺序执行</summary>
        protected void Update()
        {
            if (controller.enabled)
            {
                HandleStates();     // 1. 驱动当前状态逻辑
                HandleController(); // 2. 应用速度到 CharacterController
                HandleGround();     // 3. 地面检测与状态更新
            }
        }

        // ========== 运动辅助方法 ==========

        /// <summary>
        /// 平滑朝向旋转 —— 使用 RotateTowards 限制每帧最大旋转角度
        /// 用于角色模型朝向平滑过渡，避免瞬间转向
        /// </summary>
        public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
        {
            if (direction != Vector3.zero)
            {
                var rotation = transform.rotation;
                var rotationDelta = degreesPerSecond * Time.deltaTime;
                var target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
            }
        }

        /// <summary>减速 —— 将水平速度向零靠近，用于刹车和摩擦</summary>
        public virtual void Decelerate(float deceleration)
        {
            var delta = deceleration * decelerationMultiplier * Time.deltaTime;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, delta);
        }

        /// <summary>
        /// 加速运动 —— 核心运动算法
        /// 实现了转向阻力（turningDrag）和速度上限（topSpeed）的平滑加速度模型
        /// 算法步骤：
        ///   1. 计算当前速度在目标方向上的投影分量
        ///   2. 提取垂直于目标方向的转向分量
        ///   3. 对投影分量施加加速度（受上限限制）
        ///   4. 对转向分量施加转向阻力（平滑消除横向漂移）
        ///   5. 合并两分量得到最终水平速度
        /// </summary>
        public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
        {
            if (direction.sqrMagnitude > 0)
            {
                // 当前速度在目标方向上的投影标量
                var speed = Vector3.Dot(direction, horizontalVelocity);
                // 投影方向上的速度向量
                var velocityInDirection = direction * speed;
                // 垂直于目标方向的转向速度分量
                var turningVelocity = horizontalVelocity - velocityInDirection;
                // 转向阻力的本帧衰减量
                var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
                // 考虑倍率后的最大速度
                var targetTopSpeed = topSpeed * topSpeedMultiplier;

                // 未达上限或正在反向加速时，施加加速度
                if (horizontalVelocity.magnitude < targetTopSpeed || speed < 0)
                {
                    speed += acceleration * accelerationMultiplier * Time.deltaTime;
                    speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
                }

                velocityInDirection = direction * speed;
                // 转向分量平滑归零，实现自然的转向过渡
                turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
                horizontalVelocity = velocityInDirection + turningVelocity;
            }
        }
    }
}
