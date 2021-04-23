// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NullableDiagnosticSuppressor : DiagnosticSuppressor
    {
        private static readonly ImmutableArray<OperationKind> _invocationKind = ImmutableArray.Create(OperationKind.Invocation);

        private static readonly SuppressionDescriptor _descriptor = new(
            id: "EFS0001",
            suppressedDiagnosticId: "CS8602",
            justification: "EF Core provides null-safety in translated expression trees.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(_descriptor);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            var linqExpressionType = context.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");
            if (linqExpressionType is null)
            {
                return;
            }

            var efQueryableExtensionsType = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
            if (efQueryableExtensionsType is null)
            {
                return;
            }

            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                if (diagnostic.Location.SourceTree is not SyntaxTree tree)
                {
                    continue;
                }

                var root = tree.GetRoot(context.CancellationToken);
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                var model = context.GetSemanticModel(tree);
                if (model.GetOperation(node, context.CancellationToken) is not IOperation operation)
                {
                    continue;
                }

                if (operation.IsWithinExpressionTree(linqExpressionType, out var anonymousOrLocalFunctionOperation) &&
                    anonymousOrLocalFunctionOperation.GetAncestor(_invocationKind) is IInvocationOperation invocation &&
                    SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, efQueryableExtensionsType))
                {
                    context.ReportSuppression(Suppression.Create(_descriptor, diagnostic));
                }
            }
        }
    }
}
