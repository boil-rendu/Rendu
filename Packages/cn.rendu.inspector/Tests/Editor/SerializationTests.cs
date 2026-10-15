using System.Collections.Generic;
using RD.Inspector.Editor;
using NUnit.Framework;
using RD.Draw;
using UnityEngine;

namespace RD.Inspector.Tests.Editor
{
    /// <summary>仅用于验证 Unity 对象引用不落盘的哑资产。</summary>
    internal class SerializationTestsDummyAsset : ScriptableObject
    {
    }

    /// <summary>
    /// Serialization 的行为基线。
    /// 覆盖: 标量/字符/枚举/数组/List/Dictionary/嵌套 POCO 的写出与读回,
    /// 字符串转义, null 语义, 缺失与只读成员的处理, 以及 JSON 数值到目标类型的强转。
    /// </summary>
    public class SerializationTests
    {
        private enum Tag
        {
            Red = 0,
            Green = 1,
            Blue = 2,
        }

        private class Inner
        {
            [InspectorSerialize] public int X = 7;
        }

        private class Sample
        {
            [InspectorSerialize] public int IntValue = 42;
            [InspectorSerialize] public float FloatValue = 1.5f;
            [InspectorSerialize] public double DoubleValue = 2.25d;
            [InspectorSerialize] public bool Flag = true;
            [InspectorSerialize] public string Text = "hello";
            [InspectorSerialize] public char Letter = 'A';
            [InspectorSerialize] public Tag Kind = Tag.Green;
            [InspectorSerialize] public Inner Child = new Inner();
            [InspectorSerialize] public List<int> Numbers = new List<int> { 1, 2, 3 };
            [InspectorSerialize] public int[] Array = { 4, 5 };
            [InspectorSerialize] public Dictionary<string, int> Map = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        }

        private class Escapes
        {
            [InspectorSerialize] public string Value = "a\"b\\c\nd\te\u0001f";
        }

        private class Nullable
        {
            [InspectorSerialize] public string Text;
            [InspectorSerialize] public Inner Child;
        }

        private class ReadOnlyHolder
        {
            [InspectorSerialize] public int Settable = 1;

            [InspectorSerialize]
            public int GetterOnly
            {
                get { return 99; }
            }
        }

        private class UnityRefHolder
        {
            [InspectorSerialize] public Object Asset;
        }

        /// <summary>Unity 值类型成员: 绘制侧刚补上专用控件, 这里确认存读也能往返。</summary>
        private class UnityStructHolder
        {
            [InspectorSerialize] public Vector2 Direction = new Vector2(1, 2);
            [InspectorSerialize] public Vector4 Plane = new Vector4(1, 2, 3, 4);
            [InspectorSerialize] public Quaternion Rotation = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
            [InspectorSerialize] public Rect Area = new Rect(1, 2, 3, 4);
            [InspectorSerialize] public LayerMask Layer = 3;
        }

        /// <summary>
        /// Unity 属性包类型成员(数据不在托管字段上, 靠专用控件编辑)。
        /// 两个成员都默认留空: 曲线/渐变在纯 .NET 校验工程里造不出来(原生对象), 实例只在
        /// RoundTrip_Gradient 里现造 —— 那个用例因此被标为"需 Unity Test Runner"。
        /// </summary>
        private class UnityPropertyBagHolder
        {
            [InspectorSerialize] public Gradient Fade;
            [InspectorSerialize] public AnimationCurve Curve;
        }

        /// <summary>自引用夹具: Serialize 必须靠深度剪断, 否则递归写会直接栈溢出。</summary>
        private class Cycle
        {
            [InspectorSerialize] public int Value = 1;
            [InspectorSerialize] public Cycle Next;
        }

        /// <summary>HashSet 不是 IList, 用来验证集合还原不再因 (IList) 强转抛异常。</summary>
        private class SetHolder
        {
            [InspectorSerialize] public HashSet<int> Values = new HashSet<int> { 1, 2 };
        }

        //-------------------------------------- 写出 --------------------------------------

        [Test]
        public void Serialize_EmitsScalarsCollectionsAndNestedObjects()
        {
            var json = Serialization.Serialize(new Sample());

            StringAssert.Contains("\"IntValue\":42", json);
            StringAssert.Contains("\"FloatValue\":1.5", json);
            StringAssert.Contains("\"DoubleValue\":2.25", json);
            StringAssert.Contains("\"Flag\":true", json);
            StringAssert.Contains("\"Text\":\"hello\"", json);
            StringAssert.Contains("\"Letter\":65", json);
            StringAssert.Contains("\"Kind\":1", json);
            StringAssert.Contains("\"Child\":{\"X\":7}", json);
            StringAssert.Contains("\"Numbers\":[1,2,3]", json);
            StringAssert.Contains("\"Array\":[4,5]", json);
            StringAssert.Contains("\"a\":1", json);
            StringAssert.Contains("\"b\":2", json);
        }

        [Test]
        public void Serialize_EscapesControlAndQuoteCharacters()
        {
            var json = Serialization.Serialize(new Escapes());

            StringAssert.Contains("\\\"", json);
            StringAssert.Contains("\\\\", json);
            StringAssert.Contains("\\n", json);
            StringAssert.Contains("\\t", json);
            StringAssert.Contains("\\u0001", json);
        }

        [Test]
        public void Serialize_NullMembersBecomeJsonNull()
        {
            var json = Serialization.Serialize(new Nullable());

            StringAssert.Contains("\"Text\":null", json);
            StringAssert.Contains("\"Child\":null", json);
        }

        [Test]
        public void Serialize_UnityObjectReferenceIsNotPersisted()
        {
            var asset = ScriptableObject.CreateInstance<SerializationTestsDummyAsset>();
            try
            {
                var json = Serialization.Serialize(new UnityRefHolder { Asset = asset });
                StringAssert.Contains("\"Asset\":null", json);
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void Serialize_NullRoot_EmitsJsonNull()
        {
            Assert.AreEqual("null", Serialization.Serialize(null));
        }

        [Test]
        public void Serialize_SelfReferencingObject_IsCutByDepthLimit()
        {
            var node = new Cycle();
            node.Next = node;

            string json = null;
            Assert.DoesNotThrow(() => json = Serialization.Serialize(node),
                "环引用必须被深度剪断, 而不是栈溢出 —— 那在编辑器里无法捕获, 会直接闪退");
            StringAssert.Contains("\"Next\":null", json);
        }

        [Test]
        public void Deserialize_TruncatedUnicodeEscape_DoesNotThrow()
        {
            var target = new Escapes { Value = "keep" };

            Assert.DoesNotThrow(() => Serialization.Deserialize(target, "{\"Value\":\"a\\u00"),
                "截断的 \\uXXXX 若直接 Substring 会抛异常, 而这条路径在 OnAfterDeserialize 里");
            Assert.AreEqual("a", target.Value, "解析不到完整转义时按已读到的内容收尾");
        }

        //-------------------------------------- 往返 --------------------------------------

        [Test]
        public void RoundTrip_PreservesAllSupportedShapes()
        {
            var source = new Sample();
            var json = Serialization.Serialize(source);

            var target = new Sample
            {
                IntValue = 0,
                Text = null,
                Child = null,
                Numbers = null,
                Array = null,
                Map = null,
            };

            Serialization.Deserialize(target, json);

            Assert.AreEqual(source.IntValue, target.IntValue);
            Assert.AreEqual(source.FloatValue, target.FloatValue);
            Assert.AreEqual(source.DoubleValue, target.DoubleValue);
            Assert.AreEqual(source.Flag, target.Flag);
            Assert.AreEqual(source.Text, target.Text);
            Assert.AreEqual(source.Letter, target.Letter);
            Assert.AreEqual(source.Kind, target.Kind);

            Assert.IsNotNull(target.Child);
            Assert.AreEqual(7, target.Child.X);

            CollectionAssert.AreEqual(source.Numbers, target.Numbers);
            CollectionAssert.AreEqual(source.Array, target.Array);

            Assert.IsNotNull(target.Map);
            Assert.AreEqual(2, target.Map.Count);
            Assert.AreEqual(1, target.Map["a"]);
            Assert.AreEqual(2, target.Map["b"]);
        }

        [Test]
        public void RoundTrip_PreservesStringEscapes()
        {
            var source = new Escapes();
            var json = Serialization.Serialize(source);

            var target = new Escapes { Value = null };
            Serialization.Deserialize(target, json);

            Assert.AreEqual(source.Value, target.Value);
        }

        [Test]
        public void RoundTrip_NullStaysNull()
        {
            var json = Serialization.Serialize(new Nullable());

            var target = new Nullable { Text = "stale", Child = new Inner() };
            Serialization.Deserialize(target, json);

            Assert.IsNull(target.Text);
            Assert.IsNull(target.Child);
        }

        [Test]
        public void RoundTrip_SetIsSupported()
        {
            var json = Serialization.Serialize(new SetHolder());
            var target = new SetHolder { Values = null };

            Assert.DoesNotThrow(() => Serialization.Deserialize(target, json),
                "HashSet 只实现 ICollection<T> 而非 IList, 强转 IList 会把整个反序列化打断");
            Assert.IsNotNull(target.Values);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, target.Values);
        }

        [Test]
        public void RoundTrip_UnityStructs()
        {
            var source = new UnityStructHolder
            {
                Direction = new Vector2(1.5f, -2.5f),
                Plane = new Vector4(4, 3, 2, 1),
                Rotation = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f),
                Area = new Rect(1, 2, 3, 4),
                Layer = 5,
            };

            var json = Serialization.Serialize(source);
            StringAssert.DoesNotContain("\"Direction\":{}", json,
                "Unity 值类型的成员带不了 [InspectorSerialize], 不能因此被写成空对象(那等于静默清零)");

            var target = new UnityStructHolder();
            Serialization.Deserialize(target, json);

            Assert.AreEqual(source.Direction, target.Direction);
            Assert.AreEqual(source.Plane, target.Plane);
            Assert.AreEqual(source.Rotation.x, target.Rotation.x, 1e-6, "Quaternion");
            Assert.AreEqual(source.Rotation.w, target.Rotation.w, 1e-6, "Quaternion");
            Assert.AreEqual(source.Area, target.Area);
            Assert.AreEqual(source.Layer.value, target.Layer.value);
        }

        /// <summary>需 Unity 运行时: 渐变是原生对象, 纯 .NET 里 new 不出来(见 Program.cs 的跳过名单)。</summary>
        [Test]
        public void RoundTrip_Gradient()
        {
            var source = new UnityPropertyBagHolder { Fade = new Gradient() };
            source.Fade.mode = GradientMode.Fixed;
            source.Fade.SetKeys(
                new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.blue, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.25f, 1f) });

            var json = Serialization.Serialize(source);
            StringAssert.DoesNotContain("\"Fade\":{}", json,
                "渐变的数据在公开读写属性上, 不能因为它的字段既没有也不能带 [InspectorSerialize] 就写成空对象");

            var target = new UnityPropertyBagHolder { Fade = new Gradient() };
            Serialization.Deserialize(target, json);

            Assert.AreEqual(source.Fade.mode, target.Fade.mode);
            Assert.AreEqual(2, target.Fade.colorKeys.Length);
            Assert.AreEqual(source.Fade.colorKeys[0].color, target.Fade.colorKeys[0].color);
            Assert.AreEqual(source.Fade.colorKeys[1].color, target.Fade.colorKeys[1].color);
            Assert.AreEqual(source.Fade.colorKeys[1].time, target.Fade.colorKeys[1].time, 1e-6);
            Assert.AreEqual(source.Fade.alphaKeys[1].alpha, target.Fade.alphaKeys[1].alpha, 1e-6);
        }

        [Test]
        public void Serialize_NullCurveAndGradientAreWrittenAsNull()
        {
            var source = new UnityPropertyBagHolder();
            Assert.IsNull(source.Curve);
            Assert.IsNull(source.Fade);

            var json = Serialization.Serialize(source);
            StringAssert.Contains("\"Fade\":null", json);

            var target = new UnityPropertyBagHolder();
            Assert.DoesNotThrow(() => Serialization.Deserialize(target, json));
            Assert.IsNull(target.Curve);
            Assert.IsNull(target.Fade);
        }

        [Test]
        public void DataMembers_AnimationCurveUsesPublicProperties()
        {
            var names = new List<string>();
            foreach (var member in Serialization.GetDataMembers(typeof(AnimationCurve)))
            {
                names.Add(member.Name);
            }

            CollectionAssert.AreEquivalent(new[] { "keys", "preWrapMode", "postWrapMode" }, names,
                "曲线的数据实际存在原生 m_Ptr 里 —— 按字段序列化会存一堆句柄, 只能按公开读写属性存");
        }

        [Test]
        public void DataMembers_GradientUsesPublicProperties()
        {
            var names = new List<string>();
            foreach (var member in Serialization.GetDataMembers(typeof(Gradient)))
            {
                names.Add(member.Name);
            }

            CollectionAssert.AreEquivalent(new[] { "colorKeys", "alphaKeys", "mode", "colorSpace" }, names,
                "渐变的字段是私有的, 只能按公开读写属性存(不含只读的派生属性)");
        }

        //-------------------------------------- 读入 --------------------------------------

        [Test]
        public void Deserialize_IgnoresUnknownAndGetOnlyMembers()
        {
            var target = new ReadOnlyHolder();
            Serialization.Deserialize(target, "{\"Settable\":5,\"Unknown\":1,\"GetterOnly\":123}");

            Assert.AreEqual(5, target.Settable);
            Assert.AreEqual(99, target.GetterOnly, "无 setter 的成员应被跳过而不是抛异常");
        }

        [Test]
        public void Deserialize_CoercesJsonNumberToTargetType()
        {
            var target = new Sample();
            Serialization.Deserialize(target, "{\"IntValue\":7,\"FloatValue\":3}");

            Assert.AreEqual(7, target.IntValue);
            Assert.AreEqual(3f, target.FloatValue);
        }

        [Test]
        public void Deserialize_EmptyOrNullJson_IsNoOp()
        {
            var target = new Sample { IntValue = 42 };
            Serialization.Deserialize(target, "");
            Serialization.Deserialize(target, null);

            Assert.AreEqual(42, target.IntValue);
        }
    }
}
