using UnityEngine;

namespace Punk2026.Enemy
{
    /// <summary>
    /// 敌人配置资产 —— ScriptableObject 驱动的敌人参数
    /// 不同敌人类型（蜂群无人机/重型机甲）创建不同配置资产，共用同一个类
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Punk2026/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        // ========== 基础属性 ==========

        [Header("基础属性")]

        /// <summary>最大生命值</summary>
        public float maxHealth = 100f;

        /// <summary>移动加速度</summary>
        public float moveAcceleration = 10f;

        /// <summary>转向阻力（越大转向越灵敏）</summary>
        public float moveTurningDrag = 20f;

        /// <summary>最大移动速度</summary>
        public float moveTopSpeed = 4f;

        /// <summary>减速（急停）</summary>
        public float moveDeceleration = 20f;

        /// <summary>摩擦（无输入时自然减速）</summary>
        public float moveFriction = 20f;

        // ========== 感知与战斗 ==========

        [Header("感知与战斗")]
        
        /// <summary>检测玩家的半径（进入此范围 → 切换到 Chase 状态）</summary>
        public float detectionRadius = 15f;

        /// <summary>攻击距离（进入此范围 → 切换到 Attack 状态）</summary>
        public float attackRange = 2f;

        /// <summary>单次攻击伤害</summary>
        public float attackDamage = 15f;

        /// <summary>攻击冷却时间（秒）</summary>
        public float attackCooldown = 1.5f;

        // ========== 级联殉爆参数 ==========

        [Header("级联殉爆参数")]

        /// <summary>死亡爆炸波及半径</summary>
        public float explosionRadius = 5f;

        /// <summary>死亡爆炸伤害</summary>
        public float explosionDamage = 50f;
    }
}
