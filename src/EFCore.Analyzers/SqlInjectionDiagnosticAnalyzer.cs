// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EFCore.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SqlInjectionDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "EF1000";

        public const string MessageFormat
            = "The SQL expression passed to '{0}' embeds data that will not be parameterized. Review for potential SQL injection vulnerability.";

        private static readonly DiagnosticDescriptor _descriptor
            = new DiagnosticDescriptor(
                Id,
                title: "Possible SQL injection vulnerability.",
                messageFormat: MessageFormat,
                category: "Security",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(_descriptor);

        public override void Initialize(AnalysisContext analysisContext)
            => analysisContext.RegisterSyntaxNodeAction(
                AnalyzeSimpleMemberAccessExpressionSyntaxNode, SyntaxKind.SimpleMemberAccessExpression);

        private static void AnalyzeSimpleMemberAccessExpressionSyntaxNode(SyntaxNodeAnalysisContext analysisContext)
        {
            if (!(analysisContext.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return;
            }

            var identifierValueText = memberAccessExpressionSyntax.Name.Identifier.ValueText;

            if (identifierValueText == "FromSql"
                || identifierValueText == "ExecuteSqlCommand"
                || identifierValueText == "ExecuteSqlCommandAsync"
                || identifierValueText == "CommandText")
            {
                var symbol = analysisContext.GetSymbol(memberAccessExpressionSyntax);

                if (symbol == null)
                {
                    return;
                }

                var containingType = symbol.ContainingType;
                var containingTypeName = containingType.Name;

                if ((containingTypeName == "RelationalQueryableExtensions"
                     || containingTypeName == "RelationalDatabaseFacadeExtensions")
                    && containingType.ContainingNamespace.Name == "EntityFrameworkCore"
                    && memberAccessExpressionSyntax.Parent is InvocationExpressionSyntax invocationExpressionSyntax
                    && symbol is IMethodSymbol methodSymbol)
                {
                    // FromSql, ExecuteSqlCommand/Async

                    // Extension method vs static invocation
                    var sqlArgumentIndex = methodSymbol.ReducedFrom == null ? 1 : 0;

                    var sqlArgumentExpressionSyntax
                        = invocationExpressionSyntax.ArgumentList.Arguments[sqlArgumentIndex].Expression;

                    // Skip safe FromSql/ExecuteSqlCommand usage - string literal or interpolated string
                    switch (sqlArgumentExpressionSyntax)
                    {
                        case LiteralExpressionSyntax _:
                            return;
                        case InterpolatedStringExpressionSyntax _:
                            return;
                    }

                    CheckPossibleInjection(
                        analysisContext,
                        sqlArgumentExpressionSyntax,
                        identifierValueText,
                        invocationExpressionSyntax.GetLocation());
                }
                else if (symbol is IPropertySymbol propertySymbol)
                {
                    // DbCommand.CommandText

                    var overriddenPropertyContainingType = propertySymbol.OverriddenProperty.ContainingType;

                    if (overriddenPropertyContainingType.Name == "DbCommand"
                        && overriddenPropertyContainingType.ContainingNamespace.Name == "Common"
                        && memberAccessExpressionSyntax.Parent is AssignmentExpressionSyntax assignmentExpressionSyntax)
                    {
                        CheckPossibleInjection(
                            analysisContext,
                            assignmentExpressionSyntax.Right,
                            identifierValueText,
                            assignmentExpressionSyntax.GetLocation());
                    }
                }
            }
        }

        private static bool CheckPossibleInjection(
            SyntaxNodeAnalysisContext analysisContext, SyntaxNode syntaxNode, string identifier, Location location)
        {
            if (UsesUnsafeInterpolation(analysisContext, syntaxNode)
                || UsesUnsafeStringOperation(analysisContext, syntaxNode))
            {
                analysisContext.ReportDiagnostic(
                    Diagnostic.Create(_descriptor, location, identifier));

                return true;
            }

            var rootSyntaxNode
                = syntaxNode.Ancestors().First(n => n is MemberDeclarationSyntax);

            foreach (var identifierNameSyntax in syntaxNode.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
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
                                        location))
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
                                        location))
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

        private static bool UsesUnsafeInterpolation(
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

        private static bool UsesUnsafeStringOperation(
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
