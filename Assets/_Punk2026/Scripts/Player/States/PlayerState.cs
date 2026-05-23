using Punk2026.Actor;

namespace Punk2026.Player.States
{
    /// <summary>
    /// 玩家状态抽象基类 —— ActorState 的玩家特化版本
    /// 将泛型参数固定为 PlayerManager，简化所有具体状态类的编写
    /// 所有玩家状态（Idle/Move/Jump/Dodge/Fall）都继承此类
    /// </summary>
    public abstract class PlayerState : ActorState<PlayerManager>
    {
    }
}
