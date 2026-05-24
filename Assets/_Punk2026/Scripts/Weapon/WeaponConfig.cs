using UnityEngine;

namespace Punk2026.Weapon
{
    /// <summary>
    /// 武器配置资产 —— ScriptableObject 驱动的武器参数
    /// 通过 [CreateAssetMenu] 可在编辑器中创建不同武器的配置资产
    /// 手枪和散弹枪共用同一个类，通过不同参数值区分行为
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Punk2026/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        // ========== 基础射击参数（所有武器共用） ==========

        [Header("基础射击参数")]

        /// <summary>单发伤害值</summary>
        public float damage = 25f;

        /// <summary>射击冷却间隔（秒），控制射速</summary>
        public float fireRate = 0.15f;

        /// <summary>子弹飞行速度（单位/秒）</summary>
        public float bulletSpeed = 30f;

        /// <summary>子弹最大存活时间（秒），超时自动回收</summary>
        public float bulletLife = 2f;

        // ========== 散弹枪专用参数 ==========

        [Header("散弹枪射击参数")]

        /// <summary>弹丸数量（手枪=1，散弹枪=5）</summary>
        public int pelletCount = 1;

        /// <summary>扇形散射总角度（度），pelletCount 颗子弹在此角度内均匀分布</summary>
        public float spreadAngle = 30f;

        // ========== 物理反馈 ==========

        [Header("物理反馈")]

        /// <summary>
        /// 射击后坐力 —— 对玩家施加的反向冲力
        /// 手枪=0（无后坐力），散弹枪>0（有推力，辅助位移）
        /// </summary>
        public float recoilForce = 0f;
    }
}
