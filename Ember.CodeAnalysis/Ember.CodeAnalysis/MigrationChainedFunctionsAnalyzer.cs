using Microsoft.CodeAnalysis;
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

/*general for TODO:
 * function name should never repeat in same line
 * in Create - line should always start with dataType
 * in Alter - line should always start with AlterColumn or CreateColumn
 */

namespace Ember.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CreateAlterBase : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MT000";
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "", "'{0}' ", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        public override void Initialize(AnalysisContext Context)
        {
            Context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }
        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext Context)
        {
            InvocationExpressionSyntax InvocationExpression = (InvocationExpressionSyntax)Context.Node;
            MemberAccessExpressionSyntax MethodName = (MemberAccessExpressionSyntax)InvocationExpression.Expression;
            #region Table Create Alter Functions Arguments
            if (new String[] { "Create", "Alter" }.Contains(MethodName.Name.Identifier.Text))
            {
                IMethodSymbol MethodSymbol = (IMethodSymbol)Context.SemanticModel.GetSymbolInfo(MethodName).Symbol;
                if (MethodSymbol.ContainingType?.Name == "DataSchema")
                {
                    LambdaExpressionSyntax CallbackArgument = null;
                    if (MethodName.Name.Identifier.Text == "Create")
                        CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression;
                    else if (MethodName.Name.Identifier.Text == "Alter")
                        CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(2)?.Expression;
                    if (CallbackArgument != null)
                    {
                        //Both Create and Alter
                        #region Duplicated Primary Key Functions
                        var ChainedMethodsList = CallbackArgument.Body.DescendantNodes().OfType<InvocationExpressionSyntax>();
                        Boolean FirstPrimaryKey = true;
                        foreach ((InvocationExpressionSyntax ChainedMethods, Int32 Index) in ChainedMethodsList.Select((methodCall, Index) => (methodCall, Index)))
                        {
                            if (ChainedMethods.Parent is ExpressionStatementSyntax)
                            {
                                if (!FirstPrimaryKey && ChainedMethods.ToString().Contains(".PrimaryKey"))
                                    Context.ReportDiagnostic(Diagnostic.Create(Rule, ChainedMethods.GetLocation(), "Table Must Contain Only One PrimaryKey Column."));
                                if (FirstPrimaryKey && ChainedMethods.ToString().Contains(".PrimaryKey"))
                                    FirstPrimaryKey = false;
                            }
                        }
                        #endregion

                        if (MethodName.Name.Identifier.Text == "Create")
                        {
                            #region Alter specific Functions
                            List<InvocationExpressionSyntax> CreateColumnCalls = CallbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
                            .Where(InvocationSyntax => new String[] { "CreateColumn", "AlterColumn" }.Contains((InvocationSyntax.Expression as MemberAccessExpressionSyntax).Name.Identifier.Text)).ToList();
                            foreach (InvocationExpressionSyntax CreateColumnCall in CreateColumnCalls)
                            {
                                Context.ReportDiagnostic(Diagnostic.Create(Rule, CreateColumnCall.GetLocation(), "this method can only be used inside an 'Alter' function"));
                            }
                            #endregion
                            #region Identity Type Only Integer
                            IEnumerable<InvocationExpressionSyntax> PKCallList = CallbackArgument.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                                .Where(MemberAccess =>
                                        MemberAccess.Name.ToString() == "Identity" &&
                                        MemberAccess.Expression is InvocationExpressionSyntax &&
                                        !((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".Integer"))
                                .Select(MemberAccess => (InvocationExpressionSyntax)MemberAccess.Expression);
                            foreach (InvocationExpressionSyntax PKCall in PKCallList)
                            {
                                Context.ReportDiagnostic(Diagnostic.Create(Rule, PKCall.GetLocation(), "Identity column must be of data type Integer Decimal or Numeric"));
                            }
                            #endregion
                            #region ForeignKeys Must Be In Order
                            ChainedMethodsList = CallbackArgument.Body.DescendantNodes().OfType<InvocationExpressionSyntax>();
                            foreach ((InvocationExpressionSyntax ChainedMethods, Int32 Index) in ChainedMethodsList.Select((methodCall, Index) => (methodCall, Index)))
                            {
                                if (ChainedMethods.Parent is ExpressionStatementSyntax)
                                {
                                    foreach (InvocationExpressionSyntax LinkMethod in ChainedMethods.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                        .Where(Method => new String[] { "ForeignKey", "References", "On" }.Contains(((MemberAccessExpressionSyntax)Method.Expression).Name.ToString())))
                                    {
                                        if (LinkMethod.Parent.Parent.Parent is ExpressionStatementSyntax)
                                            if (!LinkMethod.ToString().Contains("ForeignKey") || !LinkMethod.ToString().Contains("References") || !LinkMethod.ToString().Contains("On"))
                                                Context.ReportDiagnostic(Diagnostic.Create(Rule, LinkMethod.GetLocation(), LinkMethod));
                                    }
                                }
                            }
                            #endregion
                        }
                        if (MethodName.Name.Identifier.Text == "Alter")
                        {
                            #region Alter Standard Start Function
                            List<InvocationExpressionSyntax> CreateColumnCalls = CallbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                .Where(InvocationSyntax => InvocationSyntax.Parent is ExpressionStatementSyntax && !(InvocationSyntax.ToString().Contains("CreateColumn") || InvocationSyntax.ToString().Contains("AlterColumn"))).ToList();
                            foreach (InvocationExpressionSyntax CreateColumnCall in CreateColumnCalls)
                            {
                                Context.ReportDiagnostic(Diagnostic.Create(Rule, CreateColumnCall.GetLocation(), "Table Alteration Must Start With either 'AlterColumn' or 'CreateColumn'"));
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion
            //TODO: List other similar cases.
        }
    }
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public class CreateAlterRelatedMethods : DiagnosticAnalyzer
    //{
    //    public const string DiagnosticId = "MT001";
    //    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Function Is Not Allowed In This Operation", "'{0}' ", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
    //    public override void Initialize(AnalysisContext Context)
    //    {
    //        Context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
    //    }
    //    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext Context)
    //    {
    //        InvocationExpressionSyntax InvocationExpression = (InvocationExpressionSyntax)Context.Node;
    //        MemberAccessExpressionSyntax MethodName = (MemberAccessExpressionSyntax)InvocationExpression.Expression;
    //        #region Table Create Function
    //        if (MethodName.Name.Identifier.Text == "Create")
    //        {
    //            IMethodSymbol MethodSymbol = (IMethodSymbol)Context.SemanticModel.GetSymbolInfo(MethodName).Symbol;
    //            if (MethodSymbol.ContainingType?.Name == "DataSchema")
    //            {
    //                LambdaExpressionSyntax CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression;
    //                if (CallbackArgument != null)
    //                {
    //                    #region Alter specific Functions
    //                    List<InvocationExpressionSyntax> CreateColumnCalls = CallbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
    //                        .Where(InvocationSyntax => new String[] { "CreateColumn", "AlterColumn" }.Contains((InvocationSyntax.Expression as MemberAccessExpressionSyntax).Name.Identifier.Text)).ToList();
    //                    foreach (InvocationExpressionSyntax CreateColumnCall in CreateColumnCalls)
    //                    {
    //                        Context.ReportDiagnostic(Diagnostic.Create(Rule, CreateColumnCall.GetLocation(), "this method can only be used inside an 'Alter' function"));
    //                    }
    //                    #endregion
    //                }
    //            }
    //        }
    //        else if (MethodName.Name.Identifier.Text == "Alter")
    //        {
    //            IMethodSymbol MethodSymbol = (IMethodSymbol)Context.SemanticModel.GetSymbolInfo(MethodName).Symbol;
    //            if (MethodSymbol.ContainingType?.Name == "DataSchema")
    //            {
    //                LambdaExpressionSyntax CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(2)?.Expression;
    //                if (CallbackArgument != null)
    //                {
    //                    #region Alter specific Functions
    //                    List<InvocationExpressionSyntax> CreateColumnCalls = CallbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
    //                        .Where(InvocationSyntax => InvocationSyntax.Parent is ExpressionStatementSyntax && !(InvocationSyntax.ToString().Contains("CreateColumn") || InvocationSyntax.ToString().Contains("AlterColumn"))).ToList();
    //                    foreach (InvocationExpressionSyntax CreateColumnCall in CreateColumnCalls)
    //                    {
    //                        Context.ReportDiagnostic(Diagnostic.Create(Rule, CreateColumnCall.GetLocation(), "Table Alteration Must Start With either 'AlterColumn' or 'CreateColumn'"));
    //                    }
    //                    #endregion
    //                }
    //            }
    //        }
    //        #endregion
    //        //TODO: List other similar cases.
    //    }
    //    private void CallbackArgument(LambdaExpressionSyntax CallbackArgument)
    //    {

    //    }
    //}
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public class RelatedMethodChaining : DiagnosticAnalyzer
    //{
    //    public const string DiagnosticId = "MT002";
    //    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Function Is Not Allowed In This Order", "'{0}' ", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
    //    public override void Initialize(AnalysisContext Context)
    //    {
    //        Context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
    //    }
    //    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext Context)
    //    {
    //        InvocationExpressionSyntax InvocationExpression = (InvocationExpressionSyntax)Context.Node;
    //        MemberAccessExpressionSyntax MethodName = (MemberAccessExpressionSyntax)InvocationExpression.Expression;
    //        #region Table Create Function
    //        if (MethodName.Name.Identifier.Text == "Create")
    //        {
    //            IMethodSymbol MethodSymbol = (IMethodSymbol)Context.SemanticModel.GetSymbolInfo(MethodName).Symbol;
    //            if (MethodSymbol.ContainingType?.Name == "DataSchema")
    //            {
    //                LambdaExpressionSyntax CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression;
    //                if (CallbackArgument != null)
    //                {
    //                    #region Identity Type Only Integer
    //                    IEnumerable<InvocationExpressionSyntax> PKCallList = CallbackArgument.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
    //                        .Where(MemberAccess =>
    //                                MemberAccess.Name.ToString() == "Identity" &&
    //                                MemberAccess.Expression is InvocationExpressionSyntax &&
    //                                !((InvocationExpressionSyntax)MemberAccess.Expression).ToString().Contains(".Integer"))
    //                        .Select(MemberAccess => (InvocationExpressionSyntax)MemberAccess.Expression);
    //                    foreach (InvocationExpressionSyntax PKCall in PKCallList)
    //                    {
    //                        Context.ReportDiagnostic(Diagnostic.Create(Rule, PKCall.GetLocation(), "Identity column must be of data type Integer Decimal or Numeric"));
    //                    }
    //                    #endregion
    //                    #region ForeignKeys Must Be In Order
    //                    var ChainedMethodsList = CallbackArgument.Body.DescendantNodes().OfType<InvocationExpressionSyntax>();
    //                    foreach ((InvocationExpressionSyntax ChainedMethods, Int32 Index) in ChainedMethodsList.Select((methodCall, Index) => (methodCall, Index)))
    //                    {
    //                        if (ChainedMethods.Parent is ExpressionStatementSyntax)
    //                        {
    //                            foreach (InvocationExpressionSyntax LinkMethod in ChainedMethods.DescendantNodes().OfType<InvocationExpressionSyntax>()
    //                                .Where(Method => new String[] { "ForeignKey", "References", "On" }.Contains(((MemberAccessExpressionSyntax)Method.Expression).Name.ToString())))
    //                            {
    //                                if (LinkMethod.Parent.Parent.Parent is ExpressionStatementSyntax)
    //                                    if (!LinkMethod.ToString().Contains("ForeignKey") || !LinkMethod.ToString().Contains("References") || !LinkMethod.ToString().Contains("On"))
    //                                        Context.ReportDiagnostic(Diagnostic.Create(Rule, LinkMethod.GetLocation(), LinkMethod));
    //                            }
    //                        }
    //                    }
    //                    #endregion
    //                    #region New Stuff
    //                    //TODO: List other similar cases
    //                    #endregion
    //                }
    //            }
    //        }
    //        #endregion
    //    }
    //}
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public class MethodDuplications : DiagnosticAnalyzer
    //{
    //    public const string DiagnosticId = "MT003";
    //    internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Function Is Only Allowed Once", "'{0}' ", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
    //    public override void Initialize(AnalysisContext Context)
    //    {
    //        Context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
    //    }
    //    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext Context)
    //    {
    //        InvocationExpressionSyntax InvocationExpression = (InvocationExpressionSyntax)Context.Node;
    //        MemberAccessExpressionSyntax MethodName = (MemberAccessExpressionSyntax)InvocationExpression.Expression;
    //        #region Table Create Function
    //        if (new String[] { "Create", "Alter" }.Contains(MethodName.Name.Identifier.Text))
    //        {
    //            IMethodSymbol methodSymbol = (IMethodSymbol)Context.SemanticModel.GetSymbolInfo(MethodName).Symbol;
    //            if (methodSymbol.ContainingType?.Name == "DataSchema")
    //            {
    //                LambdaExpressionSyntax CallbackArgument = null;
    //                if (MethodName.Name.Identifier.Text == "Create")
    //                    CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression;
    //                else if (MethodName.Name.Identifier.Text == "Alter")
    //                    CallbackArgument = (LambdaExpressionSyntax)InvocationExpression.ArgumentList.Arguments.ElementAtOrDefault(2)?.Expression;
    //                if (CallbackArgument != null)
    //                {
    //                    #region Duplicated Functions
    //                    var ChainedMethodsList = CallbackArgument.Body.DescendantNodes().OfType<InvocationExpressionSyntax>();
    //                    Boolean FirstPrimaryKey = true;
    //                    foreach ((InvocationExpressionSyntax ChainedMethods, Int32 Index) in ChainedMethodsList.Select((methodCall, Index) => (methodCall, Index)))
    //                    {
    //                        if (ChainedMethods.Parent is ExpressionStatementSyntax)
    //                        {
    //                            if (!FirstPrimaryKey && ChainedMethods.ToString().Contains(".PrimaryKey"))
    //                                Context.ReportDiagnostic(Diagnostic.Create(Rule, ChainedMethods.GetLocation(), "Table Must Contain Only One PrimaryKey Column."));
    //                            if (FirstPrimaryKey && ChainedMethods.ToString().Contains(".PrimaryKey"))
    //                                FirstPrimaryKey = false;
    //                        }
    //                    }
    //                    #endregion
    //                }
    //            }
    //        }
    //        #endregion
    //        //TODO: List other similar cases.
    //    }
    //}
    #region Reference Code
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public class MigrationChainedFunctionsAnalyzer : DiagnosticAnalyzer
    //{
    //    public const string DiagnosticId = "MigrationChainedFunctionsAnalyzer";
    //    internal static readonly LocalizableString Title = "MigrationChainedFunctionsAnalyzer Title";
    //    internal static readonly LocalizableString MessageFormat = "MigrationChainedFunctionsAnalyzer '{0}'";
    //    internal const string Category = "MigrationChainedFunctionsAnalyzer Category";

    //    internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    //    public override void Initialize(AnalysisContext context)
    //    {
    //        context.RegisterSyntaxNodeAction(SomeName, SyntaxKind.InvocationExpression);
    //    }
    //    public void SomeName(SyntaxNodeAnalysisContext context)
    //    {

    //    }
    //}

    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public class Custom1 : DiagnosticAnalyzer
    //{
    //    public const string DiagnosticId = "CC002";
    //    private const string Category = "Usage";

    //    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
    //        DiagnosticId,
    //        "abc.",
    //        "abc. 22",
    //        Category,
    //        DiagnosticSeverity.Error,
    //        isEnabledByDefault: true,
    //        description: "abc cc.");
    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
    //    public override void Initialize(AnalysisContext Context)
    //    {
    //        Context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
    //    }
    //    private static void AnalyzeSyntax(SyntaxNodeAnalysisContext Context)
    //    {
    //        MethodDeclarationSyntax MethodDeclaration = (MethodDeclarationSyntax)Context.Node;
    //        if (MethodDeclaration.Identifier.Text == "Create")
    //        {
    //            List<InvocationExpressionSyntax> InvocationList = MethodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
    //            foreach (InvocationExpressionSyntax Invocation in InvocationList)
    //            {
    //                if (Invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "CreateColumn")
    //                {
    //                    Context.ReportDiagnostic(Diagnostic.Create(Rule, Invocation.GetLocation()));
    //                }
    //            }
    //        }

    //    }



    //    //var mm = memberAccessExpression.Name.Identifier.Text;
    //    //if (mm == "CreateColumn" && memberAccessExpression.Expression.ToString() == "Table")
    //    //{
    //    //    var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(), "zzzz zzzz zzz zzz erroe zzzz");
    //    //    Context.ReportDiagnostic(diagnostic);
    //    //}


    //    //InvocationExpressionSyntax IES = (InvocationExpressionSyntax)Context.Node;
    //    //MemberAccessExpressionSyntax MA = (MemberAccessExpressionSyntax)IES.Expression;
    //    //if (MA.Name.Identifier.Text == "ReadLine" && MA.Expression.ToString() == "Console")
    //    //{
    //    //    Context.ReportDiagnostic(Diagnostic.Create(Rule, IES.GetLocation(), "some message zzzzzz"));
    //    //}


    //}
    #endregion
}
