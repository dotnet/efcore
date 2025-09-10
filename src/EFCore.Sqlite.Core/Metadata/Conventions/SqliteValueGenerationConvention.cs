// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
///     part of the primary key and not part of any foreign keys, were configured to have a database default value
///     or were configured to use a <see cref="SqliteValueGenerationStrategy" />.
///     It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate" /> if they were configured as computed columns.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqliteValueGenerationConvention : RelationalValueGenerationConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqliteValueGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqliteValueGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    ///     Called after an annotation is changed on a property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == SqliteAnnotationNames.ValueGenerationStrategy)
        {
            propertyBuilder.ValueGenerated(GetValueGenerated(propertyBuilder.Metadata));
            return;
        }

        base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
        return declaringTable.Name == null
            ? null
            : GetValueGenerated(property, declaringTable, Dependencies.TypeMappingSource);
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    public static new ValueGenerated? GetValueGenerated(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
            ?? (property.GetValueGenerationStrategy(storeObject) != SqliteValueGenerationStrategy.None
                ? ValueGenerated.OnAdd
                : null);

    private static ValueGenerated? GetValueGenerated(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource typeMappingSource)
        => RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
            ?? (property.GetValueGenerationStrategy(storeObject, typeMappingSource) != SqliteValueGenerationStrategy.None
                ? ValueGenerated.OnAdd
                : null);
}