// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking information and operations for a given property.
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
/// <typeparam name="TEntity">The type of the entity the property belongs to.</typeparam>
/// <typeparam name="TProperty">The type of the property.</typeparam>
public class PropertyEntry<TEntity, TProperty> : PropertyEntry
    where TEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public PropertyEntry(InternalEntityEntry internalEntry, IProperty property)
        : base(internalEntry, property)
    {
    }

    /// <summary>
    ///     The <see cref="EntityEntry{TEntity}" /> to which this member belongs.
    /// </summary>
    /// <value> An entry for the entity that owns this member. </value>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public new virtual EntityEntry<TEntity> EntityEntry
        => new(InternalEntry);

    /// <summary>
    ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
    ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
    ///     for the context to detect the change.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public new virtual TProperty CurrentValue
    {
        get => InternalEntry.GetCurrentValue<TProperty>(Metadata);
        set => base.CurrentValue = value;
    }

    /// <summary>
    ///     Gets or sets the value that was assigned to this property when it was retrieved from the database.
    ///     This property is populated when an entity is retrieved from the database, but setting it may be
    ///     useful in disconnected scenarios where entities are retrieved with one context instance and
    ///     saved with a different context instance.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public new virtual TProperty OriginalValue
    {
        get => InternalEntry.GetOriginalValue<TProperty>(Metadata);
        set => base.OriginalValue = value;
    }
}
