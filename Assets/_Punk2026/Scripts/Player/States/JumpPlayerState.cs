using UnityEngine;

namespace Punk2026.Player.States
{
    /// <summary>
    /// 跳跃状态 —— 角色正在上升阶段（垂直速度 > 0）
    /// 每帧逻辑：
    ///   1. 施加重力（上升阶段使用较小的 gravity）
    ///   2. 变高跳跃检测（松开跳跃键截断速度）
    ///   3. 空中移动（允许空中微调走位）
    ///   4. 面向瞄准方向
    ///   5. 垂直速度 ≤ 0 时切换到 Fall 状态
    /// </summary>
    public class JumpPlayerState : PlayerState
    {
        protected override void OnEnter(PlayerManager player)
        {
            // 进入跳跃：跳跃已在 PlayerManager.ExecuteJump 中执行
        }

        protected override void OnExit(PlayerManager player)
        {
            // 退出跳跃：无特殊清理
        }

        protected override void OnStep(PlayerManager player)
        {
            // 1. 重力（上升阶段，使用较小的 gravity 实现缓慢上升）
            player.ApplyGravity();
            // 2. 变高跳跃：松开 Space 时截断上升速度
            player.HandleVariableHeightJump();
            // 3. 贴地（防止跳起时紧贴地面的边缘情况）
            player.SnapToGround();
            // 4. 空中移动（使用空中加速度参数）
            player.AccelerateToInput();
            // 5. 面向移动方向
            player.FaceMoveDirection();

            // 6. 垂直速度转负 → 进入下落阶段
            if (player.upwardVelocity.y <= 0)
            {
                player.stateManager.Change<FallPlayerState>();
            }

            // 7. 安全网：如果在跳跃状态但实际已落地（极小概率），直接回到待机
            if (player.isGrounded && player.jumpCounter > 0)
            {
                player.stateManager.Change<IdlePlayerState>();
            }
        }

        public override void OnContact(PlayerManager player, Collider other)
        {
            // 跳跃状态无特殊碰撞处理
        }
    }
}
