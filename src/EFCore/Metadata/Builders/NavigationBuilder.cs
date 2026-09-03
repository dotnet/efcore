// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableNavigation" /> or <see cref="IMutableSkipNavigation" />.
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
public class NavigationBuilder : IInfrastructure<IConventionSkipNavigationBuilder?>, IInfrastructure<IConventionNavigationBuilder?>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public NavigationBuilder(IMutableNavigationBase navigationOrSkipNavigation)
    {
        Check.NotNull(navigationOrSkipNavigation, nameof(navigationOrSkipNavigation));

        InternalNavigationBuilder = (navigationOrSkipNavigation as Navigation)?.Builder;
        InternalSkipNavigationBuilder = (navigationOrSkipNavigation as SkipNavigation)?.Builder;
        Metadata = navigationOrSkipNavigation;

        Check.DebugAssert(
            InternalNavigationBuilder != null || InternalSkipNavigationBuilder != null,
            "Expected either a Navigation or SkipNavigation");
    }

    private InternalNavigationBuilder? InternalNavigationBuilder { get; set; }

    private InternalSkipNavigationBuilder? InternalSkipNavigationBuilder { get; }

    /// <summary>
    ///     The navigation being configured.
    /// </summary>
    public virtual IMutableNavigationBase Metadata { get; }

    /// <summary>
    ///     Adds or updates an annotation on the navigation property. If an annotation
    ///     with the key specified in <paramref name="annotation" /> already exists
    ///     its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual NavigationBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        if (InternalNavigationBuilder != null)
        {
            InternalNavigationBuilder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);
        }
        else
        {
            InternalSkipNavigationBuilder!.HasAnnotation(annotation, value, ConfigurationSource.Explicit);
        }

        return this;
    }

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
    public virtual NavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
    {
        if (InternalNavigationBuilder != null)
        {
            InternalNavigationBuilder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);
        }
        else
        {
            InternalSkipNavigationBuilder!.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);
        }

        return this;
    }

    /// <summary>
    ///     Sets a backing field to use for this navigation property.
    /// </summary>
    /// <param name="fieldName">The name of the field to use for this navigation property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual NavigationBuilder HasField(string? fieldName)
    {
        if (InternalNavigationBuilder != null)
        {
            InternalNavigationBuilder.HasField(fieldName, ConfigurationSource.Explicit);
        }
        else
        {
            InternalSkipNavigationBuilder!.HasField(fieldName, ConfigurationSource.Explicit);
        }

        return this;
    }

    /// <summary>
    ///     Configures whether this navigation should be automatically included in a query.
    /// </summary>
    /// <param name="autoInclude">A value indicating if the navigation should be automatically included.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual NavigationBuilder AutoInclude(bool autoInclude = true)
    {
        if (InternalNavigationBuilder != null)
        {
            InternalNavigationBuilder.AutoInclude(autoInclude, ConfigurationSource.Explicit);
        }
        else
        {
            InternalSkipNavigationBuilder!.AutoInclude(autoInclude, ConfigurationSource.Explicit);
        }

        return this;
    }

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
    public virtual NavigationBuilder EnableLazyLoading(bool lazyLoadingEnabled = true)
    {
        if (InternalNavigationBuilder != null)
        {
            InternalNavigationBuilder.EnableLazyLoading(lazyLoadingEnabled, ConfigurationSource.Explicit);
        }
        else
        {
            InternalSkipNavigationBuilder!.EnableLazyLoading(lazyLoadingEnabled, ConfigurationSource.Explicit);
        }

        return this;
    }

    /// <summary>
    ///     Configures whether this navigation is required.
    /// </summary>
    /// <param name="required">A value indicating whether the navigation should be required.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual NavigationBuilder IsRequired(bool required = true)
    {
        if (InternalNavigationBuilder != null)
        {
            InternalNavigationBuilder = InternalNavigationBuilder.IsRequired(required, ConfigurationSource.Explicit);
        }
        else
        {
            throw new InvalidOperationException(
                CoreStrings.RequiredSkipNavigation(
                    InternalSkipNavigationBuilder!.Metadata.DeclaringEntityType.DisplayName(),
                    InternalSkipNavigationBuilder.Metadata.Name));
        }

        return this;
    }

    /// <summary>
    ///     The internal builder being used to configure the skip navigation.
    /// </summary>
    IConventionSkipNavigationBuilder? IInfrastructure<IConventionSkipNavigationBuilder?>.Instance
        => InternalSkipNavigationBuilder;

    /// <summary>
    ///     The internal builder being used to configure the navigation.
    /// </summary>
    IConventionNavigationBuilder? IInfrastructure<IConventionNavigationBuilder?>.Instance
        => InternalNavigationBuilder;

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
