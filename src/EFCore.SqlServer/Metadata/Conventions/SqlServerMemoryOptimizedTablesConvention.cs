// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures indexes as non-clustered for memory-optimized tables.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqlServerMemoryOptimizedTablesConvention :
    IEntityTypeAnnotationChangedConvention,
    IKeyAddedConvention,
    IIndexAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerMemoryOptimizedTablesConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqlServerMemoryOptimizedTablesConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Called after an annotation is changed on an entity type.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == SqlServerAnnotationNames.MemoryOptimized)
        {
            var memoryOptimized = annotation?.Value as bool? == true;
            foreach (var key in entityTypeBuilder.Metadata.GetDeclaredKeys())
            {
                key.Builder.IsClustered(memoryOptimized ? false : null);
            }

            foreach (var index in
                     entityTypeBuilder.Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredIndexes()))
            {
                index.Builder.IsClustered(memoryOptimized ? false : null);
            }
        }
    }

    /// <summary>
    ///     Called after a key is added to the entity type.
    /// </summary>
    /// <param name="keyBuilder">The builder for the key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
    {
        if (keyBuilder.Metadata.DeclaringEntityType.IsMemoryOptimized())
        {
            keyBuilder.IsClustered(false);
        }
    }

    /// <summary>
    ///     Called after an index is added to the entity type.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessIndexAdded(IConventionIndexBuilder indexBuilder, IConventionContext<IConventionIndexBuilder> context)
    {
        if (indexBuilder.Metadata.DeclaringEntityType.GetAllBaseTypesInclusive().Any(et => et.IsMemoryOptimized()))
        {
            indexBuilder.IsClustered(false);
        }
    }
}
