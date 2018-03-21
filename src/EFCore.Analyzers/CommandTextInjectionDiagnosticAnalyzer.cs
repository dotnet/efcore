// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommandTextInjectionDiagnosticAnalyzer : SqlInjectionDiagnosticAnalyzerBase
    {
        public const string Id = "EF1001";

        private static readonly DiagnosticDescriptor _descriptor
            = new DiagnosticDescriptor(
                Id,
                title: DefaultTitle,
                messageFormat: MessageFormat,
                category: Category,
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: false);

        protected override DiagnosticDescriptor DiagnosticDescriptor { get; } = _descriptor;

        protected override void AnalyzeMember(
            SyntaxNodeAnalysisContext analysisContext,
            string identifierValueText,
            MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            if (identifierValueText == "CommandText")
            {
                var symbol = analysisContext.GetSymbol(memberAccessExpressionSyntax);

                switch (symbol)
                {
                    case null:
                        return;
                    case IPropertySymbol propertySymbol:

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

                        break;
                }
            }
        }
    }
}
