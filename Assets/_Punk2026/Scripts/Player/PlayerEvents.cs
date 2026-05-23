using System;
using UnityEngine.Events;

namespace Punk2026.Player
{
    /// <summary>
    /// 玩家事件容器 —— 记录玩家特有的生命周期事件
    /// 使用 [Serializable] 使其可在 Inspector 中显示并绑定回调
    /// </summary>
    [Serializable]
    public class PlayerEvents
    {
        /// <summary>跳跃事件 —— 执行跳跃时触发（用于播放跳跃动画/音效/粒子）</summary>
        public UnityEvent OnJump;

        /// <summary>闪避事件 —— 执行闪避时触发（用于播放闪避特效/音效）</summary>
        public UnityEvent OnDodge;

        /// <summary>落地事件 —— 从空中落到地面时触发（用于播放落地动画/音效）</summary>
        public UnityEvent OnLand;
    }
}
