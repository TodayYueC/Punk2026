using UnityEngine;

namespace Punk2026.Player
{
    /// <summary>
    /// 玩家配置资产 —— ScriptableObject 驱动的所有玩家参数
    /// 通过 [CreateAssetMenu] 可在 Unity 编辑器中创建配置资产文件
    /// 修改资产数值即可实时调整角色手感，无需修改代码
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Punk2026/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        // ========== 通用参数 ==========

        [Header("General")]

        /// <summary>朝向旋转速度（度/秒），PRD 要求 1200°/s</summary>
        public float rotationSpeed = 1200f;

        /// <summary>贴地力 —— 地面上施加的向下力，防止下坡弹起</summary>
        public float snapForce = 15f;

        // ========== 地面运动参数 ==========

        [Header("Movement")]

        /// <summary>地面加速度</summary>
        public float moveAcceleration = 13f;

        /// <summary>地面减速度（急停刹车）</summary>
        public float moveDeceleration = 28f;

        /// <summary>地面摩擦力（无输入时的自然减速）</summary>
        public float moveFriction = 28f;

        /// <summary>地面最大移动速度</summary>
        public float moveTopSpeed = 6f;

        /// <summary>地面转向阻力 —— 越大转向越灵敏，越小转向越飘</summary>
        public float moveTurningDrag = 28f;

        /// <summary>空中加速度（通常比地面大，允许空中微调走位）</summary>
        public float airAcceleration = 32f;

        /// <summary>刹车判定阈值 —— 当输入方向与速度方向的点积小于此值时触发刹车</summary>
        public float brakeThreshold = -0.8f;

        // ========== 跳跃参数 ==========

        [Header("Jump")]

        /// <summary>额外跳跃次数（0 = 只能跳一次，1 = 二段跳，2 = 三段跳...）</summary>
        public int extraJumps = 1;

        /// <summary>土狼跳窗口（秒）—— 离地后仍可跳跃的时间，解决"边缘掉落后无法跳"的问题</summary>
        public float coyoteTimeWindow = 0.15f;

        /// <summary>最大跳跃力（长按跳跃键达到的高度）</summary>
        public float maxJumpForce = 17f;

        /// <summary>最小跳跃力（短按跳跃键截断后的高度）</summary>
        public float minJumpForce = 10f;

        /// <summary>上升阶段重力（较小，跳起较慢）</summary>
        public float gravity = 38f;

        /// <summary>下落阶段重力（较大，落下较快，实现"快落"手感）</summary>
        public float fallGravity = 65f;

        /// <summary>最大下落速度限制</summary>
        public float maxFallSpeed = 50f;

        // ========== 闪避参数 ==========

        [Header("Dodge")]

        /// <summary>闪避冲刺速度</summary>
        public float dodgeSpeed = 25f;

        /// <summary>闪避持续时间（秒）</summary>
        public float dodgeDuration = 0.2f;

        /// <summary>闪避冷却时间（秒）</summary>
        public float dodgeCooldown = 0.8f;
    }
}
