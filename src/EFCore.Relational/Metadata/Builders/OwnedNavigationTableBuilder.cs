// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
public class OwnedNavigationTableBuilder : IInfrastructure<OwnedNavigationBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationTableBuilder(in StoreObjectIdentifier? storeObject, OwnedNavigationBuilder ownedNavigationBuilder)
    {
        StoreObject = storeObject;
        OwnedNavigationBuilder = ownedNavigationBuilder;
    }

    /// <summary>
    ///     The specified table name.
    /// </summary>
    public virtual string? Name
        => StoreObject?.Name;

    /// <summary>
    ///     The specified table schema.
    /// </summary>
    public virtual string? Schema
        => StoreObject?.Schema;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual StoreObjectIdentifier? StoreObject { get; }

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
    public virtual OwnedNavigationTableBuilder ExcludeFromMigrations(bool excluded = true)
    {
        Metadata.SetIsTableExcludedFromMigrations(excluded);

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
        var trigger = EntityTypeBuilder.HasTrigger(Metadata, modelName).Metadata;
        if (Name != null)
        {
            trigger.SetTableName(Name);
            trigger.SetTableSchema(Schema);
        }

        return new TableTriggerBuilder(trigger);
    }

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <returns>A builder to configure the check constraint.</returns>
    public virtual CheckConstraintBuilder HasCheckConstraint(
        string name,
        string? sql)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NullButNotEmpty(sql, nameof(sql));

        var checkConstraint = InternalCheckConstraintBuilder.HasCheckConstraint(
            (IConventionEntityType)Metadata,
            name,
            sql,
            ConfigurationSource.Explicit)!;

        return new CheckConstraintBuilder((IMutableCheckConstraint)checkConstraint);
    }

    /// <summary>
    ///     Configures a comment to be applied to the table
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="comment">The comment for the table.</param>
    /// <returns>A builder to further configure the table.</returns>
    public virtual OwnedNavigationTableBuilder HasComment(string? comment)
    {
        Metadata.SetComment(comment);
        return this;
    }

    /// <summary>
    ///     Maps the property to a column on the current table and returns an object that can be used
    ///     to provide table-specific configuration if the property is mapped to more than one table.
    /// </summary>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ColumnBuilder Property(string propertyName)
        => new(GetStoreObjectIdentifier(), OwnedNavigationBuilder.Property(propertyName));

    /// <summary>
    ///     Maps the property to a column on the current table and returns an object that can be used
    ///     to provide table-specific configuration if the property is mapped to more than one table.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ColumnBuilder<TProperty> Property<TProperty>(string propertyName)
        => new(GetStoreObjectIdentifier(), OwnedNavigationBuilder.Property<TProperty>(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual StoreObjectIdentifier GetStoreObjectIdentifier()
        => StoreObject ?? throw new InvalidOperationException(RelationalStrings.MappingFragmentMissingName);

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
