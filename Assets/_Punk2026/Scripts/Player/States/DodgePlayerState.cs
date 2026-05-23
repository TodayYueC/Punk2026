using UnityEngine;

namespace Punk2026.Player.States
{
    /// <summary>
    /// 闪避状态 —— 角色执行冲刺闪避时的短暂状态
    /// 进入时锁定闪避方向和速度，持续时间内逐渐减速
    /// 闪避期间不响应移动/跳跃输入（输入锁定）
    /// 时间结束后根据是否有移动输入切换到 Move 或 Idle
    ///
    /// 注意：无敌帧（I-Frames）和体力消耗逻辑暂未实现，后续在战斗系统中补充
    /// </summary>
    public class DodgePlayerState : PlayerState
    {
        /// <summary>闪避已持续时间（秒）</summary>
        protected float dodgeTimer;

        /// <summary>闪避前的水平速度大小，闪避结束后恢复</summary>
        protected float preDodgeSpeed;

        protected override void OnEnter(PlayerManager player)
        {
            dodgeTimer = 0f;
            // 保存闪避前的速度，闪避结束后恢复
            preDodgeSpeed = player.horizontalVelocity.magnitude;
            // 进入闪避时，将水平速度设为闪避方向 × 闪避速度（瞬间爆发）
            player.horizontalVelocity = player.dodgeDirection * player.config.dodgeSpeed;
        }

        protected override void OnExit(PlayerManager player)
        {
            // 退出闪避时，恢复闪避前的速度
            player.horizontalVelocity = player.dodgeDirection * preDodgeSpeed;
        }

        protected override void OnStep(PlayerManager player)
        {
            dodgeTimer += Time.deltaTime;

            // 闪避期间仍需处理物理
            player.SnapToGround();
            player.ApplyGravity();

            // 闪避速度随时间衰减（从 dodgeSpeed 减到闪避前的速度）
            var t = dodgeTimer / player.config.dodgeDuration;
            var currentSpeed = Mathf.Lerp(player.config.dodgeSpeed, preDodgeSpeed, t);
            player.horizontalVelocity = player.dodgeDirection * currentSpeed;

            // 闪避持续时间结束 → 根据输入决定下一个状态
            if (dodgeTimer >= player.config.dodgeDuration)
            {
                var moveDir = player.input.GetWorldMoveDirection();
                if (moveDir.sqrMagnitude > 0)
                {
                    // 有移动输入 → 进入移动状态
                    player.stateManager.Change<MovePlayerState>();
                }
                else
                {
                    // 无移动输入 → 进入待机状态
                    player.stateManager.Change<IdlePlayerState>();
                }
            }
        }

        public override void OnContact(PlayerManager player, Collider other)
        {
            // 闪避碰撞处理（后续可添加穿过敌人时的感电伤害）
        }
    }
}
