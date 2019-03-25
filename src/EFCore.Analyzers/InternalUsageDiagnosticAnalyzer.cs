// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InternalUsageDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "EF1001";

        public const string MessageFormat
            = "{0} is an internal API that supports the Entity Framework Core infrastructure and " +
              "not subject to the same compatibility standards as public APIs. " +
              "It may be changed or removed without notice in any release.";

        protected const string DefaultTitle = "Internal EF Core API usage.";
        protected const string Category = "Usage";

        static readonly int EFLen = "EntityFrameworkCore".Length;

        private static readonly DiagnosticDescriptor _descriptor
            = new DiagnosticDescriptor(
                Id,
                title: DefaultTitle,
                messageFormat: MessageFormat,
                category: Category,
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode,
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            switch (context.Node)
            {
                case MemberAccessExpressionSyntax memberAccessSyntax:
                {
                    if (context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol is ISymbol symbol &&
                        symbol.ContainingAssembly != context.Compilation.Assembly)
                    {
                        var containingType = symbol.ContainingType;

                        if (HasInternalAttribute(symbol))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_descriptor, memberAccessSyntax.Name.GetLocation(), $"{containingType}.{symbol.Name}"));
                            return;
                        }

                        if (IsInInternalNamespace(containingType) || HasInternalAttribute(containingType))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_descriptor, memberAccessSyntax.Name.GetLocation(), containingType));
                            return;
                        }
                    }
                    return;
                }

                case ObjectCreationExpressionSyntax creationSyntax:
                {
                    if (context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol is ISymbol symbol &&
                        symbol.ContainingAssembly != context.Compilation.Assembly)
                    {
                        var containingType = symbol.ContainingType;

                        if (HasInternalAttribute(symbol))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_descriptor, creationSyntax.GetLocation(), containingType));
                            return;
                        }

                        if (IsInInternalNamespace(containingType) || HasInternalAttribute(containingType))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_descriptor, creationSyntax.Type.GetLocation(), containingType));
                            return;
                        }
                    }

                    return;
                }

                case ClassDeclarationSyntax declarationSyntax:
                {
                    if (context.SemanticModel.GetDeclaredSymbol(declarationSyntax)?.BaseType is ISymbol symbol &&
                        symbol.ContainingAssembly != context.Compilation.Assembly &&
                        (IsInInternalNamespace(symbol) || HasInternalAttribute(symbol)) &&
                        declarationSyntax.BaseList?.Types.Count > 0)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(_descriptor, declarationSyntax.BaseList.Types[0].GetLocation(), symbol));
                    }

                    return;
                }
            }
        }

        private static bool HasInternalAttribute(ISymbol symbol)
            => symbol != null && symbol.GetAttributes().Any(a => a.AttributeClass.Name == "EntityFrameworkInternalAttribute");

        private static bool IsInInternalNamespace(ISymbol symbol)
        {
            if (symbol?.ContainingNamespace?.ToDisplayString() is string ns)
            {
                var i = ns.IndexOf("EntityFrameworkCore");

                return
                    i != -1 &&
                    (i == 0 || ns[i - 1] == '.') &&
                    i + EFLen < ns.Length && ns[i + EFLen] == '.' &&
                    ns.EndsWith(".Internal", StringComparison.Ordinal);
            }

            return false;
        }
    }
}
