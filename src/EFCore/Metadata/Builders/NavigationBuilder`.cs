// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableNavigation" /> or a <see cref="IMutableSkipNavigation" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class NavigationBuilder<TSource, TTarget> : NavigationBuilder
    where TSource : class
    where TTarget : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public NavigationBuilder(IMutableNavigationBase navigationOrSkipNavigation)
        : base(navigationOrSkipNavigation)
    {
    }

    /// <summary>
    ///     Adds or updates an annotation on the navigation property. If an annotation
    ///     with the key specified in <paramref name="annotation" /> already exists
    ///     its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual NavigationBuilder<TSource, TTarget> HasAnnotation(string annotation, object? value)
        => (NavigationBuilder<TSource, TTarget>)base.HasAnnotation(annotation, value);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, the backing field, if one is found by convention or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses.  Calling this method will change that behavior
    ///         for this property as described in the <see cref="PropertyAccessMode" /> enum.
    ///     </para>
    ///     <para>
    ///         Calling this method overrides for this property any access mode that was set on the
    ///         entity type or model.
    ///     </para>
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual NavigationBuilder<TSource, TTarget> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        => (NavigationBuilder<TSource, TTarget>)base.UsePropertyAccessMode(propertyAccessMode);

    /// <summary>
    ///     Sets a backing field to use for this navigation property.
    /// </summary>
    /// <param name="fieldName">The name of the field to use for this navigation property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual NavigationBuilder<TSource, TTarget> HasField(string? fieldName)
        => (NavigationBuilder<TSource, TTarget>)base.HasField(fieldName);

    /// <summary>
    ///     Configures whether this navigation should be automatically included in a query.
    /// </summary>
    /// <param name="autoInclude">A value indicating if the navigation should be automatically included.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual NavigationBuilder<TSource, TTarget> AutoInclude(bool autoInclude = true)
        => (NavigationBuilder<TSource, TTarget>)base.AutoInclude(autoInclude);

    /// <summary>
    ///     Configures whether this navigation should be enabled for lazy-loading. Note that a property can only be lazy-loaded
    ///     if a lazy-loading mechanism such as lazy-loading proxies or <see cref="ILazyLoader" /> injection has been configured.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="lazyLoadingEnabled">A value indicating if the navigation should be enabled for lazy-loading.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual NavigationBuilder<TSource, TTarget> EnableLazyLoading(bool lazyLoadingEnabled = true)
        => (NavigationBuilder<TSource, TTarget>)base.EnableLazyLoading(lazyLoadingEnabled);

    /// <summary>
    ///     Configures whether this navigation is required.
    /// </summary>
    /// <param name="required">A value indicating whether the navigation should be required.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual NavigationBuilder<TSource, TTarget> IsRequired(bool required = true)
        => (NavigationBuilder<TSource, TTarget>)base.IsRequired(required);

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
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
