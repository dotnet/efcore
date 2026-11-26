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
///         index on the foreign key properties that would otherwise be created by convention. There are two exceptions:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 An index on exactly the foreign key properties is always the foreign key's index, whatever its filter. The filter
///                 can't be interpreted, and the model can't hold a second unnamed index on the same properties, so a separate index
///                 would be created that has the same columns as the one the user configured.
///             </description>
///         </item>
///         <item>
///             <description>
///                 A filter that was set by convention, such as the one SQL Server adds to unique indexes over nullable columns to
///                 exclude rows with null values, is treated as excluding the rows where any nullable indexed column is null. It
///                 doesn't prevent the index from covering a foreign key if every one of those columns is a foreign key property,
///                 since a lookup on the foreign key's values never needs the rows with nulls in them. Otherwise, it's partial.
///             </description>
///         </item>
///     </list>
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
            OnIndexCoverageChanged(indexBuilder.Metadata);
        }
    }

    /// <summary>
    ///     Called when a model is being finalized.
    /// </summary>
    /// <remarks>
    ///     Upgrading a filter from being set by convention to being set explicitly, without changing its value, is not reported
    ///     as an annotation change. This makes sure that foreign key indexes are created for any index with an explicit filter.
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
                    && index.FindAnnotation(RelationalAnnotationNames.Filter) is IConventionAnnotation { Value: not null } filter
                    && filter.GetConfigurationSource() != ConfigurationSource.Convention)
                {
                    OnIndexCoverageChanged(index);
                }
            }
        }

        base.ProcessModelFinalizing(modelBuilder, context);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     An index with a filter is partial, unless it is on exactly the given properties, or the filter was set by convention
    ///     and only excludes rows with nulls in the given properties. The filter is read from the index itself, so a filter that
    ///     only applies to a specific table because of table sharing is not taken into account.
    /// </remarks>
    protected override bool IsPartial(IConventionIndex index, IReadOnlyList<IConventionPropertyBase> properties)
    {
        if (index.FindAnnotation(RelationalAnnotationNames.Filter) is not IConventionAnnotation { Value: not null } filter
            || index.Properties.Count == properties.Count)
        {
            return false;
        }

        return filter.GetConfigurationSource() != ConfigurationSource.Convention
            || index.Properties.Any(p => p is IConventionProperty { IsNullable: true } && !properties.Contains(p));
    }
}
