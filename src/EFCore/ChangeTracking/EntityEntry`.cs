// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking information and operations for a given entity.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
///         not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
/// <typeparam name="TEntity">The type of entity being tracked by this entry.</typeparam>
public class EntityEntry<TEntity> : EntityEntry
    where TEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public EntityEntry(InternalEntityEntry internalEntry)
        : base(internalEntry)
    {
    }

    /// <summary>
    ///     Gets the entity being tracked by this entry.
    /// </summary>
    public new virtual TEntity Entity
        => (TEntity)base.Entity;

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of this entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to access information and operations for.
    /// </param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        Check.NotNull(propertyExpression, nameof(propertyExpression));

        return new PropertyEntry<TEntity, TProperty>(
            InternalEntry,
            Metadata.GetProperty(propertyExpression.GetMemberAccess().GetSimpleMemberName()));
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given complex type property of this entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to access information and operations for.
    /// </param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexPropertyEntry<TEntity, TProperty> ComplexProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        Check.NotNull(propertyExpression, nameof(propertyExpression));

        return new ComplexPropertyEntry<TEntity, TProperty>(
            InternalEntry,
            Metadata.GetComplexProperty(propertyExpression.GetMemberAccess().GetSimpleMemberName()));
    }

    /// <summary>
    ///     Provides access to change tracking and loading information for a reference (i.e. non-collection)
    ///     navigation property that associates this entity to another entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the reference navigation to access information and operations for.
    /// </param>
    /// <returns>
    ///     An object that exposes change tracking information and operations for the given navigation property.
    /// </returns>
    public virtual ReferenceEntry<TEntity, TProperty> Reference<TProperty>(
        Expression<Func<TEntity, TProperty?>> propertyExpression)
        where TProperty : class
    {
        Check.NotNull(propertyExpression, nameof(propertyExpression));

        return new ReferenceEntry<TEntity, TProperty>(InternalEntry, propertyExpression.GetMemberAccess().GetSimpleMemberName());
    }

    /// <summary>
    ///     Provides access to change tracking and loading information for a collection
    ///     navigation property that associates this entity to a collection of another entities.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the collection navigation to access information and operations for.
    /// </param>
    /// <returns>
    ///     An object that exposes change tracking information and operations for the given navigation property.
    /// </returns>
    public virtual CollectionEntry<TEntity, TProperty> Collection<TProperty>(
        Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression)
        where TProperty : class
    {
        Check.NotNull(propertyExpression, nameof(propertyExpression));

        return new CollectionEntry<TEntity, TProperty>(InternalEntry, propertyExpression.GetMemberAccess().GetSimpleMemberName());
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of this entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="property">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(IProperty property)
    {
        Check.NotNull(property, nameof(property));

        ValidateType<TProperty>(property);

        return new PropertyEntry<TEntity, TProperty>(InternalEntry, property);
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given complex type property of this entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="complexProperty">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexPropertyEntry<TEntity, TProperty> ComplexProperty<TProperty>(IComplexProperty complexProperty)
    {
        Check.NotNull(complexProperty, nameof(complexProperty));

        ValidateType<TProperty>(complexProperty);

        return new ComplexPropertyEntry<TEntity, TProperty>(InternalEntry, complexProperty);
    }

    /// <summary>
    ///     Provides access to change tracking and loading information for a reference (i.e. non-collection)
    ///     navigation that associates this entity to another entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="navigation">The reference navigation.</param>
    /// <returns>
    ///     An object that exposes change tracking information and operations for the given navigation property.
    /// </returns>
    public virtual ReferenceEntry<TEntity, TProperty> Reference<TProperty>(INavigationBase navigation)
        where TProperty : class
    {
        Check.NotNull(navigation, nameof(navigation));

        return new ReferenceEntry<TEntity, TProperty>(InternalEntry, (INavigation)navigation);
    }

    /// <summary>
    ///     Provides access to change tracking and loading information for a collection
    ///     navigation property that associates this entity to a collection of another entities.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="navigation">The collection navigation.</param>
    /// <returns>
    ///     An object that exposes change tracking information and operations for the given navigation property.
    /// </returns>
    public virtual CollectionEntry<TEntity, TProperty> Collection<TProperty>(INavigationBase navigation)
        where TProperty : class
    {
        Check.NotNull(navigation, nameof(navigation));

        return new CollectionEntry<TEntity, TProperty>(InternalEntry, navigation);
    }

    /// <summary>
    ///     Provides access to change tracking and loading information for a reference (i.e. non-collection)
    ///     navigation that associates this entity to another entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyName">The name of the navigation property.</param>
    /// <returns>
    ///     An object that exposes change tracking information and operations for the given navigation property.
    /// </returns>
    public virtual ReferenceEntry<TEntity, TProperty> Reference<TProperty>(string propertyName)
        where TProperty : class
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        return new ReferenceEntry<TEntity, TProperty>(InternalEntry, propertyName);
    }

    /// <summary>
    ///     Provides access to change tracking and loading information for a collection
    ///     navigation property that associates this entity to a collection of another entities.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyName">The name of the navigation property.</param>
    /// <returns>
    ///     An object that exposes change tracking information and operations for the given navigation property.
    /// </returns>
    public virtual CollectionEntry<TEntity, TProperty> Collection<TProperty>(string propertyName)
        where TProperty : class
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        return new CollectionEntry<TEntity, TProperty>(InternalEntry, propertyName);
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of this entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyName">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        ValidateType<TProperty>(InternalEntry.EntityType.FindProperty(propertyName));

        return new PropertyEntry<TEntity, TProperty>(InternalEntry, Metadata.GetProperty(propertyName));
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given complex type property of this entity.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyName">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexPropertyEntry<TEntity, TProperty> ComplexProperty<TProperty>(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        ValidateType<TProperty>(InternalEntry.EntityType.FindComplexProperty(propertyName));

        return new ComplexPropertyEntry<TEntity, TProperty>(InternalEntry, Metadata.GetComplexProperty(propertyName));
    }

    private static void ValidateType<TProperty>(IPropertyBase? property)
    {
        if (property != null
            && property.ClrType != typeof(TProperty))
        {
            throw new ArgumentException(
                CoreStrings.WrongGenericPropertyType(
                    property.Name,
                    property.DeclaringType.DisplayName(),
                    property.ClrType.ShortDisplayName(),
                    typeof(TProperty).ShortDisplayName()));
        }
    }
}
