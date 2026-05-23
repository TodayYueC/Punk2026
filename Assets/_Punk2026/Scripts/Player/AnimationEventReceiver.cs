using UnityEngine;

namespace Punk2026.Player
{
    /// <summary>
    /// 动画事件接收器 —— 挂在 Animator 所在的模型 GameObject 上
    /// 模型自带的动画中有 AnimationEvent 调用 OnFootstep / OnLand 方法
    /// AnimationEvent 只会找 Animator 同一个 GameObject 上的方法，
    /// 所以这个组件必须和 Animator 在同一个物体上
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        /// <summary>脚步动画事件（走路/跑步动画中触发）</summary>
        public void OnFootstep() { }

        /// <summary>落地动画事件（落地动画中触发）</summary>
        public void OnLand() { }
    }
}
