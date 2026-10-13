//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

#if UNITY_EDITOR

using System.Collections.Generic;
using RD.Panel;
using RD.Panel.Editor;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// UI 工具：通用工具窗口(GenericToolWindow)的 UI 包薄壳, 头部加"发布"根,
    /// 尾部加"全局设置"根, 中间交给基类反射扫描所有带 [UIAutoMenu] 的模块。
    /// </summary>
    public class UITool : GenericToolWindow
    {
        [MenuItem("Rendu/UI Tool")]
        public static void OpenWindow()
        {
            // 停靠到 WindowsDefine 登记的同组窗口旁: 与 Excel View / Loader Tool 等菜单窗口
            // 合为同一个窗口下的不同页签, 不再各自飘独立窗口
            var window = GetWindow<UITool>("UI", true, RD.EditTool.Editor.WindowsDefine.DockedWindowTypes);
            if (window != null)
            {
                window.Show();
            }
        }

        //[MenuItem("RD/关闭 UI 工具")]
        //错误时使用的 面板出现了错误 会导致如何都打不开 就需要先关闭
        public static void CloseWindow()
        {
            GetWindow<UITool>()?.Close();
        }

        //关闭后刷新资源
        public static void CloseWindowRefresh()
        {
            CloseWindow();
            AssetDatabase.SaveAssets();

            //AssetDatabase.Refresh();//下面的刷新更NB
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        /// <summary>把 GenericToolWindow 的 BuildMenuTree 钩子挂上: 头部加"发布", 中间扫描, 尾部加"全局设置"</summary>
        protected override MenuTree BuildToolMenuTree(MenuTree tree)
        {
            // 头部: 手动挂第一个根节点(它有动态子菜单逻辑, 不能走 [UIAutoMenu] 反射)
            m_AllMenuItem.Add(new TreeMenuItem<PublishModule>(this, tree, PublishModule.m_PublishName, Icons.Folder));

            // 中间: 反射扫描所有 [UIAutoMenu] 模块
            base.BuildToolMenuTree(tree);

            // 尾部: 全局设置根(值是 this, 选中时绘制自身 [ShowInInspector] 字段)
            tree.Add("全局设置", this, Icons.SettingsCog);

            return tree;
        }

        public static EditorPrefsValue<string> UserNamePrefs = new EditorPrefsValue<string>("UITool_UserName", "UI");

        [LabelText("用户名")]
        [Required("请填写用户名")]
        [ShowInInspector]
        private static string m_Author;

        public static string Author
        {
            get
            {
                if (string.IsNullOrEmpty(m_Author))
                {
                    m_Author = UserNamePrefs.Value;
                }

                return m_Author;
            }
        }

        [HideLabel]
        [HideReferenceObjectPicker]
        [ShowInInspector]
        private BaseCreateModule m_UIBaseModule;

        [BoxGroup("全局图集设置", centerLabel: true)]
        [ShowInInspector]
        internal AtlasModule AtlasModule;

        [BoxGroup("其他设置", centerLabel: true)]
        [ShowInInspector]
        internal OtherModule OtherModule;

        protected override void OnToolInitialize()
        {
            m_UIBaseModule = new CreateBaseModule();
            AtlasModule = new();
            OtherModule = new();
            m_Author = UserNamePrefs.Value;
            m_UIBaseModule?.Initialize();
            AtlasModule?.Initialize();
            OtherModule?.Initialize();
        }

        protected override void OnToolDestroy()
        {
            UserNamePrefs.Value = Author;
            m_UIBaseModule?.OnDestroy();
            AtlasModule?.OnDestroy();
            OtherModule?.OnDestroy();
        }
    }
}
#endif
