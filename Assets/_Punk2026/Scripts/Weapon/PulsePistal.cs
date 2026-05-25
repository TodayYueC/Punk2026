using Punk2026.Pool;
using UnityEngine;

namespace Punk2026.Weapon
{
    /// <summary>
    /// 脉冲手枪 —— 最基础的单发武器
    /// 每次射击发射 1 颗子弹，无散射，无后坐力
    /// 节奏限制（只能在节拍点射击）后续在 D8 中实现
    /// </summary>
    public class PulsePistal : WeaponBase
    {
        /// <summary>
        /// 手枪射击逻辑：
        ///   1. 检查冷却
        ///   2. 重置冷却
        ///   3. 从对象池取出子弹
        ///   4. 初始化子弹（方向、伤害、速度、生命、池引用）
        /// </summary>
        public override void TryFire(Vector3 direction)
        {
            // 冷却检查：未冷却完毕则不开火
            if (!CanFire()) return;

            // 重置冷却计时器
            ResetCooldownTimer();

            // 计算射击方向和子弹朝向
            Vector3 fireDirection = direction.normalized;
            Quaternion bulletRotation = Quaternion.LookRotation(fireDirection);

            // 从对象池取出子弹并设置位置
            Bullet bullet = PoolManager.Instance.SpawnBullet(muzzlePoint.position, bulletRotation);

            // 初始化子弹参数
            bullet.Init(
                fireDirection,
                config.damage,
                config.bulletSpeed,
                config.bulletLife,
                PoolManager.Instance.GetBulletPool()
            );
        }
    }
}
