using UnityEngine;
using UnityEngine.InputSystem;

namespace Punk2026.Player
{
    /// <summary>
    /// 玩家输入读取器 —— 封装新版 Input System 的所有玩家输入
    /// 职责：
    ///   1. 读取 WASD/手柄摇杆的移动输入，并转换为相机相对的世界空间方向
    ///   2. 通过鼠标射线与地面平面求交获取世界坐标瞄准点
    ///   3. 管理跳跃缓冲（Jump Buffer）机制
    ///   4. 提供硬件级 DSP 时间戳映射（为节奏系统预留接口）
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        /// <summary>Input Actions 资产引用（包含 Move/Look/Jump/Sprint 等动作）</summary>
        public InputActionAsset actions;

        // 输入动作缓存（在 Awake 中从 actions 中查找并缓存）
        protected InputAction moveAction;   // WASD / 左摇杆
        protected InputAction aimAction;    // 鼠标移动 / 右摇杆
        protected InputAction jumpAction;   // Space / 手柄 A
        protected InputAction dodgeAction;  // Left Shift / Sprint（复用为闪避）

        /// <summary>主相机引用，用于相机相对方向转换和鼠标射线投射</summary>
        protected Camera mainCamera;

        protected const string MouseDeviceName = "Mouse";

        // ========== 跳跃缓冲系统 ==========
        // 跳跃缓冲：玩家在落地前极短时间内按下跳跃，落地瞬间自动触发跳跃
        // 解决"还没落地就按了跳但没反应"的操作痛点

        /// <summary>最近一次按下跳跃的时间戳</summary>
        protected float? lastJumpPressTime;

        /// <summary>跳跃缓冲窗口时长（秒）</summary>
        protected const float JumpBufferDuration = 0.15f;

        // ========== DSP 时间映射（为节奏系统预留） ==========
        // 通过缓存每帧的 dspTime 和 realtimeSinceStartup，
        // 可以将 Input System 的硬件时间戳精确映射到音频 DSP 时钟

        /// <summary>当前帧开始时的音频 DSP 时间</summary>
        protected double currentFrameDspTime;

        /// <summary>当前帧开始时的物理实时时间</summary>
        protected float currentFrameRealtime;

        protected virtual void Start()
        {
            actions.Enable();
        }

        /// <summary>每帧缓存 DSP 时间，并检测跳跃输入是否按下</summary>
        protected virtual void Update()
        {
            // 缓存本帧的音频和物理时钟基准（用于硬件时间戳映射）
            currentFrameDspTime = AudioSettings.dspTime;
            currentFrameRealtime = Time.realtimeSinceStartup;

            // 记录跳跃按下时间（用于跳跃缓冲判定）
            if (jumpAction.WasPressedThisFrame())
            {
                lastJumpPressTime = Time.time;
            }
        }

        protected virtual void OnEnable() => actions?.Enable();
        protected virtual void OnDisable() => actions?.Disable();

        protected virtual void Awake()
        {
            CacheActions();
            mainCamera = Camera.main;
        }

        /// <summary>从 InputActionAsset 中查找并缓存各个动作引用</summary>
        protected virtual void CacheActions()
        {
            moveAction = actions["Move"];     // 对应 InputActions 中的 Move 动作
            aimAction = actions["Look"];      // 对应 Look 动作（鼠标 delta）
            jumpAction = actions["Jump"];     // 对应 Jump 动作
            dodgeAction = actions["Sprint"];  // 复用 Sprint 动作作为闪避输入
        }

        // ========== 移动输入 ==========

        /// <summary>获取原始移动输入值（Vector2，X=左右，Y=前后）</summary>
        public virtual Vector2 GetMoveInput()
        {
            return moveAction.ReadValue<Vector2>();
        }

        /// <summary>
        /// 获取相机相对的世界空间移动方向
        /// 将屏幕空间的 WASD 输入旋转到主相机的水平朝向，实现"相对于画面的移动"
        /// 例：按 W 时角色向相机前方移动，而非世界 Z 轴
        /// </summary>
        public virtual Vector3 GetWorldMoveDirection()
        {
            var input = GetMoveInput();
            var deadzone = InputSystem.settings.defaultDeadzoneMin;

            // 死区内返回零向量，防止手柄摇杆漂移
            if (Mathf.Abs(input.x) <= deadzone && Mathf.Abs(input.y) <= deadzone)
                return Vector3.zero;

            // 死区重映射：将死区外的输入值平滑映射到 [0, 1]
            input.x = Mathf.Abs(input.x) > deadzone ? RemapDeadzone(input.x, deadzone) : 0;
            input.y = Mathf.Abs(input.y) > deadzone ? RemapDeadzone(input.y, deadzone) : 0;

            var direction = new Vector3(input.x, 0, input.y);

            if (mainCamera == null) return direction.normalized;

            // 将方向旋转到相机的水平朝向（仅取 Y 轴旋转分量）
            var cameraYaw = Quaternion.AngleAxis(mainCamera.transform.eulerAngles.y, Vector3.up);
            direction = cameraYaw * direction;

            return direction.normalized;
        }

        // ========== 瞄准输入 ==========

        /// <summary>
        /// 获取鼠标在世界坐标中的位置
        /// 使用射线与 Y=0 地面平面的数学求交（不依赖 Physics.Raycast）
        /// 这样避免射线打到玩家自己的 CharacterController 导致朝向抖动
        /// </summary>
        public virtual Vector3 GetMouseWorldPosition()
        {
            if (mainCamera == null) return transform.position;

            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // 射线与 Y=0 水平平面求交（俯视角地面假设在 Y=0）
            // 公式：t = -ray.origin.y / ray.direction.y
            if (Mathf.Abs(ray.direction.y) > 0.0001f)
            {
                var t = -ray.origin.y / ray.direction.y;
                if (t > 0)
                {
                    return ray.origin + ray.direction * t;
                }
            }

            return transform.position;
        }

        /// <summary>
        /// 获取从玩家位置指向鼠标世界坐标的归一化方向
        /// Y 轴分量归零，仅保留水平方向（俯视角不需要垂直瞄准）
        /// </summary>
        public virtual Vector3 GetAimDirection(Vector3 playerPosition)
        {
            var mouseWorld = GetMouseWorldPosition();
            var direction = mouseWorld - playerPosition;
            direction.y = 0; // 俯视角只关心水平朝向

            return direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.zero;
        }

        /// <summary>当前是否正在使用鼠标瞄准（用于区分鼠标/手柄的输入处理）</summary>
        public virtual bool IsAimingWithMouse()
        {
            if (aimAction.activeControl == null) return false;
            return aimAction.activeControl.device.name.Equals(MouseDeviceName);
        }

        // ========== 跳跃缓冲 ==========

        /// <summary>
        /// 消耗跳跃缓冲 —— 如果在缓冲窗口内有未处理的跳跃输入，返回 true 并清除
        /// 实现原理：记录按下时间，下次查询时判断是否在窗口内
        /// </summary>
        public virtual bool ConsumeJumpBuffer()
        {
            if (lastJumpPressTime != null &&
                Time.time - lastJumpPressTime < JumpBufferDuration)
            {
                lastJumpPressTime = null; // 消耗掉，防止重复触发
                return true;
            }
            return false;
        }

        /// <summary>跳跃键是否在本帧按下</summary>
        public virtual bool GetJumpPressed() => jumpAction.WasPressedThisFrame();

        /// <summary>跳跃键是否在本帧松开（用于变高跳跃截断）</summary>
        public virtual bool GetJumpReleased() => jumpAction.WasReleasedThisFrame();

        /// <summary>闪避键是否在本帧按下</summary>
        public virtual bool GetDodgePressed() => dodgeAction.WasPressedThisFrame();

        // ========== DSP 时间映射（为节奏系统预留） ==========

        /// <summary>
        /// 将硬件输入时间戳映射到音频 DSP 时钟
        /// 公式：InputDspTime = 当前帧DSP时间 + (硬件时间 - 当前帧实时时间)
        /// 用于消除帧率抖动带来的判定误差（最高 16.6ms @ 60FPS）
        /// </summary>
        public virtual double GetInputDspTime(float hardwareTimestamp)
        {
            return currentFrameDspTime + (hardwareTimestamp - currentFrameRealtime);
        }

        // ========== 工具方法 ==========

        /// <summary>死区重映射：将死区外的输入值平滑映射到 [0, 1] 或 [-1, 0]</summary>
        protected static float RemapDeadzone(float value, float deadzone)
        {
            return value > 0
                ? (value - deadzone) / (1 - deadzone)
                : (value + deadzone) / (1 - deadzone);
        }
    }
}
