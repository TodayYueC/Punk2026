using UnityEngine;

namespace Punk2026.Enemy.States
{
    /// <summary>
    /// 待机状态 —— 敌人的默认状态
    /// 每帧检测玩家是否进入 detectionRadius，进入则切换到 Chase
    /// </summary>
    public class IdleEnemyState : EnemyState
    {
        protected override void OnEnter(EnemyManager enemy)
        {
            // 进入待机：执行减速，让敌人停下来
            enemy.Decelerate(enemy.config.moveDeceleration);
        }

        protected override void OnExit(EnemyManager enemy)
        {
            // 退出待机：无特殊清理
        }

        protected override void OnStep(EnemyManager enemy)
        {
            if (enemy.playerTransform == null) return;

            // 每帧计算与玩家的距离
            float distance = Vector3.Distance(
                enemy.transform.position, enemy.playerTransform.position);

            // 玩家进入检测范围 → 切换到追踪状态
            if (distance <= enemy.config.detectionRadius)
            {
                enemy.stateManager.Change<ChaseEnemyState>();
            }
        }

        public override void OnContact(EnemyManager enemy, Collider other)
        {
            // 待机状态无碰撞处理
        }
    }
}
