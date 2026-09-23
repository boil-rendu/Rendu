using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Simplification;

namespace RD;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntitySystemCodeFixProvider)), Shared]
public class EntitySystemCodeFixProvider:CodeFixProvider
{
    // 普通 EntitySystem（RD0024）与 UI EntitySystem（UI0001）共用这一套修复逻辑：
    // 两个 Analyzer 都往 Definition.EntitySystemInterfaceSequence 里写同一份协议，
    // 差别只在于 UI 侧的 methodMetaName 额外带了 "|返回类型"。
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
        DiagnosticIds.EntitySystemAnalyzerRuleId,
        DiagnosticIds.UIEntitySystemAnalyzerRuleId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
        Diagnostic diagnostic = context.Diagnostics.First();

        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
        
        ClassDeclarationSyntax? classDeclaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
        
        CodeAction codeAction = CodeAction.Create(
            diagnostic.Id == DiagnosticIds.UIEntitySystemAnalyzerRuleId ? "生成 UI EntitySystem" : "Generate Entity System",
            cancelToken => GenerateEntitySystemAsync(context.Document,classDeclaration,diagnostic,cancelToken), 
            equivalenceKey: nameof(EntitySystemCodeFixProvider));
        context.RegisterCodeFix(codeAction,diagnostic);
    }

    private static async Task<Document> GenerateEntitySystemAsync(Document document, ClassDeclarationSyntax? classDeclaration,Diagnostic diagnostic,
    CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        ImmutableDictionary<string, string?> properties = diagnostic.Properties;

        
        if (classDeclaration==null || root==null)
        {
            return document;
        }

        var newMembers = new SyntaxList<MemberDeclarationSyntax>();
        string? seuqenceStr = properties[Definition.EntitySystemInterfaceSequence];
        if (seuqenceStr==null)
        {
            return document;
        }

        string[] sequenceArr = seuqenceStr.Split('/');
        for (int i = 0; i < sequenceArr.Length; i++)
        {
            string methodName = sequenceArr[i];
            string? methodArgs = properties[methodName];
            if (methodArgs==null)
            {
                continue;
            }
            var methodSyntax = CreateEntitySystemMethodSyntax(methodName, methodArgs);
            if (methodSyntax!=null)
            {
                newMembers = newMembers.Add(methodSyntax);
            }
            else
            {
                throw new Exception("methodSyntax==null");
            }
        }

        if (newMembers.Count==0)
        {
            throw new Exception("newMembers.Count==0");
        }
        
        var newClassDeclaration = classDeclaration.WithMembers(classDeclaration.Members.InsertRange(0, newMembers)).WithAdditionalAnnotations(Formatter.Annotation);
        document = document.WithSyntaxRoot(root.ReplaceNode(classDeclaration, newClassDeclaration));
        document = await CleanupDocumentAsync(document, cancellationToken);
        return document;
    }

    /// <summary>
    /// methodMetaName 有两种形态，分别由各自的 Analyzer 拼出：
    /// 普通 EntitySystem（RD0024）："方法名`泛型参数个数"，例如 Awake`0
    /// UI  EntitySystem（UI0001）："方法名|返回类型`泛型类型名串"，例如 UIClose|async Promise&lt;bool&gt;
    /// 先按 ` 截掉泛型部分、再按 | 拆出方法名与返回类型，一份实现即可同时覆盖两者。
    /// </summary>
    private static MethodDeclarationSyntax? CreateEntitySystemMethodSyntax(string methodName,string methodArgs)
    {
        string[] methodNameArr = methodName.Split('`')[0].Split('|');
        string[] methodArgsArr = methodArgs.Split('/');
        string systemAttr = methodArgsArr[1];
        string args = String.Empty;
        if (methodArgsArr.Length>2)
        {
            for (int i = 2; i < methodArgsArr.Length; i++)
            {
                args += $", {methodArgsArr[i]} param{i - 1}";
            }
        }

        // 没有 "|返回类型" 说明被实现的接口方法本身就是 void
        bool isReturn = methodNameArr.Length >= 2;
        string returnType = String.Empty;
        string returnValue = String.Empty;
        if (isReturn)
        {
            returnType = methodNameArr[1];
            if (returnType.Contains("async"))
            {
                if (returnType.Contains("Promise<"))
                {
                    returnValue = "await Promise.CompletedTask;\nthrow new NotImplementedException();";
                }
                else
                {
                    returnValue = "await Promise.CompletedTask;";
                }
            }
            else
            {
                returnValue = "throw new NotImplementedException();";
            }
        }
        else
        {
            returnType = "void";
        }

        string code = $$"""
        [{{systemAttr}}]
        private static {{returnType}} {{methodNameArr[0]}}(this {{methodArgsArr[0]}} self{{args}})
        {
            {{returnValue}}
        }
""";
        return SyntaxFactory.ParseMemberDeclaration(code) as MethodDeclarationSyntax;
    }
    
    internal static async Task<Document> CleanupDocumentAsync(
    Document document, CancellationToken cancellationToken)
    {
        if (document.SupportsSyntaxTree)
        {
            document = await ImportAdder.AddImportsAsync(
                document, Simplifier.AddImportsAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // format any node with explicit formatter annotation
            document = await Formatter.FormatAsync(document, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // format any elastic whitespace
            document = await Formatter.FormatAsync(document, SyntaxAnnotation.ElasticAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return document;
    }
    
}
