#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Init.Editor
{
    /// <summary>
    /// Flags枚举多选字段控件
    /// 支持在UIElements中通过下拉菜单选择多个枚举值（用于带[Flags]特性的枚举）
    /// </summary>
    public class FlagsEnumField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<FlagsEnumField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var field = (FlagsEnumField)ve;
                field.Label = _label.GetValueFromBag(bag, cc);
                // enumType需要从代码设置，UXML不支持Type参数
            }
        }

    private readonly Label _labelElement;
    private VisualElement _textContainer;
    private TextElement _textElement;

    private Type _enumType;
    private Enum _value;
    private List<Enum> _selectedValues;

        /// <summary>
        /// 标签文本
        /// </summary>
        public string Label
        {
            get => _labelElement.text;
            set
            {
                _labelElement.text = value;
                // 更新 label 的 class 以支持富文本
                _labelElement.enableRichText = false;
            }
        }

        /// <summary>
        /// 枚举类型
        /// </summary>
        public Type EnumType
        {
            get => _enumType;
            set
            {
                _enumType = value;
                _selectedValues = new List<Enum>();
            }
        }

        /// <summary>
        /// 当前选中的枚举值
        /// </summary>
        public Enum Value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateSelectedValues();
                UpdateDropdownText();
            }
        }

        /// <summary>
        /// 值变化事件
        /// </summary>
        public event Action<Enum> OnValueChanged;

        public FlagsEnumField()
        {
            // 使用 Unity 默认字段的样式
            AddToClassList("unity-base-field");
            AddToClassList("unity-base-field__aligned");

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.marginTop = 1;
            style.marginBottom = 1;

            _labelElement = new Label();
            _labelElement.AddToClassList("unity-base-field__label");
            _labelElement.style.minWidth = 120;
            _labelElement.style.marginRight = 4;
            Add(_labelElement);

            // 创建输入容器
            var inputContainer = new VisualElement();
            inputContainer.AddToClassList("unity-base-field__input");
            inputContainer.style.flexGrow = 1;
            inputContainer.style.flexDirection = FlexDirection.Row;
            inputContainer.style.overflow = Overflow.Hidden;
            Add(inputContainer);

            // 创建可点击的文本容器，使用Unity内置样式类
            _textContainer = new VisualElement();
            _textContainer.AddToClassList("unity-dropdown");
            _textContainer.style.flexGrow = 1;
            _textContainer.style.flexDirection = FlexDirection.Row;
            _textContainer.style.alignItems = Align.Center;
            _textContainer.style.overflow = Overflow.Hidden;
            _textContainer.style.cursor = StyleKeyword.Initial;
            _textContainer.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            _textContainer.style.borderLeftWidth = 1;
            _textContainer.style.borderRightWidth = 1;
            _textContainer.style.borderTopWidth = 1;
            _textContainer.style.borderBottomWidth = 1;
            _textContainer.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f);
            _textContainer.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f);
            _textContainer.style.borderTopColor = new Color(0.1f, 0.1f, 0.1f);
            _textContainer.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);
            _textContainer.style.borderTopLeftRadius = 2;
            _textContainer.style.borderTopRightRadius = 2;
            _textContainer.style.borderBottomLeftRadius = 2;
            _textContainer.style.borderBottomRightRadius = 2;
            inputContainer.Add(_textContainer);

            // 创建文本显示区域
            var textInput = new VisualElement();
            textInput.style.flexGrow = 1;
            textInput.style.overflow = Overflow.Hidden;
            textInput.style.flexDirection = FlexDirection.Row;
            textInput.style.alignItems = Align.Center;
            textInput.style.paddingLeft = 4;
            textInput.style.paddingRight = 4;
            textInput.style.height = 18;
            _textContainer.Add(textInput);

            _textElement = new TextElement();
            _textElement.style.whiteSpace = WhiteSpace.NoWrap;
            _textElement.style.overflow = Overflow.Hidden;
            _textElement.style.textOverflow = TextOverflow.Ellipsis;
            _textElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            _textElement.style.flexGrow = 1;
            _textElement.style.fontSize = 11;
            _textElement.style.color = new Color(0.85f, 0.85f, 0.85f);
            textInput.Add(_textElement);

            // 添加下拉箭头（使用Label显示箭头符号）
            var arrow = new Label("▼");
            arrow.style.fontSize = 8;
            arrow.style.marginLeft = 4;
            arrow.style.marginRight = 2;
            arrow.style.color = new Color(0.7f, 0.7f, 0.7f);
            textInput.Add(arrow);

            // 注册鼠标点击事件
            _textContainer.RegisterCallback<MouseDownEvent>(OnDropdownMouseDown);
            
            // 让整个容器可点击
            textInput.pickingMode = PickingMode.Position;
            arrow.pickingMode = PickingMode.Ignore;
        }

        public FlagsEnumField(string label, Type enumType) : this()
        {
            Label = label;
            EnumType = enumType;
        }

        /// <summary>
        /// 注册值变化回调
        /// </summary>
        public void RegisterValueChangedCallback(Action<Enum> callback)
        {
            OnValueChanged += callback;
        }

        /// <summary>
        /// 设置枚举值（不触发事件）
        /// </summary>
        public void SetValueWithoutNotify(Enum value)
        {
            _value = value;
            UpdateSelectedValues();
            UpdateDropdownText();
        }

        /// <summary>
        /// 下拉框鼠标按下事件
        /// </summary>
        private void OnDropdownMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0 && _enumType != null)
            {
                evt.StopImmediatePropagation();
                ShowMultiSelectMenu();
            }
        }

        /// <summary>
        /// 显示多选菜单
        /// </summary>
        private void ShowMultiSelectMenu()
        {
            GenericMenu menu = new GenericMenu();

            // 添加"清除全部"选项
            menu.AddItem(new GUIContent("Clear All"), _selectedValues.Count == 0, () =>
            {
                _selectedValues.Clear();
                UpdateValueFromSelection();
            });

            menu.AddSeparator("");

            // 添加每个枚举值选项
            Array enumValues = Enum.GetValues(_enumType);
            foreach (Enum enumValue in enumValues)
            {
                string displayName = enumValue.ToString();
                bool isSelected = _selectedValues.Contains(enumValue);
                menu.AddItem(new GUIContent(displayName), isSelected, () =>
                {
                    ToggleSelection(enumValue);
                });
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// 切换选中状态
        /// </summary>
        private void ToggleSelection(Enum enumValue)
        {
            // 处理"None"（0值）的情况
            long enumInt = Convert.ToInt64(enumValue);
            if (enumInt == 0)
            {
                _selectedValues.Clear();
                _selectedValues.Add(enumValue);
            }
            else
            {
                // 移除None（如果存在）
                _selectedValues.RemoveAll(v => Convert.ToInt64(v) == 0);

                if (_selectedValues.Contains(enumValue))
                {
                    _selectedValues.Remove(enumValue);
                }
                else
                {
                    _selectedValues.Add(enumValue);
                }
            }

            UpdateValueFromSelection();
        }

        /// <summary>
        /// 从选中项更新值
        /// </summary>
        private void UpdateValueFromSelection()
        {
            if (_selectedValues.Count == 0)
            {
                // 如果没有选中项，设置为0
                _value = (Enum)Enum.ToObject(_enumType, 0);
            }
            else if (_selectedValues.Count == 1)
            {
                _value = _selectedValues[0];
            }
            else
            {
                // 合并多个枚举值
                long combinedValue = 0;
                foreach (Enum selected in _selectedValues)
                {
                    combinedValue |= Convert.ToInt64(selected);
                }
                _value = (Enum)Enum.ToObject(_enumType, combinedValue);
            }

            UpdateDropdownText();
            OnValueChanged?.Invoke(_value);
        }

        /// <summary>
        /// 从当前值更新选中项列表
        /// </summary>
        private void UpdateSelectedValues()
        {
            _selectedValues = new List<Enum>();

            if (_value == null)
                return;

            long valueAsLong = Convert.ToInt64(_value);
            Array enumValues = Enum.GetValues(_enumType);

            foreach (Enum enumValue in enumValues)
            {
                long enumInt = Convert.ToInt64(enumValue);

                if (enumInt == 0)
                {
                    // "None"值的特殊处理
                    if (valueAsLong == 0)
                    {
                        _selectedValues.Add(enumValue);
                    }
                }
                else
                {
                    // 检查是否包含该枚举位
                    if ((valueAsLong & enumInt) == enumInt)
                    {
                        _selectedValues.Add(enumValue);
                    }
                }
            }
        }

        /// <summary>
        /// 更新下拉框显示文本
        /// </summary>
        private void UpdateDropdownText()
        {
            string displayText;
            if (_selectedValues.Count == 0)
            {
                displayText = "None";
            }
            else
            {
                // 直接显示所有已选择的枚举值名称
                displayText = string.Join(", ", _selectedValues.Select(v => v.ToString()));
            }

            _textElement.text = displayText;
            _textElement.tooltip = displayText; // 设置完整文本为tooltip
        }
    }
}
#endif
