// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
///     part of the primary key and not part of any foreign keys or were configured to have a database default value.
///     It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate" /> if they were configured as computed columns.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
public class RelationalValueGenerationConvention :
    ValueGenerationConvention,
    IPropertyAnnotationChangedConvention,
    IEntityTypeAnnotationChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalValueGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalValueGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        var property = propertyBuilder.Metadata;
        switch (name)
        {
            case RelationalAnnotationNames.DefaultValue:
#pragma warning disable EF1001 // Internal EF Core API usage.
                if ((((IProperty)property).TryGetMemberInfo(forMaterialization: false, forSet: false, out var member, out _)
                        ? member!.GetMemberType()
                        : property.ClrType)
#pragma warning restore EF1001 // Internal EF Core API usage.
                    == typeof(bool)
                    && Equals(true, property.GetDefaultValue()))
                {
                    propertyBuilder.HasSentinel(annotation != null ? true : null);
                }

                goto case RelationalAnnotationNames.DefaultValueSql;
            case RelationalAnnotationNames.DefaultValueSql:
            case RelationalAnnotationNames.ComputedColumnSql:
                propertyBuilder.ValueGenerated(GetValueGenerated(property));
                break;
        }
    }

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
        var entityType = entityTypeBuilder.Metadata;
        switch (name)
        {
            case RelationalAnnotationNames.ViewName:
            case RelationalAnnotationNames.FunctionName:
            case RelationalAnnotationNames.SqlQuery:
            case RelationalAnnotationNames.InsertStoredProcedure:
                if (annotation?.Value != null
                    && oldAnnotation?.Value == null
                    && entityType.GetTableName() == null)
                {
                    ProcessTableChanged(
                        entityTypeBuilder,
                        entityType.GetDefaultTableName(),
                        entityType.GetDefaultSchema(),
                        null,
                        null);
                }

                break;

            case RelationalAnnotationNames.TableName:
                var schema = entityType.GetSchema();
                ProcessTableChanged(
                    entityTypeBuilder,
                    (string?)oldAnnotation?.Value ?? entityType.GetDefaultTableName(),
                    schema,
                    entityType.GetTableName(),
                    schema);
                break;

            case RelationalAnnotationNames.Schema:
                var tableName = entityType.GetTableName();
                ProcessTableChanged(
                    entityTypeBuilder,
                    tableName,
                    (string?)oldAnnotation?.Value ?? entityType.GetDefaultSchema(),
                    tableName,
                    entityTypeBuilder.Metadata.GetSchema());
                break;

            case RelationalAnnotationNames.MappingStrategy:
                var primaryKey = entityTypeBuilder.Metadata.FindPrimaryKey();
                if (primaryKey == null)
                {
                    return;
                }

                foreach (var property in primaryKey.Properties)
                {
                    property.Builder.ValueGenerated(GetValueGenerated(property));
                }

                break;
        }
    }

    private void ProcessTableChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string? oldTable,
        string? oldSchema,
        string? newTable,
        string? newSchema)
    {
        if (newTable == null || oldTable == null)
        {
            foreach (var property in entityTypeBuilder.Metadata.GetDeclaredProperties())
            {
                property.Builder.ValueGenerated(GetValueGenerated(property));
            }

            return;
        }

        var primaryKey = entityTypeBuilder.Metadata.FindPrimaryKey();
        if (primaryKey == null)
        {
            return;
        }

        var oldLink = entityTypeBuilder.Metadata.FindRowInternalForeignKeys(StoreObjectIdentifier.Table(oldTable, oldSchema));
        var newLink = entityTypeBuilder.Metadata.FindRowInternalForeignKeys(StoreObjectIdentifier.Table(newTable, newSchema));

        if (!oldLink.Any()
            && !newLink.Any())
        {
            return;
        }

        foreach (var property in primaryKey.Properties)
        {
            property.Builder.ValueGenerated(GetValueGenerated(property));
        }
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        var table = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
        return !MappingStrategyAllowsValueGeneration(property, property.DeclaringType.GetMappingStrategy())
            ? null
            : table.Name != null
                ? GetValueGenerated(property, table)
                : property.DeclaringType.IsMappedToJson()
                && property.IsOrdinalKeyProperty()
                && (property.DeclaringType as IReadOnlyEntityType)?.FindOwnership()!.IsUnique == false
                    ? ValueGenerated.OnAddOrUpdate
                    : property.GetMappedStoreObjects(StoreObjectType.InsertStoredProcedure).Any()
                        ? GetValueGenerated((IReadOnlyProperty)property)
                        : null;
    }

    /// <summary>
    ///     Checks whether or not the mapping strategy and property allow value generation by convention.
    /// </summary>
    /// <param name="property">The property for which value generation is being considered.</param>
    /// <param name="mappingStrategy">The current mapping strategy.</param>
    /// <returns><see langword="true" /> if value generation is allowed; <see langword="false" /> otherwise.</returns>
    protected virtual bool MappingStrategyAllowsValueGeneration(
        IConventionProperty property,
        string? mappingStrategy)
    {
        if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy)
        {
            var propertyType = property.ClrType.UnwrapNullableType();
            if (property.IsPrimaryKey()
                && propertyType.IsInteger()
                && propertyType != typeof(byte))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The new store value generation strategy to set for the given property.</returns>
    public static ValueGenerated? GetValueGenerated(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
    {
        var valueGenerated = GetValueGenerated(property);
        return valueGenerated
            ?? (property.GetComputedColumnSql(storeObject) != null
                ? ValueGenerated.OnAddOrUpdate
                : property.TryGetDefaultValue(storeObject, out _) || property.GetDefaultValueSql(storeObject) != null
                    ? ValueGenerated.OnAdd
                    : null);
    }
}
