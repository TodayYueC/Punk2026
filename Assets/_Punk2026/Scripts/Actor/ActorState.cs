using UnityEngine;
using UnityEngine.Events;

namespace Punk2026.Actor
{
    /// <summary>
    /// 泛型实体状态抽象基类
    /// 实现状态模式（State Pattern），定义状态的完整生命周期：
    ///   Enter → Step（每帧） → Exit
    ///   以及 OnContact（碰撞回调）
    /// T 约束为 Actor 子类，确保状态能以强类型方式访问具体实体
    /// </summary>
    /// <typeparam name="T">状态所属的实体类型</typeparam>
    public abstract class ActorState<T> where T : Actor<T>
    {
        /// <summary>进入该状态时触发的 Unity 事件（Inspector 可绑定）</summary>
        public UnityEvent onEnter;

        /// <summary>退出该状态时触发的 Unity 事件（Inspector 可绑定）</summary>
        public UnityEvent onExit;

        /// <summary>
        /// 进入状态 —— 先触发事件，再执行子类逻辑
        /// 由状态机在切换时调用，外部不应直接调用
        /// </summary>
        public void Enter(T actor)
        {
            onEnter?.Invoke();
            OnEnter(actor);
        }

        /// <summary>进入状态时的子类初始化逻辑（动画播放、计时器重置等）</summary>
        protected abstract void OnEnter(T actor);

        /// <summary>
        /// 退出状态 —— 先触发事件，再执行子类清理
        /// 由状态机在切换时调用
        /// </summary>
        public void Exit(T actor)
        {
            onExit?.Invoke();
            OnExit(actor);
        }

        /// <summary>退出状态时的子类清理逻辑（停止动画、重置标志等）</summary>
        protected abstract void OnExit(T actor);

        /// <summary>每帧执行 —— 由状态机的 Step() 驱动</summary>
        public void Step(T actor)
        {
            OnStep(actor);
        }

        /// <summary>每帧的核心逻辑（输入检测、状态转换判定、运动控制等）</summary>
        protected abstract void OnStep(T actor);

        /// <summary>碰撞回调 —— 与 CharacterController 的 OnControllerColliderHit 联动</summary>
        public abstract void OnContact(T actor, Collider other);
    }
}
