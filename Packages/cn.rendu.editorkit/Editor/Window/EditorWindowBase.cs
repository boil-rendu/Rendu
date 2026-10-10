using System;
using UnityEditor;
using UnityEngine;

namespace RD.EditorKit.Editor
{
    /// <summary>
    /// 轻量编辑器窗口基类
    /// 默认在 OnGUI 中依据 Inspector 属性自动绘制窗口自身成员
    /// </summary>
    public class EditorWindowBase : UnityEditor.EditorWindow
    {
        private bool m_Initialized;
        private Vector2 m_ScrollPosition;

        /// <summary>本窗口的状态会话: 生命周期与窗口一致, 窗口关闭即释放。</summary>
        private readonly HostSession m_Session = new();

        /// <summary>
        /// 首次显示前调用一次 子类可重写做初始化
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// 窗口关闭销毁时调用 子类可重写做清理
        /// </summary>
        protected virtual void OnWindowDestroy()
        {
        }

        private void EnsureInitialize()
        {
            if (m_Initialized) return;
            m_Initialized = true;

            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Inspector] 窗口初始化失败 {GetType().Name}\n{e}");
            }
        }

        /// <summary>
        /// 子类 OnGUI 覆盖时也需先完成惰性初始化
        /// </summary>
        protected void EnsureWindowInitialized()
        {
            EnsureInitialize();
        }

        protected virtual void OnGUI()
        {
            EnsureInitialize();
            DrawObjectPanel();
        }

        /// <summary>
        /// 以滚动视图绘制宿主窗口对象
        /// </summary>
        protected void DrawObjectPanel()
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            Engine.DrawObject(this, this, null, null, m_Session);
            GUILayout.EndScrollView();
        }

        /// <summary>用本窗口的会话绘制任意对象, 供子类复用。</summary>
        protected void DrawTargetObject(object target)
        {
            Engine.DrawObject(target, this, null, null, m_Session);
        }

        private void OnEnable()
        {
            // 消息占位:统一走 EnsureInitialize 惰性初始化
        }

        private void OnDestroy()
        {
            try
            {
                OnWindowDestroy();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Inspector] 窗口销毁失败 {GetType().Name}\n{e}");
            }
        }
    }
}
