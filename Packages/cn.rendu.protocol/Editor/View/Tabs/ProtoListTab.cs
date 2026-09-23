using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Protocol.Editor
{
    public class ProtoListTab : TabBase
    {
        public override string Label => "📋 Proto 列表";

        private ProtoViewModel _viewModel;
        private Vector2 _scrollPosition;

        public ProtoListTab(ProtoViewModel viewModel)
        {
            _viewModel = viewModel;
            RefreshData();
        }

        public override VisualElement CreateVisualElement()
        {
            var container = new VisualElement();
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;

            container.Add(CreateSectionHeader(Label));
            container.Add(DrawStatsUI());
            container.Add(DrawActionsUI());
            container.Add(DrawProtoListUI());

            return container;
        }

        public override void OnGUI()
        {
            DrawSectionHeader(Label);

            // 统计信息
            DrawStats();
            GUILayout.Space(15);

            // 操作按钮
            DrawActions();
            GUILayout.Space(15);

            // Proto 文件列表
            DrawProtoList();
        }

        private void DrawStats()
        {
            var items = _viewModel.ProtoItems;
            int total = items.Count;
            int errors = 0;
            int formatErrors = 0;
            int duplicates = 0;
            int parseErrors = 0;

            foreach (var item in items)
            {
                if (item.HasError) errors++;
                if (item.IsFormatError) formatErrors++;
                if (item.IsDuplicateError) duplicates++;
                if (item.IsParseError) parseErrors++;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📊 统计信息", EditorStyles.boldLabel);
            GUILayout.Space(5);

            DrawInfoRow("总计", total.ToString());
            DrawInfoRow("状态", errors > 0 ? "❌ 有错误" : "✅ 正常");

            if (errors > 0)
            {
                if (formatErrors > 0) DrawInfoRow("格式错误", formatErrors.ToString());
                if (duplicates > 0) DrawInfoRow("数值重复", duplicates.ToString());
                if (parseErrors > 0) DrawInfoRow("解析失败", parseErrors.ToString());
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔧 操作", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔄 刷新", GUILayout.Height(38)))
            {
                RefreshData();
            }

            EditorGUI.BeginChangeCheck();
            bool newSortEnabled = GUILayout.Toggle(_viewModel.SortEnabled, "按编号排序", GUILayout.Width(100), GUILayout.Height(38));
            if (EditorGUI.EndChangeCheck())
            {
                _viewModel.SortEnabled = newSortEnabled;
                RefreshData();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("⚙️ 协议导出", GUILayout.Height(38)))
            {
                _viewModel.ExecuteProtocolExport();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawProtoList()
        {
            var items = _viewModel.ProtoItems;

            if (items.Count == 0)
            {
                DrawInfoBox("未找到 Proto 文件，请确认项目中有 .proto 文件");
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 Proto 文件列表", EditorStyles.boldLabel);
            GUILayout.Space(5);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var item in items)
            {
                bool isError = item.ErrorCode == 1 || item.ErrorCode == 3;
                bool isWarning = item.ErrorCode == 2;

                var bgColor = GUI.backgroundColor;
                GUI.backgroundColor = isError ? new Color(1f, 0.3f, 0.3f, 0.1f) :
                                   isWarning ? new Color(1f, 0.8f, 0.4f, 0.1f) : Color.white;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                string icon = isError ? "❌ " : isWarning ? "⚠️ " : "📄 ";
                string label = icon + item.Name;

                if (GUILayout.Button(label, GUILayout.Width(250), GUILayout.Height(30)))
                {
                    SelectProtoFile(item);
                }

                GUILayout.Label(item.FullPath, EditorStyles.wordWrappedLabel);
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = bgColor;
                GUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawInfoRow(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Width(140));
            GUILayout.Label(value);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void SelectProtoFile(ProtoItemModel item)
        {
            string assetPath = item.FullPath;
#if UNITY_EDITOR_WIN
            assetPath = assetPath.Replace(Application.dataPath.Replace("Assets", "").Replace("/", "\\"), "");
#elif UNITY_EDITOR_OSX
            assetPath = assetPath.Replace(Application.dataPath.Replace("Assets", ""), "");
#endif

            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
        }

        private void RefreshData()
        {
            _viewModel.RefreshProtoList();
        }

        private VisualElement DrawStatsUI()
        {
            var items = _viewModel.ProtoItems;
            int total = items.Count;
            int errors = items.Count(i => i.HasError);
            int formatErrors = items.Count(i => i.IsFormatError);
            int duplicates = items.Count(i => i.IsDuplicateError);
            int parseErrors = items.Count(i => i.IsParseError);

            var box = new HelpBox(
                $"📊 统计信息\n总计: {total}, 状态: {(errors > 0 ? "❌ 有错误" : "✅ 正常")}" +
                (errors > 0 ? $"\n格式错误: {formatErrors}, 数值重复: {duplicates}, 解析失败: {parseErrors}" : ""),
                HelpBoxMessageType.Info);
            box.style.marginBottom = 15;

            return box;
        }

        private VisualElement DrawActionsUI()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginBottom = 15;

            var refreshBtn = new Button(RefreshData);
            refreshBtn.text = "🔄 刷新";
            refreshBtn.style.height = 38;
            refreshBtn.style.marginRight = 5;
            container.Add(refreshBtn);

            var sortToggle = new Toggle("按编号排序");
            sortToggle.value = _viewModel.SortEnabled;
            sortToggle.style.width = 120;
            sortToggle.RegisterValueChangedCallback(evt =>
            {
                _viewModel.SortEnabled = evt.newValue;
                RefreshData();
            });
            container.Add(sortToggle);

            var flexibleSpace = new VisualElement();
            flexibleSpace.style.flexGrow = 1;
            container.Add(flexibleSpace);

            var exportBtn = new Button(() => _viewModel.ExecuteProtocolExport());
            exportBtn.text = "⚙️ 协议导出";
            exportBtn.style.height = 38;
            container.Add(exportBtn);

            return container;
        }

        private VisualElement DrawProtoListUI()
        {
            var items = _viewModel.ProtoItems;
            var container = new VisualElement();

            if (items.Count == 0)
            {
                container.Add(new HelpBox("未找到 Proto 文件，请确认项目中有 .proto 文件", HelpBoxMessageType.Info));
                return container;
            }

            var scrollView = new ScrollView();
            scrollView.style.height = 300;

            foreach (var item in items)
            {
                bool isError = item.ErrorCode == 1 || item.ErrorCode == 3;
                bool isWarning = item.ErrorCode == 2;

                var message = isError ? "❌ " : isWarning ? "⚠️ " : "📄 ";
                message += $"{item.Name}\n{item.FullPath}";

                var helpBox = new HelpBox(message, isError ? HelpBoxMessageType.Error :
                                                          isWarning ? HelpBoxMessageType.Warning :
                                                          HelpBoxMessageType.Info);
                helpBox.style.marginBottom = 2;

                var box = new VisualElement();
                box.Add(helpBox);
                box.RegisterCallback<MouseDownEvent>(evt => SelectProtoFile(item));

                scrollView.Add(box);
            }

            container.Add(scrollView);

            return container;
        }
    }
}
