// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Data.Entity.Internal
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MakeGenericMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EF1000";

        public static readonly LocalizableString Title = AnalyzerStrings.AnalyzerTitle;

        public static readonly LocalizableString MessageFormat = AnalyzerStrings.UnsafeMakeGenericMethod;
        public static readonly LocalizableString Description = AnalyzerStrings.UnsafeMakeGenericMethod;

        public const string Category = ".NET Native";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        private static readonly string _disableInspectionComment = AnalyzerStrings.DisableInspectionComment(nameof(MethodInfo.MakeGenericMethod));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
            => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpr?.Name.ToString() != nameof(MethodInfo.MakeGenericMethod))
            {
                return;
            }

            var memberSymbol =
                context.SemanticModel.GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;
            if (!memberSymbol?.ToString().
                StartsWith("System.Reflection.MethodInfo.MakeGenericMethod") ?? true)
            {
                return;
            }

            var methodDeclaration = context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (methodDeclaration.GetLeadingTrivia()
                .Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia)
                          &&
                          t.ToString().Contains(_disableInspectionComment)))
            {
                return;
            }

            if (methodDeclaration.AttributeLists.SelectMany(l => l.Attributes).Any(attribute => attribute.Name.ToString() == "GenericMethodFactory"))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation()));
        }
    }
}
