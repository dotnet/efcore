// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
/// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
public class OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> :
    OwnedNavigationSplitTableBuilder,
    IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>>
    where TOwnerEntity : class
    where TDependentEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationSplitTableBuilder(
        in StoreObjectIdentifier storeObject,
        OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder)
        : base(storeObject, ownedNavigationBuilder)
    {
    }

    private OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> OwnedNavigationBuilder
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)((IInfrastructure<OwnedNavigationBuilder>)this)
            .GetInfrastructure();

    /// <summary>
    ///     Configures the table to be ignored by migrations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information.
    /// </remarks>
    /// <param name="excluded">A value indicating whether the table should be managed by migrations.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> ExcludeFromMigrations(bool excluded = true)
        => (OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>)base.ExcludeFromMigrations(excluded);

    /// <summary>
    ///     Maps the property to a column on the current table and returns an object that can be used
    ///     to provide table-specific configuration if the property is mapped to more than one table.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
        => new(MappingFragment.StoreObject, OwnedNavigationBuilder.Property(propertyExpression));

    /// <summary>
    ///     Adds or updates an annotation on the table. If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(
        string annotation,
        object? value)
        => (OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>)base.HasAnnotation(annotation, value);

    OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>>.Instance
        => OwnedNavigationBuilder;
}
