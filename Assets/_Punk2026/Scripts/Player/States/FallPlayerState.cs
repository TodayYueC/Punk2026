using UnityEngine;

namespace Punk2026.Player.States
{
    /// <summary>
    /// 下落状态 —— 角色在空中且垂直速度 ≤ 0（正在下落或跳起后速度归零）
    /// 每帧逻辑：
    ///   1. 重力（下落阶段使用较大的 fallGravity，实现"快落"手感）
    ///   2. 空中移动
    ///   3. 面向瞄准方向
    ///   4. 多段跳/土狼跳检测（空中仍可跳跃）
    ///   5. 变高跳跃检测
    ///   6. 落地时切换回 Idle
    /// </summary>
    public class FallPlayerState : PlayerState
    {
        protected override void OnEnter(PlayerManager player)
        {
            // 进入下落：可能从 Jump 状态转入，也可能从地面直接掉落
        }

        protected override void OnExit(PlayerManager player)
        {
            // 退出下落：落地时触发
        }

        protected override void OnStep(PlayerManager player)
        {
            // 1. 重力（下落阶段，使用较大的 fallGravity 实现快速落地）
            player.ApplyGravity();
            // 2. 贴地检测
            player.SnapToGround();
            // 3. 空中移动（允许空中走位微调）
            player.AccelerateToInput();
            // 4. 面向移动方向
            player.FaceMoveDirection();
            // 5. 空中跳跃（多段跳 + 土狼跳 + 跳跃缓冲）
            player.TryJump();
            // 6. 变高跳跃（如果在空中按着跳跃键松开，截断速度）
            player.HandleVariableHeightJump();

            // 7. 落地检测 → 回到待机状态（跳跃计数由 OnGroundEnter 事件自动重置）
            if (player.isGrounded)
            {
                player.stateManager.Change<IdlePlayerState>();
            }
        }

        public override void OnContact(PlayerManager player, Collider other)
        {
            // 下落碰撞处理（后续可添加踩踏敌人伤害等）
        }
    }
}
