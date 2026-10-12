using UnityEditor;

namespace RD.Inspector.Editor
{
    /// <summary>
    /// 为 SerializedScriptableObject 及其全部子类提供检视器。
    ///
    /// 这个基类把 [InspectorSerialize] 成员序列化成 JSON 存进 m_InspectorData,
    /// 因此它的数据用 Unity 默认检视器是完全看不见的(成员大多是私有 [InspectorSerialize] 字段)。
    /// 引擎的成员可见性规则已经认 [InspectorSerialize], 所以接上检视器即可正常编辑。
    ///
    /// 用 true 让整个继承族自动生效 —— 子类可能分布在其它包(如 packagemanager 的
    /// PackageHubAsset / PackageVersionAsset), 而这些包反过来依赖本包, 无法在各自包里注册,
    /// 在这里一次性覆盖是唯一不产生循环依赖的做法。
    /// </summary>
    [CustomEditor(typeof(SerializedScriptableObject), true)]
    public class SerializedScriptableObjectEditor : InspectorEditor
    {
    }
}
