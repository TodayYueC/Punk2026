using UnityEngine;

namespace Punk2026.Player
{
    /// <summary>
    /// 动态地面投影系统 —— 俯视角下辅助高度判断的自研方案
    /// 解决俯视角跳跃时"不知道自己有多高"的痛点
    /// 原理：从角色位置向下发射射线，找到地面后在命中点渲染阴影精灵
    /// 阴影的缩放和透明度随离地高度线性衰减，直到完全消失
    /// PRD-01.2-4: 阴影始终贴合地表，Scale = BaseScale × (MaxHeight - h) / MaxHeight
    /// </summary>
    public class PlayerShadow : MonoBehaviour
    {
        [Header("Shadow Settings")]
        /// <summary>阴影 Transform（需要预先创建并设置 SpriteRenderer）</summary>
        public Transform shadowTransform;

        /// <summary>阴影可见的最大高度（超过此高度阴影完全消失）</summary>
        public float maxShadowHeight = 10f;

        /// <summary>基础缩放（地面上时的阴影大小）</summary>
        public float baseScale = 1.0f;

        /// <summary>地面偏移量，防止阴影与地面 Z-Fighting 闪烁</summary>
        public float groundOffset = 0.01f;

        [Header("References")]
        /// <summary>关联的玩家管理器</summary>
        public PlayerManager owner;

        /// <summary>阴影的 SpriteRenderer（用于修改透明度）</summary>
        protected SpriteRenderer shadowRenderer;

        protected virtual void Start()
        {
            if (owner == null)
            {
                owner = GetComponentInParent<PlayerManager>();
            }

            if (shadowTransform != null)
            {
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();
                // 将阴影从玩家层级中独立出来，不跟随玩家旋转
                shadowTransform.SetParent(null);
            }
        }

        /// <summary>在 LateUpdate 中更新阴影（确保在玩家移动之后）</summary>
        protected virtual void LateUpdate()
        {
            if (shadowTransform == null || owner == null) return;

            UpdateShadow();
        }

        /// <summary>
        /// 更新阴影位置和外观
        /// 算法：
        ///   1. 从玩家位置向下发射射线
        ///   2. 命中地面 → 阴影放置在命中点，缩放和透明度按高度比例计算
        ///   3. 未命中   → 阴影缩放为零（隐藏）
        /// </summary>
        protected virtual void UpdateShadow()
        {
            var ray = new Ray(owner.transformPosition, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, maxShadowHeight))
            {
                // 阴影放置在地面命中点（加微小偏移防止 Z-Fighting）
                shadowTransform.position = hit.point + Vector3.up * groundOffset;
                // 阴影朝向与地面法线对齐（支持非平面地面）
                shadowTransform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                // 计算离地高度
                var heightDiff = owner.transformPosition.y - hit.point.y;
                // 高度比例：地面时 = 1，最大高度时 = 0
                var ratio = Mathf.Clamp01((maxShadowHeight - heightDiff) / maxShadowHeight);

                // 缩放随高度线性衰减
                shadowTransform.localScale = Vector3.one * (baseScale * ratio);

                // 透明度随高度线性衰减（最大 0.8，防止完全不透明时过于突兀）
                if (shadowRenderer != null)
                {
                    var color = shadowRenderer.color;
                    color.a = ratio * 0.8f;
                    shadowRenderer.color = color;
                }
            }
            else
            {
                // 未检测到地面 → 隐藏阴影
                shadowTransform.localScale = Vector3.zero;
            }
        }

        /// <summary>销毁时清理独立的阴影 GameObject</summary>
        protected virtual void OnDestroy()
        {
            if (shadowTransform != null)
            {
                Destroy(shadowTransform.gameObject);
            }
        }
    }
}
