using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 管理窗口 - UI Toolkit 实现
    /// </summary>
    public class ExcelViewWindow : EditorWindow
    {
        private ExcelViewModel _viewModel;
        private VisualElement _root;
        private ListView _excelListView;
        private Label _totalLabel;
        private Label _emptyLabel;

        public static void Init()
        {
            var window = GetWindow<ExcelViewWindow>("Excel View", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            _viewModel = new ExcelViewModel();
            RefreshData();
        }

        private void CreateGUI()
        {
            var visualTree = UxmlLoader.LoadWindowUXML<ExcelViewWindow>();
            if (visualTree == null)
            {
                Debug.LogError($"无法加载 UXML 文件: ExcelViewWindow");
                return;
            }

            _root = visualTree.CloneTree();
            rootVisualElement.Add(_root);

            SetupVisualElements();
            BindEvents();
        }

        private void SetupVisualElements()
        {
            _excelListView = _root.Q<ListView>("excelListView");
            _totalLabel = _root.Q<Label>("totalLabel");
            _emptyLabel = _root.Q<Label>("emptyLabel");

            if (_excelListView != null)
            {
                _excelListView.makeItem = MakeItem;
                _excelListView.bindItem = BindItem;
                _excelListView.itemsSource = new System.Collections.Generic.List<ExcelItemModel>(_viewModel.ExcelItems);
            }
        }

        private void BindEvents()
        {
            var refreshButton = _root.Q<Button>("refreshButton");
            if (refreshButton != null)
            {
                refreshButton.clicked += RefreshData;
            }

            var exportButton = _root.Q<Button>("exportButton");
            if (exportButton != null)
            {
                exportButton.clicked += () => _viewModel.ExecuteExcelExport();
            }

            if (_excelListView != null)
            {
                _excelListView.selectionChanged += OnSelectionChanged;
            }
        }

        private VisualElement MakeItem()
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;
            item.style.paddingTop = 5;
            item.style.paddingBottom = 5;
            item.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            item.style.borderBottomWidth = 1;
            item.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            var fileNameBtn = new Button();
            fileNameBtn.style.width = 250;
            fileNameBtn.style.height = 24;
            fileNameBtn.style.fontSize = 12;

            var relativeDirLabel = new Label();
            relativeDirLabel.style.width = 200;
            relativeDirLabel.style.fontSize = 12;
            relativeDirLabel.style.color = new Color(0.6f, 0.6f, 0.6f);

            var fullPathLabel = new Label();
            fullPathLabel.style.flexGrow = 1;
            fullPathLabel.style.fontSize = 12;
            fullPathLabel.style.color = new Color(0.6f, 0.6f, 0.6f);

            item.Add(fileNameBtn);
            item.Add(relativeDirLabel);
            item.Add(fullPathLabel);

            return item;
        }

        private void BindItem(VisualElement element, int index)
        {
            var item = _viewModel.ExcelItems[index];
            var container = element as VisualElement;

            var fileNameBtn = container[0] as Button;
            var relativeDirLabel = container[1] as Label;
            var fullPathLabel = container[2] as Label;

            fileNameBtn.text = item.FileName;
            relativeDirLabel.text = item.RelativeDir ?? ".";
            fullPathLabel.text = item.FullPath;

            fileNameBtn.clickable = new Clickable(() => SelectExcelFile(item));
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            // 处理选择变化
        }

        private void SelectExcelFile(ExcelItemModel item)
        {
            string assetPath = item.FullPath;
#if UNITY_EDITOR_WIN
            assetPath = assetPath.Replace(Application.dataPath.Replace("Assets", "").Replace("/", "\\"), "");
#elif UNITY_EDITOR_OSX
            assetPath = assetPath.Replace(Application.dataPath.Replace("Assets", ""), "");
#endif

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
            else
            {
                Debug.LogError($"文件未找到: {assetPath}");
            }
        }

        private void RefreshData()
        {
            _viewModel.RefreshExcelList();

            if (_totalLabel != null)
            {
                _totalLabel.text = $"总计: {_viewModel.ExcelItems.Count}";
            }

            if (_excelListView != null)
            {
                _excelListView.itemsSource = new System.Collections.Generic.List<ExcelItemModel>(_viewModel.ExcelItems);
                _excelListView.Rebuild();
            }

            if (_emptyLabel != null)
            {
                _emptyLabel.style.display = _viewModel.ExcelItems.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_excelListView != null)
            {
                _excelListView.style.display = _viewModel.ExcelItems.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
