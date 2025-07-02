// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking information and operations for a given complex type instance.
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
public class ComplexEntry : IInfrastructure<InternalComplexEntry>
{
    private static readonly int MaxEntityState = Enum.GetValuesAsUnderlyingType<EntityState>().Cast<int>().Max();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexEntry(InternalComplexEntry internalEntry)
    {
        InternalEntry = internalEntry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalComplexEntry InternalEntry { get; }

    /// <summary>
    ///     Gets the metadata that describes the facets of this property and how it maps to the database.
    /// </summary>
    public virtual IComplexProperty Metadata => InternalEntry.ComplexProperty;

    /// <summary>
    ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
    ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
    ///     for the context to detect the change.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public virtual object? CurrentValue
    {
        get
        {
            var list = (IList?)InternalEntry.ContainingEntry[Metadata];
            return list == null
                ? throw new InvalidOperationException(
                    CoreStrings.ComplexCollectionNotInitialized(Metadata.DeclaringType.ShortNameChain(), Metadata.Name))
                : list[InternalEntry.Ordinal];
        }
    }

    /// <summary>
    ///     The <see cref="EntityEntry" /> to which this member belongs.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <value> An entry for the entity that owns this member. </value>
    public virtual EntityEntry EntityEntry
        => new(InternalEntry.EntityEntry);

    /// <summary>
    ///     Gets or sets that state that this entry is in.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method sets the state of the complex collection element represented by this entry as well as the state of any
    ///         nested complex properties and collections.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public virtual EntityState State
    {
        get => InternalEntry.EntityState;
        set
        {
            if (value < 0
                || (int)value > MaxEntityState)
            {
                throw new ArgumentException(CoreStrings.InvalidEnumValue(value, nameof(value), typeof(EntityState)));
            }

            InternalEntry.SetEntityState(value);
        }
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="property">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual PropertyEntry Property(IProperty property)
    {
        Check.NotNull(property, nameof(property));

        return new PropertyEntry(InternalEntry, property);
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="propertyName">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual PropertyEntry Property(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        return new PropertyEntry(InternalEntry, Metadata.ComplexType.GetProperty(propertyName));
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for all properties of this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public virtual IEnumerable<PropertyEntry> Properties
        => Metadata.ComplexType.GetProperties().Select(property => new PropertyEntry(InternalEntry, property));

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of a nested complex type on this
    ///     complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="property">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexPropertyEntry ComplexProperty(IComplexProperty property)
    {
        Check.NotNull(property, nameof(property));

        return new ComplexPropertyEntry(InternalEntry, property);
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given property of a nested complex type on this
    ///     complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="propertyName">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexPropertyEntry ComplexProperty(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        return new ComplexPropertyEntry(InternalEntry, Metadata.ComplexType.GetComplexProperty(propertyName));
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for all properties of nested complex types on this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public virtual IEnumerable<ComplexPropertyEntry> ComplexProperties
        => Metadata.ComplexType.GetComplexProperties().Select(property => new ComplexPropertyEntry(InternalEntry, property));

    /// <summary>
    ///     Provides access to change tracking information and operations for a given collection property of a complex type on this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="property">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexCollectionEntry ComplexCollection(IComplexProperty property)
    {
        Check.NotNull(property, nameof(property));

        return new ComplexCollectionEntry(InternalEntry, property);
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for a given collection property of a complex type on this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="propertyName">The property to access information and operations for.</param>
    /// <returns>An object that exposes change tracking information and operations for the given property.</returns>
    public virtual ComplexCollectionEntry ComplexCollection(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        return new ComplexCollectionEntry(InternalEntry, Metadata.ComplexType.GetComplexProperty(propertyName));
    }

    /// <summary>
    ///     Provides access to change tracking information and operations for all collection properties of complex type on this complex type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public virtual IEnumerable<ComplexCollectionEntry> ComplexCollections
        => Metadata.ComplexType.GetComplexProperties().Where(p => p.IsCollection).Select(property => new ComplexCollectionEntry(InternalEntry, property));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    InternalComplexEntry IInfrastructure<InternalComplexEntry>.Instance
        => InternalEntry;
}
