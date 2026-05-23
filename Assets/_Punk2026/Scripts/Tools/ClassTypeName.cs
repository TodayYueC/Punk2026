using System;
using UnityEngine;

namespace Punk2026.Tools
{
    /// <summary>
    /// 自定义属性 —— 在 Inspector 中显示指定基类的所有子类下拉选择器
    /// 配合 ClassTypeNameDrawer 使用，在编辑器中以友好的下拉列表选择状态类
    /// 用于 PlayerStateManager 等需要在 Inspector 中配置状态列表的场景
    /// </summary>
    public class ClassTypeName : PropertyAttribute
    {
        /// <summary>允许选择的基类 Type（只有继承自该类型的类才会出现在下拉列表中）</summary>
        public Type type;

        public ClassTypeName(Type type)
        {
            this.type = type;
        }
    }
}
