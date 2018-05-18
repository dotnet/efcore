// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class SqlInjectionDiagnosticAnalyzerBase : DiagnosticAnalyzer
    {
        public const string MessageFormat
            = "The SQL expression passed to '{0}' embeds data that will not be parameterized."
              + " Review for potential SQL injection vulnerability. See https://go.microsoft.com/fwlink/?linkid=871170 for more information.";

        public const int RecursionLimit = 30;

        protected const string DefaultTitle = "Possible SQL injection vulnerability.";
        protected const string Category = "Security";

        protected abstract DiagnosticDescriptor DiagnosticDescriptor { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticDescriptor);

        public override void Initialize(AnalysisContext analysisContext)
            => analysisContext.RegisterSyntaxNodeAction(
                AnalyzeSimpleMemberAccessExpressionSyntaxNode, SyntaxKind.SimpleMemberAccessExpression);

        private void AnalyzeSimpleMemberAccessExpressionSyntaxNode(SyntaxNodeAnalysisContext analysisContext)
        {
            if (!(analysisContext.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return;
            }

            var identifierValueText = memberAccessExpressionSyntax.Name.Identifier.ValueText;

            AnalyzeMember(analysisContext, identifierValueText, memberAccessExpressionSyntax);
        }

        protected abstract void AnalyzeMember(
            SyntaxNodeAnalysisContext analysisContext,
            string identifierValueText,
            MemberAccessExpressionSyntax memberAccessExpressionSyntax);

        protected bool CheckPossibleInjection(
            SyntaxNodeAnalysisContext analysisContext,
            SyntaxNode syntaxNode,
            string identifier,
            Location location,
            ISet<SyntaxNode> visited,
            ref int depth)
        {
            if (UsesUnsafeInterpolation(analysisContext, syntaxNode)
                || UsesUnsafeStringOperation(analysisContext, syntaxNode))
            {
                analysisContext.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptor, location, identifier));

                return true;
            }

            if (visited.Contains(syntaxNode)
                || ++depth > RecursionLimit)
            {
                return false;
            }

            visited.Add(syntaxNode);

            try
            {
                var rootSyntaxNode
                    = syntaxNode.Ancestors().First(n => n is MemberDeclarationSyntax);

                foreach (var identifierNameSyntax
                    in syntaxNode.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                {
                    var symbol = analysisContext.GetSymbol(identifierNameSyntax);

                    if (symbol is ILocalSymbol
                        || symbol is IParameterSymbol)
                    {
                        foreach (var descendantNode in rootSyntaxNode.DescendantNodes())
                        {
                            if (descendantNode == syntaxNode)
                            {
                                break;
                            }

                            switch (descendantNode)
                            {
                                case AssignmentExpressionSyntax assignmentExpressionSyntax
                                    when assignmentExpressionSyntax.Left is IdentifierNameSyntax
                                         && Equals(analysisContext.GetSymbol(assignmentExpressionSyntax.Left), symbol):
                                {
                                    if (CheckPossibleInjection(
                                        analysisContext,
                                        assignmentExpressionSyntax.Right,
                                        identifier,
                                        location,
                                        visited,
                                        ref depth))
                                    {
                                        return true;
                                    }

                                    break;
                                }
                                case VariableDeclaratorSyntax variableDeclaratorSyntax
                                    when Equals(analysisContext.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax), symbol):
                                {
                                    if (CheckPossibleInjection(
                                        analysisContext,
                                        variableDeclaratorSyntax.Initializer,
                                        identifier,
                                        location,
                                        visited,
                                        ref depth))
                                    {
                                        return true;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            
                return false;
            }
            finally
            {
                --depth;
            }
        }

        protected static bool UsesUnsafeInterpolation(
            SyntaxNodeAnalysisContext analysisContext, SyntaxNode syntaxNode)
            => syntaxNode
                .DescendantNodesAndSelf()
                .Any(
                    sn => sn is InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax
                          && interpolatedStringExpressionSyntax
                              .DescendantNodes()
                              .OfType<InterpolationSyntax>()
                              .SelectMany(i => i.DescendantNodesAndSelf())
                              .Any(n => IsLocalOrParameterSymbol(analysisContext, n)));

        protected static bool UsesUnsafeStringOperation(
            SyntaxNodeAnalysisContext analysisContext, SyntaxNode syntaxNode)
            => syntaxNode
                .DescendantNodesAndSelf()
                .Any(
                    sn =>
                    {
                        if (
                            // Test for various string methods
                            sn is InvocationExpressionSyntax invocationExpressionSyntax
                            && invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                            && memberAccessExpressionSyntax.Name.Identifier.ValueText is string identifier
                            && (identifier == "Format"
                                || identifier == "Concat"
                                || identifier == "Insert"
                                || identifier == "Replace"
                                || identifier == "Join")

                            // Test for string '+' operator
                            || sn is BinaryExpressionSyntax binaryExpressionSyntax
                            && binaryExpressionSyntax.OperatorToken.Kind() == SyntaxKind.PlusToken)
                        {
                            var memberSymbol = analysisContext.GetSymbol(sn);

                            if (memberSymbol != null)
                            {
                                var containingType = memberSymbol.ContainingType;
                                var containingTypeName = containingType.Name;

                                if (containingTypeName == "String"
                                    && containingType.ContainingNamespace.Name == "System")
                                {
                                    return sn
                                        .DescendantNodes()
                                        .Any(n => IsLocalOrParameterSymbol(analysisContext, n));
                                }
                            }
                        }

                        return false;
                    });

        private static bool IsLocalOrParameterSymbol(
            SyntaxNodeAnalysisContext analysisContext, SyntaxNode syntaxNode)
        {
            var symbol = analysisContext.GetSymbol(syntaxNode);

            return symbol is ILocalSymbol || symbol is IParameterSymbol;
        }
    }
}
