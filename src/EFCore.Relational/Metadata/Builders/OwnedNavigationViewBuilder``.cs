// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
/// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
public class OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity> :
    OwnedNavigationViewBuilder,
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
    public OwnedNavigationViewBuilder(
        in StoreObjectIdentifier storeObject,
        OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder)
        : base(storeObject, ownedNavigationBuilder)
    {
    }

    private OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> OwnedNavigationBuilder
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)((IInfrastructure<OwnedNavigationBuilder>)this)
            .GetInfrastructure();

    /// <summary>
    ///     Maps the property to a column on the current view and returns an object that can be used
    ///     to provide view-specific configuration if the property is mapped to more than one view.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ViewColumnBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
        => new(StoreObject, OwnedNavigationBuilder.Property(propertyExpression));

    OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>>.Instance
        => OwnedNavigationBuilder;
}
