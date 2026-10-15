#if UNITY_EDITOR
using System;
using System.Linq;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    //Editor
    /// <summary>
    /// UIBindDataTable 的编辑器扩展（UNITY_EDITOR）
    /// 提供 Inspector 上的「自动检查 / 添加新数据 / 重置」按钮，以及数据的增删校验与回调挂接
    /// </summary>
    public sealed partial class UIBindDataTable
    {
        /// <summary>
        /// 自动检查：规整所有数据的命名（补前缀、首字母大写），移除空数据，最后刷新数据表
        /// </summary>
        [GUIColor(1, 1, 0)]
        [Button("自动检查", 30)]
        [PropertyOrder(-100)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        public void AutoCheck()
        {
            if (!UIOperationHelper.CheckUIOperation(this)) return;

            var dicKey = m_DataDic.Keys.ToList();
            foreach (var oldName in dicKey)
            {
                if (string.IsNullOrEmpty(oldName))
                {
                    continue;
                }

                var uiData = m_DataDic[oldName];
                if (uiData?.DataValue == null)
                {
                    Debug.LogError($"数据 {oldName} 为空 已移除请重新添加");
                    m_DataDic.Remove(oldName);
                    continue;
                }

                var newName = oldName;

                if (!oldName.CheckFirstName(NameUtility.DataName))
                {
                    newName = $"{NameUtility.FirstName}{NameUtility.DataName}{oldName}";
                }

                newName = newName.ChangeToBigName(NameUtility.DataName);

                if (oldName != newName)
                {
                    m_DataDic.Remove(oldName);
                    m_DataDic.Add(newName, uiData);
                }
            }

            OnValidate();
        }

        /// <summary>待添加的新数据（名称 + 数据类型）</summary>
        [DetailedInfoBox("添加新数据 说明",
            @"如果出现点击添加一个新的数据界面闪一下
然后什么都没加上的情况
这是由于预制件嵌套刷新问题
需要进入到预制件内部进行添加 外部无法操作
为了防止你莫名其妙的吧一个预制件之外的东西拖进来")]
        [ShowInInspector]
        [BoxGroup("添加新数据")]
        [HideReferenceObjectPicker]
        [HideLabel]
        [PropertyOrder(-99)]
        [NonSerialized]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private UINewData m_AddUINewData = new UINewData();

        /// <summary>添加新数据（校验名称非空/不重名/已选类型后加入字典并自动检查）</summary>
        [GUIColor(0, 1, 0)]
        [BoxGroup("添加新数据")]
        [Button("添加",40)]
        [PropertyOrder(-98)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private void AddNewData()
        {
            if (string.IsNullOrEmpty(m_AddUINewData.Name))
            {
                UnityTipsHelper.ShowError($"必须填写名称才可以添加");
                return;
            }

            if (m_DataDic.ContainsKey(m_AddUINewData.Name))
            {
                UnityTipsHelper.ShowError($"已存在同名数据  请修改 {m_AddUINewData.Name}");
                return;
            }

            if (m_AddUINewData.Data == null)
            {
                UnityTipsHelper.ShowError($"必须选择数据类型才可以添加  请修改 {m_AddUINewData.Name}");
                return;
            }

            var data = new UIData(m_AddUINewData.Name, m_AddUINewData.Data);

            m_DataDic.Add(data.Name, data);

            m_AddUINewData = new UINewData();

            AutoCheck();
        }

        /// <summary>重置待添加数据（保留上次输入的名称）</summary>
        [BoxGroup("添加新数据")]
        [Button("重置",20)]
        [PropertyOrder(-97)]
        [ShowIf("ShowIfResetNewData")]
        private void ResetNewData()
        {
            var lastName = m_AddUINewData.Name;
            m_AddUINewData      = new UINewData();
            m_AddUINewData.Name = lastName;
        }

        /// <summary>仅当在编辑态且已选类型时才显示「重置」</summary>
        private bool ShowIfResetNewData()
        {
            return UIOperationHelper.CommonShowIf() && m_AddUINewData?.Data != null;
        }

        /// <summary>移除回调：先解绑，再从字典移除（字典无该键时按 GUID 兜底移除）</summary>
        private void RemoveCallBack(UIData data)
        {
            data.OnDataRemoveCallBack();
            if (m_DataDic.ContainsKey(data.Name))
            {
                m_DataDic.Remove(data.Name);
            }
            else
            {
                OnRemoveDataByGuid(data);
            }
        }

        /// <summary>按 GUID 兜底移除不符合规范的数据</summary>
        private bool OnRemoveDataByGuid(UIData uiData)
        {
            foreach (var cData in m_DataDic)
            {
                if (cData.Value.DataGuid == uiData.DataGuid)
                {
                    m_DataDic.Remove(cData.Key);
                    Logger.LogError($"移除了一个不符合规范的数据 {cData.Key}");
                    return true;
                }
            }

            return false;
        }

        /// <summary>移除数据入口：已有绑定时弹确认框，否则直接移除</summary>
        private void OnRemoveData(UIData uiData)
        {
            if (string.IsNullOrEmpty(uiData.Name))
            {
                OnRemoveDataByGuid(uiData);
                return;
            }

            if (!m_DataDic.ContainsKey(uiData.Name))
            {
                OnRemoveDataByGuid(uiData);
                return;
            }

            var data = m_DataDic[uiData.Name];

            //如果已经有绑定了 需要提醒是否移除
            if (data.GetBindCount() >= 1)
            {
                var callBackTips = $"{data.Name} 已绑定 {data.GetBindCount()}个目标\n移除会强制清楚所有绑定 请确认是否需要移除!!!";
                UnityTipsHelper.CallBack(callBackTips, () => { RemoveCallBack(data); });
                return;
            }

            RemoveCallBack(data);
        }

        /// <summary>序列化校验：重新挂接数据的变更/移除回调并刷新数据表</summary>
        private void OnValidate()
        {
            if (UIOperationHelper.IsPlaying())
            {
                return;
            }

            foreach (var data in m_DataDic)
            {
                if (data.Value == null)
                {
                    continue;
                }

                data.Value.ClearBinds();
                data.Value.OnDataChange(data.Key);
                data.Value.OnDataRemoveAction = OnRemoveData;
            }

            InitDataTable();
        }
    }
}
#endif
