// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
public class TableBuilder<TEntity> : TableBuilder, IInfrastructure<EntityTypeBuilder<TEntity>>
    where TEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public TableBuilder(in StoreObjectIdentifier? storeObject, EntityTypeBuilder<TEntity> entityTypeBuilder)
        : base(storeObject, entityTypeBuilder)
    {
    }

    private EntityTypeBuilder<TEntity> EntityTypeBuilder
        => (EntityTypeBuilder<TEntity>)((IInfrastructure<EntityTypeBuilder>)this).Instance;

    /// <summary>
    ///     Configures a comment to be applied to the table
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="comment">The comment for the table.</param>
    /// <returns>A builder to further configure the table.</returns>
    public new virtual TableBuilder<TEntity> HasComment(string? comment)
        => (TableBuilder<TEntity>)base.HasComment(comment);

    /// <summary>
    ///     Configures the table to be ignored by migrations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="excluded">A value indicating whether the table should be managed by migrations.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual TableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
        => (TableBuilder<TEntity>)base.ExcludeFromMigrations(excluded);

    /// <summary>
    ///     Maps the property to a column on the current table and returns an object that can be used
    ///     to provide table-specific configuration if the property is mapped to more than one table.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        => new(GetStoreObjectIdentifier(), EntityTypeBuilder.Property(propertyExpression));

    EntityTypeBuilder<TEntity> IInfrastructure<EntityTypeBuilder<TEntity>>.Instance
        => EntityTypeBuilder;
}
