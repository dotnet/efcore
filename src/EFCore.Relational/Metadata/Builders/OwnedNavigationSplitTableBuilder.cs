// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
public class OwnedNavigationSplitTableBuilder : IInfrastructure<OwnedNavigationBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationSplitTableBuilder(in StoreObjectIdentifier storeObject, OwnedNavigationBuilder ownedNavigationBuilder)
    {
        Check.DebugAssert(
            storeObject.StoreObjectType == StoreObjectType.Table,
            "StoreObjectType should be Table, not " + storeObject.StoreObjectType);

        InternalMappingFragment = EntityTypeMappingFragment.GetOrCreate(
            ownedNavigationBuilder.OwnedEntityType, storeObject, ConfigurationSource.Explicit);
        OwnedNavigationBuilder = ownedNavigationBuilder;
    }

    /// <summary>
    ///     The specified table name.
    /// </summary>
    public virtual string Name
        => MappingFragment.StoreObject.Name;

    /// <summary>
    ///     The specified table schema.
    /// </summary>
    public virtual string? Schema
        => MappingFragment.StoreObject.Schema;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityTypeMappingFragment InternalMappingFragment { get; }

    /// <summary>
    ///     The mapping fragment being configured.
    /// </summary>
    public virtual IMutableEntityTypeMappingFragment MappingFragment
        => InternalMappingFragment;

    /// <summary>
    ///     The entity type being configured.
    /// </summary>
    public virtual IMutableEntityType Metadata
        => OwnedNavigationBuilder.OwnedEntityType;

    private OwnedNavigationBuilder OwnedNavigationBuilder { get; }

    /// <summary>
    ///     Configures the table to be ignored by migrations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information.
    /// </remarks>
    /// <param name="excluded">A value indicating whether the table should be managed by migrations.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public virtual OwnedNavigationSplitTableBuilder ExcludeFromMigrations(bool excluded = true)
    {
        MappingFragment.IsTableExcludedFromMigrations = excluded;

        return this;
    }

    /// <summary>
    ///     Configures a database trigger on the table.
    /// </summary>
    /// <param name="modelName">The name of the trigger.</param>
    /// <returns>A builder that can be used to configure the database trigger.</returns>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
    /// </remarks>
    public virtual TableTriggerBuilder HasTrigger(string modelName)
    {
        var trigger = EntityTypeBuilder.HasTrigger(OwnedNavigationBuilder.OwnedEntityType, modelName).Metadata;
        trigger.SetTableName(Name);
        trigger.SetTableSchema(Schema);

        return new TableTriggerBuilder(trigger);
    }

    /// <summary>
    ///     Maps the property to a column on the current table and returns an object that can be used
    ///     to provide table-specific configuration if the property is mapped to more than one table.
    /// </summary>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ColumnBuilder Property(string propertyName)
        => new(MappingFragment.StoreObject, OwnedNavigationBuilder.Property(propertyName));

    /// <summary>
    ///     Maps the property to a column on the current table and returns an object that can be used
    ///     to provide table-specific configuration if the property is mapped to more than one table.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ColumnBuilder<TProperty> Property<TProperty>(string propertyName)
        => new(MappingFragment.StoreObject, OwnedNavigationBuilder.Property<TProperty>(propertyName));

    /// <summary>
    ///     Adds or updates an annotation on the table. If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationSplitTableBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        InternalMappingFragment.Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    OwnedNavigationBuilder IInfrastructure<OwnedNavigationBuilder>.Instance
        => OwnedNavigationBuilder;

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
