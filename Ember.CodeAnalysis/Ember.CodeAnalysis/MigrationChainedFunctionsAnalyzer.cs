﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Ember.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CreateAlterRelatedMethods : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MT001";
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Function Is Not Allowed In This Operation", "'{0}' ", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        public override void Initialize(AnalysisContext Context)
        {
            Context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }
        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext Context)
        {
            InvocationExpressionSyntax invocationExpression = (InvocationExpressionSyntax)Context.Node;
            MemberAccessExpressionSyntax MethodName = (MemberAccessExpressionSyntax)invocationExpression.Expression;
            if (MethodName.Name.Identifier.Text == "Create")
            {
                IMethodSymbol methodSymbol = (IMethodSymbol)Context.SemanticModel.GetSymbolInfo(MethodName).Symbol;
                if (methodSymbol.ContainingType?.Name == "DataSchema")
                {
                    LambdaExpressionSyntax callbackArgument = (LambdaExpressionSyntax)invocationExpression.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression;
                    if (callbackArgument != null)
                    {
                        List<InvocationExpressionSyntax> CreateColumnCalls = callbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
                            .Where(invocationSyntax => (invocationSyntax.Expression as MemberAccessExpressionSyntax).Name.Identifier.Text == "CreateColumn").ToList();
                        foreach (InvocationExpressionSyntax createColumnCall in CreateColumnCalls)
                        {
                            Context.ReportDiagnostic(Diagnostic.Create(Rule, createColumnCall.GetLocation(), "name =>" + MethodName.GetType().ToString()));
                        }
                    }
                }
            }
            //TODO: List other similar cases.
        }
    }
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RelatedMethodChaining : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MT002";
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Function Is Not Allowed In This Order", "'{0}' ", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        public override void Initialize(AnalysisContext Context)
        {
            Context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }
        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext Context)
        {
            InvocationExpressionSyntax InvocationExpression = (InvocationExpressionSyntax)Context.Node;
            MemberAccessExpressionSyntax MethodName = (MemberAccessExpressionSyntax)InvocationExpression.Expression;
            if (MethodName.Name.Identifier.Text == "Create")
            {
                IMethodSymbol methodSymbol = (IMethodSymbol)Context.SemanticModel.GetSymbolInfo(MethodName).Symbol;
                if (methodSymbol.ContainingType?.Name == "DataSchema")
                {
                    // find the Identity DataType.
                    LambdaExpressionSyntax CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression;
                    if (CallbackArgument != null)
                    {


                        var block = CallbackArgument.Body;
                        var methodCalls = block.DescendantNodes().OfType<InvocationExpressionSyntax>();

                        var tableNames = new HashSet<string>();

                        foreach (var methodCall in methodCalls)
                        {
                            if (methodCall.Expression is MemberAccessExpressionSyntax memberAccess &&
                                memberAccess.Expression is IdentifierNameSyntax identifier &&
                                identifier.Identifier.Text == "Table")
                            {
                                var tableName = methodCall.ArgumentList.Arguments.FirstOrDefault()?.ToString();
                               
                                    
                                        var diagnostic = Diagnostic.Create(Rule, methodCall.GetLocation(), tableName);
                                        Context.ReportDiagnostic(diagnostic);
                                    
                                
                            }
                        }
                        //List<InvocationExpressionSyntax> PKCallList = CallbackArgument.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                        //.Where(MemberAccess =>
                        //    MemberAccess.Name.ToString() == "Identity" &&
                        //    MemberAccess.Expression is InvocationExpressionSyntax &&
                        //    !((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".Integer"))
                        //.Select(MemberAccess => (InvocationExpressionSyntax)MemberAccess.Expression)
                        //.ToList();
                        //foreach (InvocationExpressionSyntax PKCall in PKCallList)
                        //{
                        //    Context.ReportDiagnostic(Diagnostic.Create(Rule, PKCall.GetLocation(), "Identity column must be of data type Integer Decimal or Numeric"));
                        //}
                        //if foreign key make sure all necessary function are present
                        //ForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
                        foreach (MemberAccessExpressionSyntax FKCall in CallbackArgument.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Where(x => x.Name.ToString() == "Integer" && x.Expression is InvocationExpressionSyntax).Select(x => (MemberAccessExpressionSyntax)x))
                        {
                            Context.ReportDiagnostic(Diagnostic.Create(Rule, FKCall.GetLocation(), FKCall.Name.ToString()));
                        }
                        //List<InvocationExpressionSyntax>  FKCallList = CallbackArgument.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                        //.Where(MemberAccess =>
                        //{
                        //    if (MemberAccess.Name.ToString() == "ForeignKey" && MemberAccess.Expression is InvocationExpressionSyntax)
                        //        return true; // !((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".References") || ((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".On");
                        //    if (MemberAccess.Name.ToString() == "References" && MemberAccess.Expression is InvocationExpressionSyntax)
                        //        return true; // !((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".ForeignKey") || ((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".On");
                        //    if (MemberAccess.Name.ToString() == "On" && MemberAccess.Expression is InvocationExpressionSyntax)
                        //        return true; // !((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".ForeignKey") || ((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".References");
                        //    else
                        //        return false;
                        //})
                        //.Select(MemberAccess => (InvocationExpressionSyntax)MemberAccess.Expression)
                        //.ToList();
                        //foreach (InvocationExpressionSyntax FKCall in FKCallList)
                        //{
                        //    //Context.ReportDiagnostic(Diagnostic.Create(Rule, FKCall.GetLocation(), $"{FKCall.Expression.ToString()} ----- ForeignKey Must include all Related Functions"));
                        //}
                        // if found see if it is on the first chain 
                        // if found see if her ancestor is something other than an integer type
                    }
                }
            }
            //TODO: List other similar cases.
        }
    }
    #region Reference Code
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
            context.RegisterSyntaxNodeAction(SomeName, SyntaxKind.InvocationExpression);
        }
        public void SomeName(SyntaxNodeAnalysisContext context)
        {

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



        //var mm = memberAccessExpression.Name.Identifier.Text;
        //if (mm == "CreateColumn" && memberAccessExpression.Expression.ToString() == "Table")
        //{
        //    var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(), "zzzz zzzz zzz zzz erroe zzzz");
        //    Context.ReportDiagnostic(diagnostic);
        //}


        //InvocationExpressionSyntax IES = (InvocationExpressionSyntax)Context.Node;
        //MemberAccessExpressionSyntax MA = (MemberAccessExpressionSyntax)IES.Expression;
        //if (MA.Name.Identifier.Text == "ReadLine" && MA.Expression.ToString() == "Console")
        //{
        //    Context.ReportDiagnostic(Diagnostic.Create(Rule, IES.GetLocation(), "some message zzzzzz"));
        //}


    }
    #endregion
}
