// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RawSqlStringInjectionDiagnosticAnalyzer : SqlInjectionDiagnosticAnalyzerBase
    {
        public const string Id = "EF1000";

        private static readonly DiagnosticDescriptor _descriptor
            = new DiagnosticDescriptor(
                Id,
                title: DefaultTitle,
                messageFormat: MessageFormat,
                category: Category,
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        protected override DiagnosticDescriptor DiagnosticDescriptor { get; } = _descriptor;

        protected override void AnalyzeMember(
            SyntaxNodeAnalysisContext analysisContext,
            string identifierValueText,
            MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            if (identifierValueText == "FromSql"
                || identifierValueText == "ExecuteSqlCommand"
                || identifierValueText == "ExecuteSqlCommandAsync")
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

                    var depth = 0;

                    CheckPossibleInjection(
                        analysisContext,
                        sqlArgumentExpressionSyntax,
                        identifierValueText,
                        invocationExpressionSyntax.GetLocation(),
                        visited: new HashSet<SyntaxNode>(),
                        ref depth);
                }
            }
        }
    }
}
