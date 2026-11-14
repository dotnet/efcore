// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A relational-specific convention inheriting from <see cref="ForeignKeyIndexConvention" /> that takes index filters
///     into account.
/// </summary>
/// <remarks>
///     <para>
///         An index with a filter only includes some of the rows in the table, so it cannot be used in place of the
///         index on the foreign key properties that would otherwise be created by convention. The exception is a filter that
///         was set by convention, such as the filter that SQL Server uses to exclude rows with null values from unique indexes.
///         These filters are not considered when deciding whether an index is needed. Strictly, such a filter can also exclude rows
///         that a lookup on a prefix of the indexed columns needs, but treating it as covering keeps the behavior that models
///         without explicit filters have always had, and so avoids adding indexes to existing models.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public class RelationalForeignKeyIndexConvention : ForeignKeyIndexConvention, IIndexAnnotationChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalForeignKeyIndexConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalForeignKeyIndexConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies)
        => RelationalDependencies = relationalDependencies;

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Called after an annotation is changed on an index.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessIndexAnnotationChanged(
        IConventionIndexBuilder indexBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == RelationalAnnotationNames.Filter)
        {
            ProcessIndexCoverageChanged(indexBuilder.Metadata);
        }
    }

    /// <summary>
    ///     Called when a model is being finalized.
    /// </summary>
    /// <remarks>
    ///     Upgrading a filter from being set by convention to being set explicitly, without changing its value, is not reported
    ///     as an annotation change. This makes sure that foreign key indexes are created for any index that is partial at this point.
    /// </remarks>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var index in entityType.GetDeclaredIndexes().ToList())
            {
                if (index.IsInModel
                    && IsPartial(index))
                {
                    ProcessIndexCoverageChanged(index);
                }
            }
        }

        base.ProcessModelFinalizing(modelBuilder, context);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     An index is partial if it has a filter that was not set by convention. Filters set by convention, such as the filter
    ///     used by SQL Server to exclude rows with null values from unique indexes, are ignored. The filter is read from the index
    ///     itself, so a filter that only applies to a specific table because of table sharing is not taken into account.
    /// </remarks>
    protected override bool IsPartial(IConventionIndex index)
        => index.FindAnnotation(RelationalAnnotationNames.Filter) is IConventionAnnotation { Value: not null } filter
            && filter.GetConfigurationSource() != ConfigurationSource.Convention;
}
