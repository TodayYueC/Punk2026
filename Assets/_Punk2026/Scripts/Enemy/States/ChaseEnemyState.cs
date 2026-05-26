using UnityEngine;

namespace Punk2026.Enemy.States
{
    /// <summary>
    /// 追踪状态 —— 使用 NavMeshAgent 追踪玩家
    /// 进入时启用 Agent，退出时停止
    /// 每帧更新 Agent 目标为玩家位置，同时用 Actor 的运动系统控制物理移动
    /// </summary>
    public class ChaseEnemyState : EnemyState
    {
        protected override void OnEnter(EnemyManager enemy)
        {
            // 启用 NavMeshAgent 寻路
            enemy.agent.enabled = true;
        }

        protected override void OnExit(EnemyManager enemy)
        {
            // 退出追踪：停止 Agent 寻路
            if (enemy.agent.isActiveAndEnabled)
            {
                enemy.agent.ResetPath();
            }
        }

        protected override void OnStep(EnemyManager enemy)
        {
            if (enemy.playerTransform == null) return;

            // 计算与玩家的实时距离
            float distance = Vector3.Distance(
                enemy.transform.position, enemy.playerTransform.position);

            // 玩家逃离检测范围 → 退回待机
            if (distance > enemy.config.detectionRadius)
            {
                enemy.stateManager.Change<IdleEnemyState>();
                return;
            }

            // 玩家进入攻击范围 → 切换到攻击
            if (distance <= enemy.config.attackRange)
            {
                enemy.stateManager.Change<AttackEnemyState>();
                return;
            }

            // 设置 Agent 目标为玩家位置
            enemy.agent.SetDestination(enemy.playerTransform.position);

            // 用 Agent 的期望速度驱动 Actor 的运动系统（物理移动）
            Vector3 desiredDirection = enemy.agent.desiredVelocity.normalized;
            enemy.Accelerate(desiredDirection);

            // 关键：将 Agent 的虚拟坐标同步回 CharacterController 的实际位置
            // 防止 Agent 和物理位置脱节导致"瞬移"或"滑步"
            enemy.agent.nextPosition = enemy.transform.position;
        }

        public override void OnContact(EnemyManager enemy, Collider other)
        {
            // 追踪状态无碰撞处理
        }
    }
}
