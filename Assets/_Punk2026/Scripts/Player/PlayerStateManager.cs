using System.Collections.Generic;
using Punk2026.Actor;
using Punk2026.Player.States;
using UnityEngine;

namespace Punk2026.Player
{
    /// <summary>
    /// 玩家状态机管理器 —— 注册并管理玩家的 5 个状态
    /// 继承自 ActorStateManager<PlayerManager>，自动关联 PlayerManager 实体
    /// 状态列表顺序决定了 index 值，与 Animator 参数同步
    /// </summary>
    [RequireComponent(typeof(PlayerManager))]
    public class PlayerStateManager : ActorStateManager<PlayerManager>
    {
        /// <summary>
        /// 返回玩家的所有状态实例
        /// 状态顺序（index）：
        ///   0 = Idle    （待机）
        ///   1 = Move    （移动）
        ///   2 = Jump    （跳跃）
        ///   3 = Dodge   （闪避）
        ///   4 = Fall    （下落）
        /// </summary>
        protected override List<ActorState<PlayerManager>> GetStateList()
        {
            return new List<ActorState<PlayerManager>>
            {
                new IdlePlayerState(),
                new MovePlayerState(),
                new JumpPlayerState(),
                new DodgePlayerState(),
                new FallPlayerState()
            };
        }
    }
}
