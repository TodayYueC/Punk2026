using UnityEngine;
using UnityEngine.Pool;

namespace Punk2026.Weapon
{
    /// <summary>
    /// 投射物（子弹）组件
    /// 所有武器共用同一个 Bullet 类，行为相同：飞行 → 碰撞检测 → 造成伤害 → 回收到池
    /// 使用 Raycast 而非 Rigidbody 物理驱动，更可控且不依赖物理帧率
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        // ========== 子弹数据 ==========

        /// <summary>飞行速度向量（方向 × 速度大小）</summary>
        private Vector3 velocity;

        /// <summary>单发伤害值</summary>
        private float damage;

        /// <summary>最大存活时间（秒），超时自动回收防止内存泄漏</summary>
        private float maxLifeTime;

        /// <summary>已存活时间计时器</summary>
        private float lifeTimer;

        /// <summary>所属对象池引用，用于回收时调用 pool.Release(this)</summary>
        private IObjectPool<Bullet> bulletPool;

        // ========== 碰撞配置 ==========

        /// <summary>碰撞检测层级掩码，只检测指定层（如敌人、地形），避免打到玩家自己</summary>
        [Header("碰撞筛选")]
        [SerializeField] private LayerMask hitLayers;

        // ========== 初始化 ==========

        /// <summary>
        /// 子弹初始化 —— 从对象池取出后由 WeaponManager 调用
        /// 设置飞行方向、伤害、存活时间和所属池引用
        /// </summary>
        /// <param name="direction">飞行方向（会归一化）</param>
        /// <param name="damage">伤害值</param>
        /// <param name="maxLifeTime">最大存活时间</param>
        /// <param name="bulletPool">所属对象池（回收时需要）</param>
        public void Init(Vector3 direction, float damage,float speed, float maxLifeTime, IObjectPool<Bullet> bulletPool)
        {
            this.damage = damage;
            this.maxLifeTime = maxLifeTime;
            this.bulletPool = bulletPool;
            velocity = direction.normalized * speed;
            lifeTimer = 0f;
        }

        // ========== 每帧更新 ==========

        /// <summary>
        /// 每帧逻辑：
        ///   1. 递增存活计时器，超时则回收
        ///   2. 计算本帧移动距离
        ///   3. 向前方 Raycast 检测碰撞
        ///   4. 命中 → 处理碰撞 → 回收
        ///   5. 未命中 → 继续移动
        /// </summary>
        public void Update()
        {
            // 存活时间检查
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifeTime) ReturnToPool();

            // 计算本帧移动距离和方向
            float moveDistance = velocity.magnitude * Time.deltaTime;
            Vector3 direction = velocity.normalized;

            // 向前方发射射线检测碰撞（距离 = 本帧移动距离，防止穿透）
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDistance, hitLayers))
            {
                HandleCollision(hit);
                return;
            }

            // 未命中 → 正常移动
            transform.position += velocity * Time.deltaTime;
        }

        // ========== 碰撞处理 ==========

        /// <summary>
        /// 处理碰撞结果
        /// TODO: 后续在此处对接 IDamageable 接口造成敌人伤害
        /// </summary>
        private void HandleCollision(RaycastHit hit)
        {
            // TODO: 在此处调用接口伤害敌人
            // 例如：if (hit.collider.TryGetComponent(out IDamageable target)) target.TakeDamage(damage);
            ReturnToPool();
        }

        // ========== 回收 ==========

        /// <summary>
        /// 回收到对象池
        /// 如果池引用存在则安全回收，否则直接销毁（防止池被意外销毁时的空引用）
        /// </summary>
        private void ReturnToPool()
        {
            if (bulletPool != null)
                bulletPool.Release(this);
            else
                Destroy(gameObject);
        }
    }
}
