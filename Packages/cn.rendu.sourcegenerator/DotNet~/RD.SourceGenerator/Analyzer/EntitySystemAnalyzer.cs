using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RD;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EntitySystemAnalyzer: DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        EntitySystemAnalyzerRule.Rule,
        EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule,
        UIEntitySystemAnalyzerRule.Rule,
        UIEntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule);

    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerGlobalSetting.EnableAnalyzer)
        {
            return;
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(this.Analyzer, SymbolKind.NamedType);
        context.RegisterSymbolAction(this.AnalyzeIsSystemMethodValid, SymbolKind.NamedType);
    }

    // 普通 EntitySystem / LSEntitySystem 与 UI EntitySystem 的扫描流程完全一致，
    // 区别只有接口清单和要报的诊断，所以放在同一张表里用 Rule 区分。
    // UI EntitySystem 复用的也是同一套 EntitySystemOf / EntitySystem 标签。
    private static readonly ImmutableArray<RDSystemData> SupportedSystemDatas = ImmutableArray.Create(
        new RDSystemData(EntitySystemAnalyzerRule.Rule, EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule,
            Definition.EntitySystemOfAttribute, Definition.EntitySystemAttribute, Definition.EntityType, Definition.EntitySystemAttributeMetaName,
            new SystemMethodData(Definition.IAwakeInterface, Definition.AwakeMethod),
            new SystemMethodData(Definition.IUpdateInterface, Definition.UpdateMethod),
            new SystemMethodData(Definition.IDestroyInterface, Definition.DestroyMethod),
            new SystemMethodData(Definition.IAddComponentInterface, Definition.AddComponentMethod),
            new SystemMethodData(Definition.IDeserializeInterface, Definition.DeserializeMethod),
            new SystemMethodData(Definition.IGetComponentInterface, Definition.GetComponentMethod, extraParameter: Definition.SystemType),
            new SystemMethodData(Definition.ILoadInterface, Definition.LoadMethod),
            new SystemMethodData(Definition.ILateUpdateInterface, Definition.LateUpdateMethod),
            new SystemMethodData(Definition.ISerializeInterface, Definition.SerializeMethod),
            new SystemMethodData(Definition.ILSRollbackInterface, Definition.LSRollbackMethod)),

        new RDSystemData(EntitySystemAnalyzerRule.Rule, EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule,
            Definition.LSEntitySystemOfAttribute, Definition.LSEntitySystemAttribute, Definition.LSEntityType, Definition.LSEntitySystemAttributeMetaName,
            new SystemMethodData(Definition.IAwakeInterface, Definition.AwakeMethod),
            new SystemMethodData(Definition.ILSUpdateInterface, Definition.LSUpdateMethod),
            new SystemMethodData(Definition.IDestroyInterface, Definition.DestroyMethod),
            new SystemMethodData(Definition.IAddComponentInterface, Definition.AddComponentMethod),
            new SystemMethodData(Definition.IDeserializeInterface, Definition.DeserializeMethod),
            new SystemMethodData(Definition.IGetComponentInterface, Definition.GetComponentMethod, extraParameter: Definition.SystemType),
            new SystemMethodData(Definition.ILoadInterface, Definition.LoadMethod),
            new SystemMethodData(Definition.ISerializeInterface, Definition.SerializeMethod),
            new SystemMethodData(Definition.ILSRollbackInterface, Definition.LSRollbackMethod)),

        new RDSystemData(UIEntitySystemAnalyzerRule.Rule, UIEntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule,
            Definition.EntitySystemOfAttribute, Definition.EntitySystemAttribute, Definition.EntityType, Definition.EntitySystemAttributeMetaName,

            //自定义
            new SystemMethodData(Definition.IUIWindowCloseInterface, Definition.UIWindowCloseMethod, Definition.AsyncPromiseReturnType, Definition.BoolType),
            new SystemMethodData(Definition.IUIBackCloseInterface, Definition.UIBackCloseMethod, Definition.AsyncPromiseReturnType, Definition.UIEventPanelInfoType),
            new SystemMethodData(Definition.IUIBackOpenInterface, Definition.UIBackOpenMethod, Definition.AsyncPromiseReturnType, Definition.UIEventPanelInfoType),
            new SystemMethodData(Definition.IUIBackHomeCloseInterface, Definition.UIBackHomeCloseMethod, Definition.AsyncPromiseReturnType, Definition.UIEventPanelInfoType),

            //async Promise<bool>
            new SystemMethodData(Definition.IUICloseInterface, Definition.UICloseMethod, Definition.AsyncPromiseBoolReturnType),
            new SystemMethodData(Definition.IUIOpenInterface, Definition.UIOpenMethod, Definition.AsyncPromiseBoolReturnType),
            new SystemMethodData(Definition.IUIDisCloseInterface, Definition.UIDisCloseMethod, Definition.AsyncPromiseBoolReturnType),

            //async Promise
            new SystemMethodData(Definition.IAwakeAsyncInterface, Definition.AwakeAsyncMethod, Definition.AsyncPromiseReturnType),
            new SystemMethodData(Definition.IDynamicEventInterface, Definition.DynamicEventMethod, Definition.AsyncPromiseReturnType),
            new SystemMethodData(Definition.IUIBackHomeOpenInterface, Definition.UIBackHomeOpenMethod, Definition.AsyncPromiseReturnType),
            new SystemMethodData(Definition.IUICloseTweenInterface, Definition.UICloseTweenMethod, Definition.AsyncPromiseReturnType),
            new SystemMethodData(Definition.IUIOpenTweenInterface, Definition.UIOpenTweenMethod, Definition.AsyncPromiseReturnType),

            //void
            new SystemMethodData(Definition.IUIBindInterface, Definition.UIBindMethod),
            new SystemMethodData(Definition.IUIDisableInterface, Definition.UIDisableMethod),
            new SystemMethodData(Definition.IUIEnableInterface, Definition.UIEnableMethod),
            new SystemMethodData(Definition.IUIInitializeInterface, Definition.UIInitializeMethod),
            new SystemMethodData(Definition.IUICloseTweenEndInterface, Definition.UICloseTweenEndMethod),
            new SystemMethodData(Definition.IUIOpenTweenEndInterface, Definition.UIOpenTweenEndMethod)));

    // 同一个诊断（同一 Rule）的数据共用一份 properties，最终只报一条诊断
    private static readonly ImmutableArray<IGrouping<DiagnosticDescriptor, RDSystemData>> RDSystemDataGroups =
        SupportedSystemDatas.GroupBy(data => data.Rule).ToImmutableArray();

    private class RDSystemData
    {
        public string EntityTypeName;
        public string SystemOfAttribute;
        public string SystemAttributeShowName;
        public string SystemAttributeMetaName;
        public SystemMethodData[] SystemMethods;
        public DiagnosticDescriptor Rule;
        public DiagnosticDescriptor MethodNeedSystemOfAttrRule;

        public RDSystemData(DiagnosticDescriptor rule, DiagnosticDescriptor methodNeedSystemOfAttrRule,
            string systemOfAttribute, string systemAttributeShowName, string entityTypeName, string systemAttributeMetaName,
            params SystemMethodData[] systemMethods)
        {
            this.Rule = rule;
            this.MethodNeedSystemOfAttrRule = methodNeedSystemOfAttrRule;
            this.SystemOfAttribute = systemOfAttribute;
            this.SystemAttributeShowName = systemAttributeShowName;
            this.EntityTypeName = entityTypeName;
            this.SystemAttributeMetaName = systemAttributeMetaName;
            this.SystemMethods = systemMethods;
        }
    }

    public struct SystemMethodData
    {
        public string InterfaceName;
        public string MethodName;

        /// <summary>
        /// 写进 diagnostic properties 的方法名，形如 "方法名" 或 "方法名|返回类型"。
        /// CodeFix 端按 "|" 拆出返回类型，没有 "|" 就说明接口方法本身返回 void。
        /// </summary>
        public string MethodMetaName;

        /// <summary>
        /// self 与泛型参数之外还需要一并匹配的参数类型，多个用 "/" 分隔。
        /// </summary>
        public string[] ExtraParameterTypes;

        public SystemMethodData(string interfaceName, string methodName, string methodReturnType = "", string extraParameter = "")
        {
            this.InterfaceName = interfaceName;
            this.MethodName = methodName;
            this.MethodMetaName = string.IsNullOrEmpty(methodReturnType) ? methodName : $"{methodName}|{methodReturnType}";
            this.ExtraParameterTypes = extraParameter.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    private void Analyzer(SymbolAnalysisContext context)
    {
        if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
        {
            return;
        }

        foreach (IGrouping<DiagnosticDescriptor, RDSystemData> group in RDSystemDataGroups)
        {
            ImmutableDictionary<string, string?>.Builder? builder = null;
            foreach (RDSystemData rdSystemData in group)
            {
                this.AnalyzeRDSystem(context, rdSystemData, ref builder);
            }

            this.ReportNeedGenerateSystem(context, namedTypeSymbol, ref builder, group.Key);
        }
    }

    private void AnalyzeRDSystem(SymbolAnalysisContext context, RDSystemData rdSystemData, ref ImmutableDictionary<string, string?>.Builder? builder)
    {
        if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
        {
            return;
        }

        // 筛选出含有SystemOf标签的类
        AttributeData? attr = namedTypeSymbol.GetFirstAttribute(rdSystemData.SystemOfAttribute);
        if (attr == null)
        {
            return;
        }

        // 获取所属的实体类symbol
        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol)
        {
            return;
        }

        bool ignoreAwake = false;
        if (attr.ConstructorArguments.Length >= 2 && attr.ConstructorArguments[1].Value is bool ignore)
        {
            ignoreAwake = ignore;
        }

        // 排除非Entity子类
        if (entityTypeSymbol.BaseType?.ToString() != rdSystemData.EntityTypeName)
        {
            return;
        }

        foreach (INamedTypeSymbol? interfaceTypeSymbol in entityTypeSymbol.AllInterfaces)
        {
            if (ignoreAwake && interfaceTypeSymbol.IsInterface(Definition.IAwakeInterface))
            {
                continue;
            }

            foreach (SystemMethodData systemMethodData in rdSystemData.SystemMethods)
            {
                if (!interfaceTypeSymbol.IsInterface(systemMethodData.InterfaceName))
                {
                    continue;
                }

                if (!IsInterfaceMethodImplemented(namedTypeSymbol, systemMethodData, entityTypeSymbol, interfaceTypeSymbol))
                {
                    StringBuilder str = new();
                    str.Append(entityTypeSymbol);
                    str.Append("/");
                    str.Append(rdSystemData.SystemAttributeShowName);

                    StringBuilder genericArgs = new();
                    foreach (ITypeSymbol? typeArgument in interfaceTypeSymbol.TypeArguments)
                    {
                        str.Append("/");
                        str.Append(typeArgument);
                        genericArgs.Append(typeArgument);
                    }

                    foreach (string extraParameterType in systemMethodData.ExtraParameterTypes)
                    {
                        str.Append("/");
                        str.Append(extraParameterType);
                    }

                    // 泛型接口把类型参数拼在 "`" 后面，非泛型接口不带后缀
                    string methodMetaName = interfaceTypeSymbol.IsGenericType
                            ? $"{systemMethodData.MethodMetaName}`{genericArgs}"
                            : systemMethodData.MethodMetaName;

                    AddProperty(ref builder, methodMetaName, str.ToString());
                }

                break;
            }
        }
    }

    /// <summary>
    /// 类上是否存在参数为 self + 泛型参数 + 额外参数的同名方法，即该接口方法已被实现
    /// </summary>
    private static bool IsInterfaceMethodImplemented(INamedTypeSymbol namedTypeSymbol, SystemMethodData systemMethodData,
        ITypeSymbol entityTypeSymbol, INamedTypeSymbol interfaceTypeSymbol)
    {
        List<string> parameterTypes = new() { entityTypeSymbol.ToString() };
        foreach (ITypeSymbol? typeArgument in interfaceTypeSymbol.TypeArguments)
        {
            parameterTypes.Add(typeArgument.ToString());
        }

        parameterTypes.AddRange(systemMethodData.ExtraParameterTypes);

        return namedTypeSymbol.HasMethodWithParams(systemMethodData.MethodName, parameterTypes.ToArray());
    }

    private void AddProperty(ref ImmutableDictionary<string, string?>.Builder? builder, string methodMetaName, string methodArgs)
    {
        if (builder == null)
        {
            builder = ImmutableDictionary.CreateBuilder<string, string?>();
        }

        // 同一接口的多个泛型实例化会命中同一个方法名，只保留第一次
        if (builder.ContainsKey(methodMetaName))
        {
            return;
        }

        if (builder.TryGetValue(Definition.EntitySystemInterfaceSequence, out string? seqValue))
        {
            builder[Definition.EntitySystemInterfaceSequence] = $"{seqValue}/{methodMetaName}";
        }
        else
        {
            builder.Add(Definition.EntitySystemInterfaceSequence, methodMetaName);
        }

        builder.Add(methodMetaName, methodArgs);
    }

    private void ReportNeedGenerateSystem(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol,
        ref ImmutableDictionary<string, string?>.Builder? builder, DiagnosticDescriptor rule)
    {
        if (builder == null)
        {
            return;
        }

        foreach (SyntaxReference? reference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ClassDeclarationSyntax classDeclarationSyntax)
            {
                // 直接引用具体规则，动态取用会让 RS1005 无法静态确认它已声明在 SupportedDiagnostics 中
                Diagnostic diagnostic = rule == UIEntitySystemAnalyzerRule.Rule
                        ? Diagnostic.Create(UIEntitySystemAnalyzerRule.Rule, classDeclarationSyntax.Identifier.GetLocation(),
                            builder.ToImmutable(), namedTypeSymbol.Name)
                        : Diagnostic.Create(EntitySystemAnalyzerRule.Rule, classDeclarationSyntax.Identifier.GetLocation(),
                            builder.ToImmutable(), namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private void AnalyzeIsSystemMethodValid(SymbolAnalysisContext context)
    {
        if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
        {
            return;
        }

        foreach (ISymbol? symbol in namedTypeSymbol.GetMembers())
        {
            if (symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            foreach (RDSystemData rdSystemData in SupportedSystemDatas)
            {
                if (!methodSymbol.HasAttribute(rdSystemData.SystemAttributeMetaName))
                {
                    continue;
                }

                if (methodSymbol.Parameters.Length==0)
                {
                    continue;
                }

                AttributeData? attr = namedTypeSymbol.GetFirstAttribute(rdSystemData.SystemOfAttribute);
                if (attr == null || attr.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol
                    || entityTypeSymbol.ToString()!=methodSymbol.Parameters[0].Type.ToString())
                {
                    ReportNeedSystemOfAttr(context,methodSymbol,rdSystemData);
                }
            }
        }
    }

    private void ReportNeedSystemOfAttr(SymbolAnalysisContext context, IMethodSymbol methodSymbol,RDSystemData rdSystemData)
    {
        foreach (SyntaxReference? reference in methodSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                // 同上，直接引用具体规则以通过 RS1005
                Diagnostic diagnostic = rdSystemData.MethodNeedSystemOfAttrRule == UIEntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule
                        ? Diagnostic.Create(UIEntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule, methodDeclarationSyntax.Identifier.GetLocation()
                            ,methodSymbol.Name, rdSystemData.SystemAttributeShowName,rdSystemData.SystemOfAttribute)
                        : Diagnostic.Create(EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule, methodDeclarationSyntax.Identifier.GetLocation()
                            ,methodSymbol.Name, rdSystemData.SystemAttributeShowName,rdSystemData.SystemOfAttribute);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

}
