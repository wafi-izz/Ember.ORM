using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataSchemaManager.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataStructureAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MyAnalyzerRule";
    private const string Title = "MyAnalyzer";
    private const string MessageFormat = "CreateColumn is not recommended after other alterations. Check if it is intended.";
    private const string Category = "Usage";

    private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        System.Diagnostics.Debug.WriteLine("Analyzer is running");
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IfStatement);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        // Replace this with your own logic to check the condition
        if (ifStatement.Condition != null && ifStatement.Condition.ToString().Contains("CreateColumn"))
        {
            var diagnostic = Diagnostic.Create(Rule, ifStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConsoleWriteAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CW001";
    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Avoid using Console.WriteLine", "Consider using a logging framework instead", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        System.Diagnostics.Debug.WriteLine("Analyzer is running");
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
        if (memberAccessExpression == null)
            return;

        var methodName = memberAccessExpression.Name.Identifier.Text;

        if (methodName == "WriteLine" && memberAccessExpression.Expression.ToString() == "Console")
        {
            var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(), "Consider using a logging framework instead");
            context.ReportDiagnostic(diagnostic);
        }
    }
}