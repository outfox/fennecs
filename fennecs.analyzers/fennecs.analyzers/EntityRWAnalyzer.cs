using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace fennecs.analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RWEntityAnalyzer : DiagnosticAnalyzer
{
    // Rule that defines our diagnostic
    private static readonly DiagnosticDescriptor Rule = new(
        id: "FOX001",
        title: "RW<Entity> usage not allowed",
        messageFormat: "Use R<Entity> instead (plain Components of type Entity identify the entity itself, and are not writable)",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for analyzing generic names
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.GenericName);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var genericName = (GenericNameSyntax) context.Node;
        var typeSymbol = context.SemanticModel.GetSymbolInfo(genericName).Symbol as INamedTypeSymbol;

        if (typeSymbol?.IsGenericType == true &&
            typeSymbol is {Name: "RW", TypeArguments.Length: 1} &&
            typeSymbol.TypeArguments[0].Name == "Entity")
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rule,
                    genericName.GetLocation()));
        }
    }
}