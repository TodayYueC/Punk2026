using Punk2026.Pool;
using UnityEngine;

namespace Punk2026.Weapon
{
    /// <summary>
    /// 电浆散弹枪 —— 扇形多发武器
    /// 每次射击发射 pelletCount 颗子弹，在 spreadAngle 角度内均匀分布
    /// 射击后有后坐力（recoilForce > 0 时）
    /// 节奏限制（只能在 1、3 拍射击）后续在 D8 中实现
    /// </summary>
    public class PlusmaShotGun : WeaponBase
    {
        /// <summary>
        /// 散弹枪射击逻辑：
        ///   1. 检查冷却
        ///   2. 重置冷却
        ///   3. 计算扇形内每颗子弹的角度偏移
        ///   4. 循环发射 pelletCount 颗子弹
        ///   5. 应用后坐力
        /// </summary>
        public override void TryFire(Vector3 direction)
        {
            // 基础检查
            if (!CanFire()) return;
            ResetCooldownTimer();

            Vector3 baseDirection = direction.normalized;
            int count = config.pelletCount;         // 弹丸数量（默认 5）
            float totalSpread = config.spreadAngle; // 散射总角度（默认 30°）

            // 计算每颗子弹之间的角度间隔
            // 例：5 颗子弹，30° 散射 → 间隔 7.5°，从 -15° 到 +15°
            float angleStep = count > 1 ? totalSpread / (count - 1) : 0f;
            float startAngle = -totalSpread / 2;

            // 遍历发射每颗子弹
            for (int i = 0; i < count; i++)
            {
                // 计算当前子弹的角度偏移
                float currentAngleOffset = startAngle + angleStep * i;

                // 将基础方向绕 Y 轴旋转偏移角度，得到散射方向
                Vector3 rotatedDirection = Quaternion.Euler(0f, currentAngleOffset, 0f) * baseDirection;
                Quaternion bulletRotation = Quaternion.LookRotation(rotatedDirection);

                // 从池中取出子弹
                Bullet bullet = PoolManager.Instance.SpawnBullet(muzzlePoint.position, bulletRotation);

                // 初始化子弹
                bullet.Init(
                    rotatedDirection,
                    config.damage,
                    config.bulletSpeed,
                    config.bulletLife,
                    PoolManager.Instance.GetBulletPool()
                );
            }

            // 后坐力（recoilForce > 0 时才施加）
            if (config.recoilForce > 0)
            {
                ApplyRecoil(baseDirection);
            }
        }

        /// <summary>
        /// 施加后坐力 —— 向射击反方向推动玩家
        /// 由 PlayerManager 接收并应用到水平速度上
        /// 目前为空实现，后续由调用方决定如何施加（不直接修改玩家速度）
        /// </summary>
        /// <param name="baseDirection">射击方向（取反即为后坐力方向）</param>
        private void ApplyRecoil(Vector3 baseDirection)
        {
            Vector3 recoilDirection = -baseDirection;
            // TODO: 后续在此处添加后坐力施加逻辑
            // 例如：PlayerManager.horizontalVelocity += recoilDirection * config.recoilForce;
            Debug.Log("ApplyRecoil:" + recoilDirection);
        }
    }
}
