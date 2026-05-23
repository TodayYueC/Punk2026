using System;
using System.Collections.Generic;
using UnityEngine;

namespace Punk2026.Actor
{
    /// <summary>
    /// 状态机管理器基类（非泛型）—— 仅持有事件容器引用
    /// 作为 MonoBehaviour 的直接基类，方便 Inspector 序列化
    /// </summary>
    public abstract class ActorStateManager : MonoBehaviour
    {
        /// <summary>状态切换事件（onChange / onEnter / onExit）</summary>
        public ActorStateManagerEvents events;
    }

    /// <summary>
    /// 泛型状态机管理器 —— 核心 FSM 容器
    /// 管理状态的注册、查找、切换和每帧驱动
    /// 使用字典实现 O(1) 的类型查找，避免运行时反射开销
    /// </summary>
    /// <typeparam name="T">状态机所属的实体类型</typeparam>
    public abstract class ActorStateManager<T> : ActorStateManager where T : Actor<T>
    {
        /// <summary>状态列表（按注册顺序，index 用于 Animator 参数同步）</summary>
        protected List<ActorState<T>> stateList = new();

        /// <summary>当前正在执行的状态</summary>
        public ActorState<T> currentState { get; protected set; }

        /// <summary>上一个状态（用于动画过渡和状态回溯）</summary>
        public ActorState<T> lastState { get; protected set; }

        /// <summary>当前状态在列表中的索引</summary>
        public int index => stateList.IndexOf(currentState);

        /// <summary>上一个状态在列表中的索引</summary>
        public int lastIndex => stateList.IndexOf(lastState);

        /// <summary>类型 → 状态实例的映射字典，实现 O(1) 状态查找</summary>
        protected Dictionary<Type, ActorState<T>> stateDictionary = new();

        /// <summary>状态机关联的实体引用</summary>
        public T actor { get; protected set; }

        /// <summary>初始化顺序：先注册状态，再获取实体引用</summary>
        protected virtual void Start()
        {
            InitStates();
            InitActor();
        }

        /// <summary>从同一 GameObject 获取实体引用</summary>
        protected virtual void InitActor() => actor = GetComponent<T>();

        /// <summary>
        /// 初始化状态系统：
        ///   1. 调用子类的 GetStateList() 获取所有状态实例
        ///   2. 将每个状态注册到字典中（以 Type 为 Key）
        ///   3. 将列表第一个状态设为初始状态
        /// </summary>
        protected virtual void InitStates()
        {
            stateList = GetStateList();

            foreach (var state in stateList)
            {
                var type = state.GetType();
                if (!stateDictionary.ContainsKey(type))
                {
                    stateDictionary.Add(type, state);
                }
            }

            if (stateList.Count > 0)
            {
                currentState = stateList[0];
            }
        }

        /// <summary>由子类实现，返回该实体的所有状态实例列表</summary>
        protected abstract List<ActorState<T>> GetStateList();

        /// <summary>每帧驱动当前状态的 OnStep 逻辑</summary>
        public void Step()
        {
            if (currentState != null && Time.timeScale > 0)
            {
                currentState.Step(actor);
            }
        }

        /// <summary>通过泛型类型参数切换状态（编译时安全）</summary>
        public virtual void Change<TState>() where TState : ActorState<T>
        {
            if (stateDictionary.ContainsKey(typeof(TState)))
            {
                Change(stateDictionary[typeof(TState)]);
            }
        }

        /// <summary>
        /// 状态切换核心方法
        /// 执行流程：旧状态 Exit → 记录 lastState → 新状态 Enter → 广播事件
        /// Time.timeScale == 0 时禁止切换（暂停状态保护）
        /// </summary>
        public virtual void Change(ActorState<T> to)
        {
            if (to != null && Time.timeScale > 0)
            {
                if (currentState != null)
                {
                    currentState.Exit(actor);                    // 旧状态退出
                    events.onExit.Invoke(currentState.GetType()); // 广播退出事件
                    lastState = currentState;                     // 记录上一状态
                }

                currentState = to;
                currentState.Enter(actor);                   // 新状态进入
                events.onEnter.Invoke(currentState.GetType()); // 广播进入事件
                events.onChange?.Invoke();                     // 广播切换事件（用于动画触发）
            }
        }
    }
}
