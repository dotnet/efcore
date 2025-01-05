// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
///     part of the primary key and not part of any foreign keys, were configured to have a database default value
///     or were configured to use a <see cref="SqlServerValueGenerationStrategy" />.
///     It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate" /> if they were configured as computed columns.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqlServerValueGenerationConvention : RelationalValueGenerationConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerValueGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqlServerValueGenerationConvention(
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
        if (name == SqlServerAnnotationNames.ValueGenerationStrategy)
        {
            propertyBuilder.ValueGenerated(GetValueGenerated(propertyBuilder.Metadata));
            return;
        }

        base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
    }

    /// <summary>
    ///     Called after an annotation is changed on an entity.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name is SqlServerAnnotationNames.TemporalPeriodStartPropertyName or SqlServerAnnotationNames.TemporalPeriodEndPropertyName
            && annotation?.Value is string propertyName)
        {
            var periodProperty = entityTypeBuilder.Metadata.FindProperty(propertyName);
            periodProperty?.Builder.ValueGenerated(GetValueGenerated(periodProperty));

            // cleanup the previous period property - its possible that it won't be deleted
            // (e.g. when removing period with default name, while the property with that same name has been explicitly defined)
            if (oldAnnotation?.Value is string oldPropertyName)
            {
                var oldPeriodProperty = entityTypeBuilder.Metadata.FindProperty(oldPropertyName);
                oldPeriodProperty?.Builder.ValueGenerated(GetValueGenerated(oldPeriodProperty));
            }
        }

        base.ProcessEntityTypeAnnotationChanged(entityTypeBuilder, name, annotation, oldAnnotation, context);
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        // TODO: move to relational?
        if (property.DeclaringType.IsMappedToJson()
#pragma warning disable EF1001 // Internal EF Core API usage.
            && property.IsOrdinalKeyProperty()
#pragma warning restore EF1001 // Internal EF Core API usage.
            && (property.DeclaringType as IReadOnlyEntityType)?.FindOwnership()!.IsUnique == false)
        {
            return ValueGenerated.OnAdd;
        }

        var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
        if (declaringTable.Name == null)
        {
            return null;
        }

        // If the first mapping can be value generated then we'll consider all mappings to be value generated
        // as this is a client-side configuration and can't be specified per-table.
        return GetValueGenerated(property, declaringTable, Dependencies.TypeMappingSource);
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    public static new ValueGenerated? GetValueGenerated(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
            ?? (property.GetValueGenerationStrategy(storeObject) != SqlServerValueGenerationStrategy.None
                ? ValueGenerated.OnAdd
                : null);

    private static ValueGenerated? GetValueGenerated(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource typeMappingSource)
        => GetTemporalValueGenerated(property)
            ?? RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
            ?? (property.GetValueGenerationStrategy(storeObject, typeMappingSource) != SqlServerValueGenerationStrategy.None
                ? ValueGenerated.OnAdd
                : null);

    private static ValueGenerated? GetTemporalValueGenerated(IReadOnlyProperty property)
    {
        var entityType = property.DeclaringType as IReadOnlyEntityType;
        return entityType != null
            && entityType.IsTemporal()
            && (entityType.GetPeriodStartPropertyName() == property.Name
                || entityType.GetPeriodEndPropertyName() == property.Name)
                ? ValueGenerated.OnAddOrUpdate
                : null;
    }
}
