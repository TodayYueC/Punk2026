using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Punk2026.Tools
{
    /// <summary>
    /// ClassTypeName 的自定义 PropertyDrawer
    /// 在 Inspector 中将 string 字段渲染为下拉列表
    /// 下拉列表内容为指定基类的所有子类（通过反射扫描所有程序集）
    /// 名称格式化：驼峰命名自动插入空格（如 FallPlayerState → "Fall Player State"）
    /// </summary>
    [CustomPropertyDrawer(typeof(ClassTypeName))]
    public class ClassTypeNameDrawer : PropertyDrawer
    {
        /// <summary>ClassTypeName 属性实例（持有基类 Type 引用）</summary>
        protected ClassTypeName classTypeName;

        /// <summary>子类的完整类型名列表（如 "Punk2026.Player.States.IdlePlayerState"）</summary>
        protected List<string> fullNames;

        /// <summary>格式化后的显示名列表（如 "Idle Player State"）</summary>
        protected List<string> displayNames;

        /// <summary>是否已初始化（避免重复反射扫描）</summary>
        protected bool initialized;

        /// <summary>
        /// 初始化：通过反射扫描所有程序集，找到指定基类的所有子类
        /// 将子类名分为完整名（用于序列化存储）和显示名（用于 Inspector 显示）
        /// </summary>
        protected virtual void Initialize()
        {
            classTypeName = (ClassTypeName)attribute;

            // 扫描所有程序集中的类型，筛选出继承自指定基类的子类
            var classes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(classTypeName.type));

            // 完整名（含命名空间）用于序列化存储
            fullNames = classes.Select(type => type.ToString()).ToList();
            // 显示名（仅类名，驼峰插入空格）用于 Inspector 下拉列表
            displayNames = classes
                .Select(type => type.Name)
                .Select(name => Regex.Replace(name, "(\\B[A-Z])", " $1"))
                .ToList();
        }

        /// <summary>确保属性有默认值（空字符串时默认选中第一个子类）</summary>
        protected virtual void EnsureDefaultValue(SerializedProperty property)
        {
            if (property.stringValue.Length == 0 && fullNames.Count > 0)
            {
                property.stringValue = fullNames[0];
            }
        }

        /// <summary>绘制 Inspector 下拉列表 GUI</summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 延迟初始化（首次绘制时执行一次反射扫描）
            if (!initialized)
            {
                initialized = true;
                Initialize();
            }

            if (fullNames.Count > 0)
            {
                EnsureDefaultValue(property);

                // 当前值不在列表中时跳过绘制（防止编译错误导致的异常）
                if (!fullNames.Contains(property.stringValue)) return;

                var currentIndex = fullNames.IndexOf(property.stringValue);
                // 绘制前缀标签
                position = EditorGUI.PrefixLabel(position, label);
                // 绘制下拉列表，返回选中索引
                var selectedIndex = EditorGUI.Popup(position, currentIndex, displayNames.ToArray());
                // 更新属性值为选中的完整类型名
                property.stringValue = fullNames[selectedIndex];
            }
        }
    }
}
