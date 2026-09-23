#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RD.Inspector;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>
    ///  ET宏
    /// </summary>
    public class ETMacroModule : BaseUIToolModule
    {
        //TODO 功能TODO
        
        public override void Initialize()
        {
            var assembly = RD.Client.AssemblyHelper.GetAssembly("RD.UI.Editor");

            var allMacroEnum = RD.Client.AssemblyHelper.GetClassesWithAttribute<UIEnumETMacroAttribute>(assembly);

            Type macroDataBaseType = typeof(MacroDataBase<>);

            foreach (var macroEnum in allMacroEnum)
            {
                Type specificType = macroDataBaseType.MakeGenericType(macroEnum);
                var  instance     = (MacroDataBase)Activator.CreateInstance(specificType);
                instance.Initialize();
            }
        }

        public override void OnDestroy()
        {
        }
    }
}
#endif
