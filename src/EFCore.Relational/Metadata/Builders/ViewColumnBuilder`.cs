// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
public class ViewColumnBuilder<TProperty> : ViewColumnBuilder, IInfrastructure<PropertyBuilder<TProperty>>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ViewColumnBuilder(in StoreObjectIdentifier storeObject, PropertyBuilder<TProperty> propertyBuilder)
        : base(storeObject, propertyBuilder)
    {
    }

    private PropertyBuilder<TProperty> PropertyBuilder
        => (PropertyBuilder<TProperty>)((IInfrastructure<PropertyBuilder>)this).Instance;

    /// <summary>
    ///     Configures the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual ViewColumnBuilder<TProperty> HasColumnName(string? name)
        => (ViewColumnBuilder<TProperty>)base.HasColumnName(name);

    /// <summary>
    ///     Adds or updates an annotation on the property for a specific view.
    ///     If an annotation with the key specified in <paramref name="annotation" />
    ///     already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ViewColumnBuilder<TProperty> HasAnnotation(string annotation, object? value)
        => (ViewColumnBuilder<TProperty>)base.HasAnnotation(annotation, value);

    PropertyBuilder<TProperty> IInfrastructure<PropertyBuilder<TProperty>>.Instance
        => PropertyBuilder;
}
