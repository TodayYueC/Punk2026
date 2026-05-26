using Punk2026.Actor;

namespace Punk2026.Enemy.States
{
    /// <summary>
    /// 敌人状态抽象基类 —— ActorState 的敌人特化版本
    /// 将泛型参数固定为 EnemyManager，简化所有具体状态类的编写
    /// 所有敌人状态（Idle/Chase/Attack）都继承此类
    /// </summary>
    public abstract class EnemyState : ActorState<EnemyManager>
    {
    }
}
