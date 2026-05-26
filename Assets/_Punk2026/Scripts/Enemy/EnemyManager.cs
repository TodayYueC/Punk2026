using Punk2026.Actor;
using UnityEngine;
using UnityEngine.AI;

namespace Punk2026.Enemy
{
    /// <summary>
    /// 敌人管理器 —— 所有敌人的核心类
    /// 继承自 Actor&lt;EnemyManager&gt;，复用运动系统（CharacterController、地面检测、加减速）
    /// 职责：生命值管理、受击、死亡、级联殉爆、NavMeshAgent 寻路
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyManager : Actor<EnemyManager>
    {
        // ========== 配置与引用 ==========

        /// <summary>敌人配置资产（HP/速度/攻击/爆炸参数）</summary>
        [Header("资产配置")]
        [SerializeField] private EnemyConfig enemyConfig;

        /// <summary>敌人层级掩码（用于 OverlapSphere 只检测敌人，不检测玩家）</summary>
        [SerializeField] private LayerMask enemyLayer;

        // ========== 生命状态 ==========

        /// <summary>当前生命值</summary>
        private float currentHealth;

        /// <summary>死亡互斥锁 —— 为 true 时不再接受伤害，防止级联爆炸递归</summary>
        public bool bIsDead { get; private set; }

        // ========== 碰撞检测缓存 ==========

        /// <summary>
        /// 静态碰撞体数组缓存 —— 用于 OverlapSphereNonAlloc，避免每次分配新数组产生 GC
        /// 最多同时检测 30 个碰撞体
        /// </summary>
        private readonly static Collider[] hitColliders = new Collider[30];

        // ========== 公开属性 ==========

        /// <summary>配置的公开访问器</summary>
        public EnemyConfig config => enemyConfig;

        /// <summary>NavMeshAgent 组件引用</summary>
        public NavMeshAgent agent { get; private set; }

        /// <summary>玩家 Transform 引用（追踪目标）</summary>
        public Transform playerTransform { get; private set; }

        // ========== 初始化 ==========

        /// <summary>
        /// 初始化：获取 NavMeshAgent，禁用其自动更新（由 Actor 的运动系统接管）
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false; // 朝向由 Actor 的 FaceDirection 控制
            agent.updatePosition = false; // 位置由 Actor 的 CharacterController 控制
        }

        /// <summary>初始化生命值，查找玩家引用</summary>
        private void Start()
        {
            currentHealth = enemyConfig.maxHealth;
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            playerTransform = playerObj.transform;
        }

        // ========== 受击与死亡 ==========

        /// <summary>
        /// 受击 —— 扣除生命值，HP 归零时触发死亡
        /// 如果 bIsDead 为 true（已死亡），直接返回，防止级联爆炸递归
        /// </summary>
        public override void TakeDamage(float damage)
        {
            if (bIsDead) return; // 互斥锁：已死亡的敌人不再接受伤害

            currentHealth -= damage;
            Debug.Log("受伤，剩余血量" + currentHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        /// <summary>
        /// 死亡 —— 设置死亡锁 → 触发级联爆炸 → 销毁 GameObject
        /// </summary>
        public override void Die()
        {
            Debug.Log("死亡");
            bIsDead = true;
            ExecuteDieAoe(); // 先爆炸再销毁，确保爆炸效果可见
            Destroy(gameObject);
        }

        // ========== 级联殉爆 ==========

        /// <summary>
        /// 死亡爆炸 —— 搜索爆炸范围内所有敌人，对它们造成 AOE 伤害
        /// 使用 OverlapSphereNonAlloc 避免 GC（复用静态 hitColliders 数组）
        ///
        /// 防递归原理：
        ///   A 死亡 → A.bIsDead = true → A 爆炸 → B.TakeDamage()
        ///   → B 死亡 → B.bIsDead = true → B 爆炸 → A.TakeDamage()
        ///   → A.bIsDead == true → 直接返回，不递归
        /// </summary>
        private void ExecuteDieAoe()
        {
            // 非分配版本的 OverlapSphere，结果写入预分配的静态数组
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                enemyConfig.explosionRadius,
                hitColliders,
                enemyLayer // 只检测敌人层，不检测玩家
            );

            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitColliders[i];

                // 跳过自己 + 只对有 EnemyManager 的物体造成伤害
                if (hit.gameObject != this.gameObject &&
                    hit.TryGetComponent<EnemyManager>(out var companionMesh))
                {
                    companionMesh.TakeDamage(enemyConfig.explosionDamage);
                }

                // 清空引用，防止下次调用时残留旧数据
                hitColliders[i] = null;
            }
        }

        // ========== 运动封装 ==========

        /// <summary>
        /// 敌人加速 —— 封装配置参数，简化状态类调用
        /// 复用 Actor 基类的 4 参数加速算法
        /// </summary>
        public virtual void Accelerate(Vector3 direction)
        {
            Accelerate(direction, enemyConfig.moveTurningDrag, enemyConfig.moveAcceleration, enemyConfig.moveTopSpeed);
        }
    }
}
