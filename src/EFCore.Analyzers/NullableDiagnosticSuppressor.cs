// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NullableDiagnosticSuppressor : DiagnosticSuppressor
    {
        private static readonly SuppressionDescriptor _descriptor = new(
            id: "EFS0001",
            suppressedDiagnosticId: "CS8602",
            justification: "The dereference is safe inside this expression tree.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(_descriptor);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            context.ReportSuppression(Suppression.Create(_descriptor, context.ReportedDiagnostics[0]));


            /*var linqExpressionType = context.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");
            if (linqExpressionType is null)
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
                var operation = model.GetOperation(node, context.CancellationToken);
                if (operation?.IsWithinExpressionTree(linqExpressionType) == true)
                {
                    context.ReportSuppression(Suppression.Create(_descriptor, diagnostic));
                }

            }*/
        }
    }
}
