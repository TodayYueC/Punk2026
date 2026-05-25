using UnityEngine;

namespace Punk2026.Weapon
{
    /// <summary>
    /// 玩家武器管理器 —— 管理玩家持有的所有武器
    /// 职责：
    ///   1. 持有武器数组（Inspector 中拖入）
    ///   2. 管理武器切换（激活/隐藏对应的武器 GameObject）
    ///   3. 委托射击给当前激活的武器
    /// </summary>
    public class PlayerWeaponManager : MonoBehaviour
    {
        // ========== 武器库 ==========

        /// <summary>武器数组（Inspector 中拖入所有武器组件）</summary>
        [Header("武器库")]
        [SerializeField] private WeaponBase[] weapons;

        /// <summary>当前激活的武器索引</summary>
        private int currentWeaponIndex = 0;

        /// <summary>当前武器的便捷访问器</summary>
        public WeaponBase currentWeapon => weapons != null && weapons.Length > 0 ? weapons[currentWeaponIndex] : null;

        // ========== 初始化 ==========

        public void Start()
        {
            InitWeaponInventory();
        }

        /// <summary>
        /// 初始化武器库 —— 只激活当前武器，隐藏其他武器
        /// 通过 SetActive 切换武器 GameObject 的显示/隐藏
        /// </summary>
        private void InitWeaponInventory()
        {
            if (weapons == null || weapons.Length == 0) return;
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    weapons[i].gameObject.SetActive(i == currentWeaponIndex);
                }
            }
        }

        // ========== 射击接口 ==========

        /// <summary>
        /// 委托射击给当前武器
        /// 由 PlayerManager.TryFire() 调用，PlayerWeaponManager 本身不做射击判定
        /// </summary>
        /// <param name="aimDirection">瞄准方向（归一化）</param>
        public void TryFireCurrentWeapon(Vector3 aimDirection)
        {
            if (currentWeapon != null)
            {
                currentWeapon.TryFire(aimDirection);
            }
        }

        // ========== 武器切换 ==========

        /// <summary>
        /// 切换到下一把武器（循环切换）
        /// 隐藏当前武器 → 索引+1 → 激活新武器
        /// </summary>
        public void SwitchNextWeapon()
        {
            if (weapons == null || weapons.Length == 0) return;

            // 隐藏当前武器
            weapons[currentWeaponIndex].gameObject.SetActive(false);

            // 循环递增索引
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;

            // 激活新武器
            weapons[currentWeaponIndex].gameObject.SetActive(true);
            Debug.Log("切换至武器:" + currentWeaponIndex);
        }
    }
}
