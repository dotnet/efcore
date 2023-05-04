// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
public class OwnedNavigationSplitViewBuilder : IInfrastructure<OwnedNavigationBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationSplitViewBuilder(in StoreObjectIdentifier storeObject, OwnedNavigationBuilder ownedNavigationBuilder)
    {
        Check.DebugAssert(
            storeObject.StoreObjectType == StoreObjectType.View,
            "StoreObjectType should be View, not " + storeObject.StoreObjectType);

        MappingFragment = EntityTypeMappingFragment.GetOrCreate(
            ownedNavigationBuilder.OwnedEntityType, storeObject, ConfigurationSource.Explicit);
        OwnedNavigationBuilder = ownedNavigationBuilder;
    }

    /// <summary>
    ///     The specified view name.
    /// </summary>
    public virtual string Name
        => MappingFragment.StoreObject.Name;

    /// <summary>
    ///     The specified view schema.
    /// </summary>
    public virtual string? Schema
        => MappingFragment.StoreObject.Schema;

    /// <summary>
    ///     The mapping fragment being configured.
    /// </summary>
    public virtual IMutableEntityTypeMappingFragment MappingFragment { get; }

    private OwnedNavigationBuilder OwnedNavigationBuilder { get; }

    /// <summary>
    ///     Maps the property to a column on the current view and returns an object that can be used
    ///     to provide view-specific configuration if the property is mapped to more than one view.
    /// </summary>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ViewColumnBuilder Property(string propertyName)
        => new(MappingFragment.StoreObject, OwnedNavigationBuilder.Property(propertyName));

    /// <summary>
    ///     Maps the property to a column on the current view and returns an object that can be used
    ///     to provide view-specific configuration if the property is mapped to more than one view.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ViewColumnBuilder<TProperty> Property<TProperty>(string propertyName)
        => new(MappingFragment.StoreObject, OwnedNavigationBuilder.Property<TProperty>(propertyName));

    /// <summary>
    ///     Adds or updates an annotation on the view. If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationSplitViewBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        ((EntityTypeMappingFragment)MappingFragment).Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

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
