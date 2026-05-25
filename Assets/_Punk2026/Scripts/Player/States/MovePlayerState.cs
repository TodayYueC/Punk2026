using UnityEngine;

namespace Punk2026.Player.States
{
    /// <summary>
    /// 移动状态 —— 玩家有 WASD 输入时的运动状态
    /// 每帧逻辑：
    ///   1. 射击/跳跃/闪避/重力/贴地/离地检测
    ///   2. 面向移动方向（射击瞬间短暂面向瞄准方向，由 TryFire 锁定）
    ///   3. 有输入时加速（含刹车判定：反向输入时急停）
    ///   4. 无输入时摩擦减速
    ///   5. 速度归零时切换回 Idle
    /// </summary>
    public class MovePlayerState : PlayerState
    {
        protected override void OnEnter(PlayerManager player)
        {
            // 进入移动：无特殊初始化
        }

        protected override void OnExit(PlayerManager player)
        {
            // 退出移动：无特殊清理
        }

        protected override void OnStep(PlayerManager player)
        {
            // 基础检测
            // 开火判定，不打断移动物理位移
            player.TryFire();
            player.TryJump();
            player.SnapToGround();
            player.ApplyGravity();
            player.CheckFall();
            player.TryDodge();
            

            // 面向移动方向（射击锁定期间由 TryFire 中的 FaceAimDirection 接管）
            if (!player.isShootingRotationLocked)
            {
                player.FaceMoveDirection();
            }


            var moveDir = player.input.GetWorldMoveDirection();

            if (moveDir.sqrMagnitude > 0)
            {
                // 有移动输入时：检测是否需要刹车
                // 点积 < brakeThreshold 表示输入方向与当前速度方向几乎相反
                var dot = Vector3.Dot(moveDir, player.horizontalVelocity);
                if (dot >= player.config.brakeThreshold)
                {
                    // 同向或垂直：正常加速
                    player.Accelerate(moveDir);
                }
                else
                {
                    // 反向：执行减速（刹车手感）
                    player.Decelerate();
                }
            }
            else
            {
                // 无移动输入时：施加摩擦力自然减速
                player.ApplyFriction();

                // 速度完全归零时，切换回待机状态
                if (player.horizontalVelocity.sqrMagnitude <= 0)
                {
                    player.stateManager.Change<IdlePlayerState>();
                }
            }
        }

        public override void OnContact(PlayerManager player, Collider other)
        {
            // 移动状态无特殊碰撞处理
        }
    }
}
