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
        private IEnumerable<InvocationExpressionSyntax> GetChainedMethods(InvocationExpressionSyntax invocationExpression)
        {
            var currentExpression = invocationExpression.Expression;

            while (currentExpression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is IdentifierNameSyntax methodName)
                {
                    var invocation = SyntaxFactory.InvocationExpression(memberAccess);
                    yield return invocation;
                }

                currentExpression = memberAccess.Expression;
            }
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
                        #region Both Create and Alter
                        #region Duplicated Primary Key Functions
                        var ChainedMethodsList = CallbackArgument.Body.DescendantNodes().OfType<InvocationExpressionSyntax>();
                        Boolean FirstPrimaryKey = true;
                        foreach (InvocationExpressionSyntax ChainedMethods in ChainedMethodsList)
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
                        #region Duplicated Functions In Same Line
                        foreach (InvocationExpressionSyntax ChainedMethods in ChainedMethodsList)
                        {
                            if (ChainedMethods.Parent is ExpressionStatementSyntax)
                            {
                                List<MemberAccessExpressionSyntax> LinkMethodsList = ChainedMethods.Parent.DescendantNodes().OfType<MemberAccessExpressionSyntax>().GroupBy(x => x.Name.Identifier.Text).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
                                foreach (MemberAccessExpressionSyntax LinkMethod in LinkMethodsList)
                                {
                                    if (LinkMethodsList.First() != LinkMethod)
                                        Context.ReportDiagnostic(Diagnostic.Create(Rule, LinkMethod.GetLocation(), $"You Can't Use The Function {LinkMethod.Name.Identifier.Text} Twice In The Same Line"));
                                }
                            }
                        }
                        #endregion
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
                            #region create must always start with datatype
                            List<String> SchemaDataTypeList = new List<String>() { "Integer", "String", "Varchar", "Boolean", "Date", "Time", "DateTime", "Timestamp", "Xml", "Binary", "VarBinary", "Geometry", "File", "Image" };
                            List<InvocationExpressionSyntax> CreateChainedMethodsList = CallbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                .Where(InvocationSyntax => InvocationSyntax.Parent is ExpressionStatementSyntax && !SchemaDataTypeList.Any(x => InvocationSyntax.ToString().Contains(x))).ToList();
                            foreach (InvocationExpressionSyntax CreateChainedMethods in CreateChainedMethodsList)
                            {
                                Context.ReportDiagnostic(Diagnostic.Create(Rule, CreateChainedMethods.GetLocation(), $"Table Creation Must Start With a DataType ({String.Join(" - ", SchemaDataTypeList)}) "));
                            }
                            #endregion
                        }
                        if (MethodName.Name.Identifier.Text == "Alter")
                        {
                            #region Alter Standard Start Function
                            List<InvocationExpressionSyntax> AlterColumnCalls = CallbackArgument.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                .Where(InvocationSyntax => InvocationSyntax.Parent is ExpressionStatementSyntax && !(InvocationSyntax.ToString().Contains("CreateColumn") || InvocationSyntax.ToString().Contains("AlterColumn"))).ToList();
                            foreach (InvocationExpressionSyntax AlterColumnCall in AlterColumnCalls)
                            {
                                Context.ReportDiagnostic(Diagnostic.Create(Rule, AlterColumnCall.GetLocation(), "Table Alteration Must Start With either 'AlterColumn' or 'CreateColumn'"));
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
}
