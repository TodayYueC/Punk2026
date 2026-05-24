using Punk2026.Weapon;
using UnityEngine;
using UnityEngine.Pool;

namespace Punk2026.Pool
{
    /// <summary>
    /// 子弹对象池管理器 —— 全局单例
    /// 使用 UnityEngine.Pool.ObjectPool 管理子弹实例，消除频繁 Instantiate/Destroy 产生的 GC
    /// 所有武器共用同一个子弹池（子弹行为相同，只是参数不同）
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        // ========== 单例 ==========

        /// <summary>全局唯一实例，通过 PoolManager.Instance 访问</summary>
        public static PoolManager Instance { get; private set; }

        // ========== 对象池配置（Inspector 中设置） ==========

        [Header("子弹池配置")]
        /// <summary>子弹预制体引用</summary>
        [SerializeField] private Bullet bulletPrefab;

        /// <summary>池的默认容量（预热时创建的实例数）</summary>
        [SerializeField] private int defaultSize = 100;

        /// <summary>池的最大容量（超出时 Get 会创建新实例但 Release 后会 Destroy）</summary>
        [SerializeField] private int maxSize = 200;

        /// <summary>内部持有的子弹对象池实例</summary>
        private IObjectPool<Bullet> bulletPool;

        // ========== 初始化 ==========

        /// <summary>
        /// 单例初始化 + 创建对象池
        /// 如果已有实例则销毁自身，确保全局唯一
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 创建对象池并预热
        /// 配置四个生命周期回调：创建、取出、回收、销毁
        /// </summary>
        private void InitPool()
        {
            bulletPool = new ObjectPool<Bullet>(
                createFunc: CreateBulletInstance,           // 池中无可用实例时创建新的
                actionOnGet: OnGetBulletFromPool,           // 从池中取出时调用
                actionOnRelease: OnReleaseBulletToPool,     // 回收到池时调用
                actionOnDestroy: OnDestroyBulletInstance,   // 销毁实例时调用
                collectionCheck: true,                      // 检查重复回收（调试用）
                defaultCapacity: defaultSize,               // 初始容量
                maxSize: maxSize                            // 最大容量
            );

            // 预热：预先创建 defaultSize 个实例，避免运行时首次射击的卡顿
            PreWarmPool();
        }

        /// <summary>
        /// 预热对象池 —— 预先创建并回收 defaultSize 个实例
        /// 这样运行时第一次射击不会触发 Instantiate，消除首帧卡顿
        /// </summary>
        private void PreWarmPool()
        {
            Bullet[] tempArray = new Bullet[defaultSize];

            // 全部取出（触发创建）
            for (int i = 0; i < defaultSize; i++)
            {
                tempArray[i] = bulletPool.Get();
            }

            // 全部回收（放回池中）
            for (int i = 0; i < defaultSize; i++)
            {
                bulletPool.Release(tempArray[i]);
            }
        }

        // ========== 对象池生命周期回调 ==========

        #region 对象池回调方法

        /// <summary>创建子弹实例（Instantiate 预制体）</summary>
        private Bullet CreateBulletInstance()
        {
            Bullet bullet = Instantiate(bulletPrefab, transform);
            return bullet;
        }

        /// <summary>从池中取出时调用 —— 激活 GameObject</summary>
        private void OnGetBulletFromPool(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
        }

        /// <summary>回收到池时调用 —— 隐藏 GameObject（停止 Update、节省渲染）</summary>
        private void OnReleaseBulletToPool(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
        }

        /// <summary>销毁实例时调用（池满时多余实例会被销毁）</summary>
        private void OnDestroyBulletInstance(Bullet bullet)
        {
            Destroy(bullet.gameObject);
        }

        #endregion

        // ========== 公共访问接口 ==========

        #region 公共访问接口

        /// <summary>
        /// 从池中取出一颗子弹并设置位置和朝向
        /// 武器调用此方法获取子弹，然后调用 bullet.Init() 设置速度和伤害
        /// </summary>
        /// <param name="position">子弹初始位置</param>
        /// <param name="rotation">子弹初始朝向</param>
        /// <returns>可用的子弹实例</returns>
        public Bullet SpawnBullet(Vector3 position, Quaternion rotation)
        {
            Bullet bullet = bulletPool.Get();
            bullet.transform.SetPositionAndRotation(position, rotation);
            return bullet;
        }

        /// <summary>获取子弹池的引用（供 Bullet 自身持有，用于回收）</summary>
        public IObjectPool<Bullet> GetBulletPool() => bulletPool;

        #endregion
    }
}
