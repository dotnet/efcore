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
    private static readonly SuppressionDescriptor SuppressUninitializedDbSetRule = new(
        id: "EFSPR1001",
        suppressedDiagnosticId: "CS8618",
        justification: AnalyzerStrings.UninitializedDbSetWarningSuppressionJustification);

    /// <inheritdoc />
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; }
        = ImmutableArray.Create(SuppressUninitializedDbSetRule);

    /// <inheritdoc />
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        INamedTypeSymbol? dbSetTypeSymbol = null;
        INamedTypeSymbol? dbContextTypeSymbol = null;

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            // We have an warning about an uninitialized non-nullable property.

            // CS8618 contains the location of the uninitialized property in AdditionalLocations; note that if the class has a constructor,
            // the diagnostic main Location points to the constructor rather than to the uninitialized property.
            // The AdditionalLocations was added in 7.0.0-preview.3, fall back to the main location just in case and older compiler is
            // being used (the check below for PropertyDeclarationSyntax will filter out the diagnostic if it's pointing to a constructor).
            var location = diagnostic.AdditionalLocations.Count > 0
                ? diagnostic.AdditionalLocations[0]
                : diagnostic.Location;

            // Get the node, and make sure it's a property whose type syntactically contains DbSet (fast check before getting the semantic
            // model, which is heavier).
            if (location.SourceTree is not { } sourceTree
                || sourceTree.GetRoot().FindNode(location.SourceSpan) is not PropertyDeclarationSyntax propertyDeclarationSyntax
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
                dbSetTypeSymbol = context.Compilation.DbSetType();
                dbContextTypeSymbol = context.Compilation.DbContextType();

                if (dbSetTypeSymbol is null || dbContextTypeSymbol is null)
                {
                    return;
                }
            }

            // Check that the property is actually a DbSet<T>, and that its containing type inherits from DbContext
            if (propertySymbol.Type.OriginalDefinition.Equals(dbSetTypeSymbol, SymbolEqualityComparer.Default)
                && InheritsFrom(propertySymbol.ContainingType, dbContextTypeSymbol))
            {
                context.ReportSuppression(Suppression.Create(SuppressUninitializedDbSetRule, diagnostic));
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
