using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace fennecs.analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RefOutEntityAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: "FOX002",
        title: "Ref/Out Entity parameters not allowed in delegates",
        messageFormat: "Usage of {0} Entity parameters is not allowed in delegates",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Delegates cannot have ref or out Entity parameters, use 'in' instead if needed.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for analyzing parameter lists in both delegate declarations
        // and lambda expressions
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.ParameterList);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var parameterList = (ParameterListSyntax) context.Node;

        // Check if we're in a delegate or lambda context
        if (!IsInDelegateContext(parameterList))
            return;

        foreach (var parameter in parameterList.Parameters)
        {
            // Skip if no ref/out modifier
            if (!parameter.Modifiers.Any(m =>
                    m.IsKind(SyntaxKind.RefKeyword) ||
                    m.IsKind(SyntaxKind.OutKeyword)))
                continue;

            // Get the type symbol
            if (parameter.Type == null) continue;

            var typeSymbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, parameter.Type)
                .Symbol as INamedTypeSymbol;

            if (typeSymbol?.Name != "Entity") continue;

            var modifier = parameter.Modifiers.First(m =>
                m.IsKind(SyntaxKind.RefKeyword) ||
                m.IsKind(SyntaxKind.OutKeyword));

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rule,
                    parameter.GetLocation(),
                    modifier.ValueText));
        }
    }

    private static bool IsInDelegateContext(ParameterListSyntax parameterList)
    {
        var parent = parameterList.Parent;
        return parent is DelegateDeclarationSyntax ||
               parent is ParenthesizedLambdaExpressionSyntax ||
               (parent is MethodDeclarationSyntax method &&
                method.Parent is DelegateDeclarationSyntax);
    }
}