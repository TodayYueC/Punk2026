using System.Collections.Generic;
using UnityEngine;

namespace Punk2026.Player
{
    /// <summary>
    /// 玩家动画控制器 —— 将实体状态和运动数据桥接到 Animator
    /// Animator 参数（与 Animator Controller 中一一对应）：
    ///   - Jump (bool)        : 是否在跳跃中（jumpCounter > 0 且未落地）
    ///   - Grounded (bool)    : 是否在地面上
    ///   - FreeFall (bool)    : 是否在自由下落（非地面 + 向下速度）
    ///   - Speed (float)      : 水平移动速度
    ///   - MotionSpeed (float): 动画播放速率
    /// </summary>
    [RequireComponent(typeof(PlayerManager))]
    public class PlayerAnimator : MonoBehaviour
    {
        [System.Serializable]
        public class ForcedTransition
        {
            public int fromStateIndex;
            public int animationLayer;
            public string toAnimationState;
        }

        [Header("Settings")]
        /// <summary>动画播放速度下限，防止低速时动画抖动</summary>
        public float minLateralAnimationSpeed = 0.5f;

        /// <summary>强制过渡配置列表</summary>
        public List<ForcedTransition> forcedTransitions;

        /// <summary>Animator 组件引用（不赋值时自动从子物体查找）</summary>
        public Animator animator;

        protected Dictionary<int, ForcedTransition> forcedTransitionMap;
        protected PlayerManager owner;

        // ========== Animator 参数名（必须与 Animator Controller 中的参数名完全一致） ==========
        public string jumpParam = "Jump";
        public string groundedParam = "Grounded";
        public string freeFallParam = "FreeFall";
        public string speedParam = "Speed";
        public string motionSpeedParam = "MotionSpeed";

        // ========== 缓存的 Hash ==========
        protected int jumpHash;
        protected int groundedHash;
        protected int freeFallHash;
        protected int speedHash;
        protected int motionSpeedHash;

        protected virtual void Start()
        {
            InitOwner();
            InitAnimator();
            InitForcedTransitions();
            InitParameterHashes();
        }

        protected virtual void LateUpdate()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            UpdateAnimatorParameters();
        }

        /// <summary>每帧同步运动数据到 Animator</summary>
        protected virtual void UpdateAnimatorParameters()
        {
            var hSpeed = owner.horizontalVelocity.magnitude;
            var animSpeed = Mathf.Max(hSpeed / owner.config.moveTopSpeed, minLateralAnimationSpeed);

            // Jump: 正在跳跃中（跳跃计数 > 0 且还没落地）
            var isJumping = owner.jumpCounter > 0 && !owner.isGrounded;
            animator.SetBool(jumpHash, isJumping);

            // Grounded: 是否在地面上
            animator.SetBool(groundedHash, owner.isGrounded);

            // FreeFall: 不在地面上且正在下落（垂直速度向下）
            var isFreeFall = !owner.isGrounded && owner.velocity.y < 0;
            animator.SetBool(freeFallHash, isFreeFall);

            // Speed: 水平移动速度
            animator.SetFloat(speedHash, hSpeed);

            // MotionSpeed: 动画播放速率
            animator.SetFloat(motionSpeedHash, animSpeed);
        }

        protected virtual void InitOwner()
        {
            owner = GetComponent<PlayerManager>();
            owner.stateManager.events.onChange.AddListener(HandleForcedTransition);
        }

        protected virtual void InitAnimator()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                Debug.LogWarning($"[PlayerAnimator] {gameObject.name} 上没有找到 Animator 组件，动画将不会播放。");
            }
        }

        protected virtual void InitForcedTransitions()
        {
            forcedTransitionMap = new Dictionary<int, ForcedTransition>();
            if (forcedTransitions == null) return;

            foreach (var transition in forcedTransitions)
            {
                if (!forcedTransitionMap.ContainsKey(transition.fromStateIndex))
                {
                    forcedTransitionMap.Add(transition.fromStateIndex, transition);
                }
            }
        }

        protected virtual void HandleForcedTransition()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;

            var lastIndex = owner.stateManager.lastIndex;
            if (forcedTransitionMap.ContainsKey(lastIndex))
            {
                var entry = forcedTransitionMap[lastIndex];
                animator.Play(entry.toAnimationState, entry.animationLayer);
            }
        }

        protected virtual void InitParameterHashes()
        {
            jumpHash = Animator.StringToHash(jumpParam);
            groundedHash = Animator.StringToHash(groundedParam);
            freeFallHash = Animator.StringToHash(freeFallParam);
            speedHash = Animator.StringToHash(speedParam);
            motionSpeedHash = Animator.StringToHash(motionSpeedParam);
        }
    }
}
