using UnityEngine;

namespace Punk2026.Player.States
{
    /// <summary>
    /// 待机状态 —— 玩家无移动输入时的默认状态
    /// 每帧逻辑：
    ///   1. 跳跃/闪避/射击检测
    ///   2. 贴地和重力维持
    ///   3. 面向移动方向（射击瞬间短暂面向瞄准方向，由 TryFire 锁定）
    ///   4. 有移动输入时切换到 Move 状态
    /// </summary>
    public class IdlePlayerState : PlayerState
    {
        protected override void OnEnter(PlayerManager player)
        {
            // 进入待机：无特殊初始化
        }

        protected override void OnExit(PlayerManager player)
        {
            // 退出待机：无特殊清理
        }

        protected override void OnStep(PlayerManager player)
        {
            // 1. 跳跃检测（含跳跃缓冲和土狼跳）
            player.TryJump();
            // 2. 离地检测（站在边缘可能被推离地面）
            player.CheckFall();
            // 3. 强制贴地，防止下坡弹起
            player.SnapToGround();
            // 4. 重力维持
            player.ApplyGravity();
            // 5. 闪避检测
            player.TryDodge();
            // 5. 射击检测
            player.TryFire();

            // 6. 面向移动方向（射击锁定期间由 TryFire 中的 FaceAimDirection 接管）
            if (!player.isShootingRotationLocked)
            {
                player.FaceMoveDirection();
            }

            // 7. 有移动输入或残余速度时，切换到移动状态
            var moveDir = player.input.GetWorldMoveDirection();
            if (moveDir.sqrMagnitude > 0 || player.horizontalVelocity.sqrMagnitude > 0)
            {
                player.stateManager.Change<MovePlayerState>();
            }
        }

        public override void OnContact(PlayerManager player, Collider other)
        {
            // 待机状态无碰撞处理
        }
    }
}
