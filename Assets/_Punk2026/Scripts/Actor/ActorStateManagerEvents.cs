using System;
using UnityEngine.Events;

namespace Punk2026.Actor
{
    /// <summary>
    /// 状态机事件容器 —— 记录状态切换相关的事件
    /// onChange 用于触发动画状态机的 Trigger
    /// onEnter/onExit 携带状态类型信息，用于调试和动画过渡判定
    /// </summary>
    [Serializable]
    public class ActorStateManagerEvents
    {
        /// <summary>状态切换事件（不携带参数，用于通用触发）</summary>
        public UnityEvent onChange;

        /// <summary>进入新状态事件（携带状态的 Type 信息）</summary>
        public UnityEvent<Type> onEnter;

        /// <summary>退出旧状态事件（携带状态的 Type 信息）</summary>
        public UnityEvent<Type> onExit;
    }
}
