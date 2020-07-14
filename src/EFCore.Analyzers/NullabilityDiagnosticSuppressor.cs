using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class NullabilityDiagnosticSuppressor : DiagnosticSuppressor
    {
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(new SuppressionDescriptor("EFSup-CS8618", "CS8618", "Entity Framework initializes DbSet<T>, so they are not null."));

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var root = diagnostic.Location.SourceTree.GetRoot(context.CancellationToken);
                if (!(root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax node))
                {
                    continue;
                }
                if (!(node.Parent is ClassDeclarationSyntax classDeclaration))
                {
                    continue;
                }

                // TODO: Check if the class derives from DbContext. (Not sure how to do it yet)

                if (!(node.Type is GenericNameSyntax genericSyntax))
                {
                    continue;
                }
                if (genericSyntax.Identifier.Text != "DbSet")
                {
                    continue;
                }

                context.ReportSuppression(Suppression.Create(SupportedSuppressions.First(), diagnostic));
            }
        }
    }
}
