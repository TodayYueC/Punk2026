using System;
using UnityEngine.Events;

namespace Punk2026.Actor
{
    /// <summary>
    /// 实体事件容器 —— 记录地面相关的生命周期事件
    /// 使用 [Serializable] 使其可在 Inspector 中显示并绑定回调
    /// </summary>
    [Serializable]
    public class ActorEvents
    {
        /// <summary>落地事件 —— 角色从空中落到地面时触发（用于重置跳跃计数、播放落地动画等）</summary>
        public UnityEvent OnGroundEnter;

        /// <summary>离地事件 —— 角色从地面离开时触发（用于播放起跳动画等）</summary>
        public UnityEvent OnGroundExit;
    }
}
