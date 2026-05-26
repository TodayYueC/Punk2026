using System.Collections.Generic;
using Punk2026.Actor;
using Punk2026.Enemy.States;

namespace Punk2026.Enemy
{
    /// <summary>
    /// 敌人状态机管理器 —— 注册并管理敌人的 3 个状态
    /// 继承自 ActorStateManager&lt;EnemyManager&gt;
    /// 状态顺序（index）：
    ///   0 = Idle   （待机，检测玩家距离）
    ///   1 = Chase  （追踪，NavMeshAgent 追踪玩家）
    ///   2 = Attack （攻击，范围内攻击 + 冷却）
    /// </summary>
    public class EnemyStateManager : ActorStateManager<EnemyManager>
    {
        protected override List<ActorState<EnemyManager>> GetStateList()
        {
            return new List<ActorState<EnemyManager>>
            {
                new IdleEnemyState(),
                new ChaseEnemyState(),
                new AttackEnemyState()
            };
        }
    }
}
