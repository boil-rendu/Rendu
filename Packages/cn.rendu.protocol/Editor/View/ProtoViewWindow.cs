using System.Collections.Generic;
using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Proto 查看窗口 - UI Toolkit 实现
    /// </summary>
    public class ProtoViewWindow : EditorWindow
    {
        private ProtoViewModel _sharedViewModel;
        private VisualElement _root;
        private VisualElement _tabContentContainer;
        private Button _listTabButton;
        private Button _editTabButton;

        private List<ITabStrategy> _tabs;
        private ITabStrategy _selectedTab;
        private int _selectedIndex = 0;

        public static void Init()
        {
            var window = GetWindow<ProtoViewWindow>("Proto View", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(900, 700);
        }

        private void OnEnable()
        {
            if (_sharedViewModel == null)
            {
                _sharedViewModel = new ProtoViewModel();
            }

            if (_tabs == null)
            {
                _tabs = new List<ITabStrategy>
                {
                    new ProtoListTab(_sharedViewModel),
                    new ProtoEditTab(_sharedViewModel)
                };
            }

            if (_selectedTab == null && _tabs.Count > 0)
            {
                _selectedTab = _tabs[0];
            }
        }

        private void OnDisable()
        {
            _selectedTab = null;
        }

        private void CreateGUI()
        {
            var visualTree = UxmlLoader.LoadWindowUXML<ProtoViewWindow>();
            if (visualTree == null)
            {
                Debug.LogError($"无法加载 UXML 文件: ProtoViewWindow");
                return;
            }

            _root = visualTree.CloneTree();
            rootVisualElement.Add(_root);

            SetupVisualElements();
            BindEvents();
            ShowTabContent(0);
        }

        private void SetupVisualElements()
        {
            _tabContentContainer = _root.Q<VisualElement>("tabContentContainer");
            _listTabButton = _root.Q<Button>("listTabButton");
            _editTabButton = _root.Q<Button>("editTabButton");
        }

        private void BindEvents()
        {
            if (_listTabButton != null)
            {
                _listTabButton.clicked += () => ShowTabContent(0);
            }

            if (_editTabButton != null)
            {
                _editTabButton.clicked += () => ShowTabContent(1);
            }
        }

        private void ShowTabContent(int index)
        {
            if (index < 0 || index >= _tabs.Count)
                return;

            _selectedIndex = index;
            _selectedTab = _tabs[index];

            UpdateTabButtonStyles();

            if (_tabContentContainer != null)
            {
                _tabContentContainer.Clear();

                var tabContent = _selectedTab.CreateVisualElement();
                if (tabContent != null)
                {
                    _tabContentContainer.Add(tabContent);
                }
            }
        }

        private void UpdateTabButtonStyles()
        {
            if (_listTabButton != null)
            {
                _listTabButton.style.backgroundColor = _selectedIndex == 0
                    ? new Color(0.25f, 0.55f, 0.95f, 0.3f)
                    : Color.clear;
            }

            if (_editTabButton != null)
            {
                _editTabButton.style.backgroundColor = _selectedIndex == 1
                    ? new Color(0.25f, 0.55f, 0.95f, 0.3f)
                    : Color.clear;
            }
        }
    }
}
