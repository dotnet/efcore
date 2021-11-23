// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UninitializedDbSetDiagnosticSuppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor _suppressUninitializedDbSetRule = new(
        id: "SPR1001",
        suppressedDiagnosticId: "CS8618",
        justification: "DbSet properties on DbContext subclasses are automatically populated by the DbContext constructor");

    /// <inheritdoc />
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; }
        = ImmutableArray.Create(_suppressUninitializedDbSetRule);

    /// <inheritdoc />
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        INamedTypeSymbol? dbSetTypeSymbol = null;
        INamedTypeSymbol? dbContextTypeSymbol = null;

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            // We have an warning about an uninitialized non-nullable property.
            // Get the node, and make sure it's a property who's type syntactically contains DbSet (fast check before getting the
            // semantic model, which is heavier).
            if (diagnostic.Location.SourceTree is not { } sourceTree
                || sourceTree.GetRoot().FindNode(diagnostic.Location.SourceSpan) is not PropertyDeclarationSyntax propertyDeclarationSyntax
                || !propertyDeclarationSyntax.Type.ToString().Contains("DbSet"))
            {
                continue;
            }

            // Get the semantic symbol and do some basic checks
            if (context.GetSemanticModel(sourceTree).GetDeclaredSymbol(propertyDeclarationSyntax) is not IPropertySymbol propertySymbol
                || propertySymbol.IsStatic
                || propertySymbol.IsReadOnly)
            {
                continue;
            }

            if (dbSetTypeSymbol is null || dbContextTypeSymbol is null)
            {
                dbSetTypeSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbSet`1");
                dbContextTypeSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");

                if (dbSetTypeSymbol is null || dbContextTypeSymbol is null)
                {
                    return;
                }
            }

            // Check that the property is actually a DbSet<T>, and that its containing type inherits from DbContext
            if (propertySymbol.Type.OriginalDefinition.Equals(dbSetTypeSymbol, SymbolEqualityComparer.Default)
                && InheritsFrom(propertySymbol.ContainingType, dbContextTypeSymbol))
            {
                context.ReportSuppression(Suppression.Create(SupportedSuppressions[0], diagnostic));
            }

            static bool InheritsFrom(ITypeSymbol typeSymbol, ITypeSymbol baseTypeSymbol)
            {
                var baseType = typeSymbol.BaseType;
                while (baseType is not null)
                {
                    if (baseType.Equals(baseTypeSymbol, SymbolEqualityComparer.Default))
                    {
                        return true;
                    }

                    baseType = baseType.BaseType;
                }

                return false;
            }
        }
    }
}
