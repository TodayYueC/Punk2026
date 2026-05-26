using UnityEngine;

namespace Punk2026.Enemy.States
{
    /// <summary>
    /// 攻击状态 —— 敌人在攻击范围内对玩家造成伤害
    /// 进入时停止移动，面向玩家，按冷却时间循环攻击
    /// 玩家离开攻击范围时回到 Chase 状态
    /// </summary>
    public class AttackEnemyState : EnemyState
    {
        /// <summary>攻击冷却计时器</summary>
        private float cooldownTimer;

        protected override void OnEnter(EnemyManager enemy)
        {
            // 进入攻击：停止移动，重置冷却
            enemy.Decelerate(enemy.config.moveDeceleration);
            cooldownTimer = 0f;
        }

        protected override void OnExit(EnemyManager enemy)
        {
            // 退出攻击：无特殊清理
        }

        protected override void OnStep(EnemyManager enemy)
        {
            if (enemy.playerTransform == null) return;

            // 计算与玩家的实时距离
            float distance = Vector3.Distance(
                enemy.transform.position, enemy.playerTransform.position);

            // 玩家离开攻击范围 → 回到追踪
            if (distance > enemy.config.attackRange)
            {
                enemy.stateManager.Change<ChaseEnemyState>();
                return;
            }

            // 冷却计时
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
            else
            {
                // 冷却完毕 → 执行攻击
                ExecuteAttack(enemy);
                cooldownTimer = enemy.config.attackCooldown; // 重置冷却
            }
        }

        public override void OnContact(EnemyManager enemy, Collider other)
        {
            // 攻击状态无碰撞处理
        }

        /// <summary>
        /// 执行攻击 —— 对玩家造成伤害
        /// TODO: 后续对接 PlayerManager.TakeDamage() 实际扣血
        /// </summary>
        private void ExecuteAttack(EnemyManager enemy)
        {
            // TODO: 敌人攻击逻辑，对接玩家受伤
            Debug.Log($"[Enemy Fire] 敌人攻击，对玩家造成了 {enemy.config.attackDamage} 点伤害！");
        }
    }
}
