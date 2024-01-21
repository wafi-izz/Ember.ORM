using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Ember.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MigrationChainedFunctionsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MigrationChainedFunctionsAnalyzer";
        internal static readonly LocalizableString Title = "MigrationChainedFunctionsAnalyzer Title";
        internal static readonly LocalizableString MessageFormat = "MigrationChainedFunctionsAnalyzer '{0}'";
        internal const string Category = "MigrationChainedFunctionsAnalyzer Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
        }
    }


    //write line
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConsoleWriteAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CW001";
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Avoid using Console.WriteLine", "Consider using a logging framework instead", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext Context)
        {
            System.Diagnostics.Debug.WriteLine("Analyzer is running");
            Context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext Context)
        {
            var invocationExpression = (InvocationExpressionSyntax)Context.Node;

            var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpression == null)
                return;

            var methodName = memberAccessExpression.Name.Identifier.Text;


            //if (methodName == "CreateColumn" && memberAccessExpression.Expression.ToString() == "Table")
            //{
            //    var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(), "zzzz zzzz zzz zzz erroe zzzz");
            //    Context.ReportDiagnostic(diagnostic);
            //}

            var invocation = (InvocationExpressionSyntax)Context.Node;
            var methodSymbol = Context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            // Check if the method symbol is not null and it is the DataSchema.Create method
            if (methodSymbol != null &&
                methodSymbol.Name == "Create" &&
                methodSymbol.ContainingType?.Name == "DataSchema")
            {
                // Get the callback parameter (second argument of Create method)
                var callbackArgument = invocation.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression as LambdaExpressionSyntax;

                // Check if the callback parameter is a LambdaExpressionSyntax
                if (callbackArgument != null)
                {
                    // Check if CreateColumn is called inside the callback
                    var createColumnCalls = callbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .Where(invocationSyntax =>
                        {
                            var createColumnSymbol = Context.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;
                            return createColumnSymbol != null && createColumnSymbol.Name == "CreateColumn";
                        });

                    foreach (var createColumnCall in createColumnCalls)
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(Rule, createColumnCall.GetLocation()));
                    }
                }
            }

            //InvocationExpressionSyntax IES = (InvocationExpressionSyntax)Context.Node;
            //MemberAccessExpressionSyntax MA = (MemberAccessExpressionSyntax)IES.Expression;
            //if (MA.Name.Identifier.Text == "ReadLine" && MA.Expression.ToString() == "Console")
            //{
            //    Context.ReportDiagnostic(Diagnostic.Create(Rule, IES.GetLocation(), "some message zzzzzz"));
            //}

        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Custom1 : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CC002";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "abc.",
            "abc. 22",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "abc cc.");
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        public override void Initialize(AnalysisContext Context)
        {
            Context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
        }
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext Context)
        {
            MethodDeclarationSyntax MethodDeclaration = (MethodDeclarationSyntax)Context.Node;
            if (MethodDeclaration.Identifier.Text == "Create")
            {
                List<InvocationExpressionSyntax> InvocationList = MethodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
                foreach (InvocationExpressionSyntax Invocation in InvocationList)
                {
                    if (Invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "CreateColumn")
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(Rule, Invocation.GetLocation()));
                    }
                }
            }

        }
    }
}
