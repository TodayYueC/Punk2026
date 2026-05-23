using UnityEngine;

namespace Punk2026.Player
{
    /// <summary>
    /// 俯视角相机控制器
    /// 相机位置 = 玩家位置 + 基于 pitch 计算的后方偏移 + 高度偏移
    /// 保持玩家在屏幕中心，Inspector 中可调所有参数
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        [Header("Follow Target")]
        public PlayerManager target;

        [Header("Camera Settings")]
        /// <summary>相机离地面的高度</summary>
        public float height = 14f;

        /// <summary>俯仰角（0=水平，90=正下方）</summary>
        [Range(0, 90)]
        public float pitch = 55f;

        [Header("Smoothing")]
        /// <summary>跟随平滑时间（越小越紧跟）</summary>
        public float followDamping = 0.1f;

        protected Vector3 currentVelocity;

        protected virtual void Start()
        {
            if (target == null)
            {
                target = FindFirstObjectByType<PlayerManager>();
            }

            SnapToTarget();
        }

        protected virtual void LateUpdate()
        {
            if (target == null) return;

            HandleFollow();
        }

        /// <summary>
        /// 平滑跟随玩家
        /// 相机位置 = 玩家位置 + 向上偏移(height) + 向后偏移(由pitch计算)
        /// 这样 pitch 角度下玩家始终在屏幕中心
        /// </summary>
        protected virtual void HandleFollow()
        {
            var pitchRad = pitch * Mathf.Deg2Rad;
            // 向后偏移 = height / tan(pitch)，保证视线正好穿过玩家
            var backOffset = height / Mathf.Tan(pitchRad);

            var targetPosition = target.transformPosition
                                 + Vector3.up * height
                                 + Vector3.back * backOffset;

            transform.position = Vector3.SmoothDamp(
                transform.position, targetPosition, ref currentVelocity, followDamping);

            transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>瞬间对准（跳过平滑）</summary>
        public virtual void SnapToTarget()
        {
            if (target == null) return;

            var pitchRad = pitch * Mathf.Deg2Rad;
            var backOffset = height / Mathf.Tan(pitchRad);

            transform.position = target.transformPosition
                                 + Vector3.up * height
                                 + Vector3.back * backOffset;

            transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
            currentVelocity = Vector3.zero;
        }
    }
}
