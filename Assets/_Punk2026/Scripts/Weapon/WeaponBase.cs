using UnityEngine;

namespace Punk2026.Weapon
{
    /// <summary>
    /// 武器抽象基类 —— 所有武器的公共逻辑
    /// 职责：
    ///   1. 持有 WeaponConfig 配置引用（Inspector 中拖入）
    ///   2. 管理射击冷却计时器
    ///   3. 定义抽象 TryFire 方法，由子类实现各自的射击逻辑
    /// 子类：PulsePistal（手枪）、PlusmaShotGun（散弹枪）
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        // ========== 配置与引用 ==========

        /// <summary>武器配置资产（伤害、射速、弹速等参数）</summary>
        [Header("武器资产")]
        [SerializeField] protected WeaponConfig config;

        /// <summary>枪口位置（子弹从这里生成）</summary>
        [Header("枪口火花位置")]
        [SerializeField] protected Transform muzzlePoint;

        // ========== 冷却系统 ==========

        /// <summary>冷却计时器（>0 时不能射击）</summary>
        protected float cooldownTimer;

        /// <summary>配置的公开访问器</summary>
        public WeaponConfig weaponConfig => config;

        /// <summary>每帧递减冷却计时器</summary>
        protected void Update()
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        }

        // ========== 射击接口 ==========

        /// <summary>尝试射击（由子类实现具体逻辑）</summary>
        /// <param name="direction">瞄准方向</param>
        public abstract void TryFire(Vector3 direction);

        /// <summary>是否冷却完毕可以射击</summary>
        public bool CanFire() => cooldownTimer <= 0f;

        /// <summary>重置冷却计时器（射击后调用）</summary>
        protected void ResetCooldownTimer() => cooldownTimer = config.fireRate;
    }
}
