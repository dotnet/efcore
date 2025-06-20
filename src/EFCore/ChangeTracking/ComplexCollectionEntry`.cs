// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking and loading information for a collection
///     navigation property that associates this entity to a collection of another entities.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
///         not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>,
///         <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>,
///         and <see href="https://aka.ms/efcore-docs-load-related-data">Loading related entities</see> for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TEntity">The type of the entity the property belongs to.</typeparam>
/// <typeparam name="TElement">The element type.</typeparam>
public class ComplexCollectionEntry<TEntity, TElement> : ComplexCollectionEntry
    where TEntity : class
    where TElement : notnull
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexCollectionEntry(IInternalEntry internalEntry, IComplexProperty property)
        : base(internalEntry, property)
    {
    }

    /// <summary>
    ///     The <see cref="EntityEntry{TEntity}" /> to which this member belongs.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <value> An entry for the entity that owns this member. </value>
    public override EntityEntry<TEntity> EntityEntry
        => new(InternalEntry.EntityEntry);

    /// <summary>
    ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
    ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
    ///     for the context to detect the change.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    public new virtual IReadOnlyList<TElement>? CurrentValue
    {
        get => this.GetInfrastructure().GetCurrentValue<IReadOnlyList<TElement>?>(Metadata);
        set => base.CurrentValue = value;
    }

    /// <summary>
    ///     Gets a <see cref="ComplexEntry{TEntity, TElement}"/> for the complex item at the specified ordinal.
    /// </summary>
    /// <param name="ordinal">The ordinal of the complex item to access.</param>
    /// <returns>A <see cref="ComplexEntry{TEntity, TElement}"/> for the complex item at the specified ordinal.</returns>
    public override ComplexEntry<TEntity, TElement> this[int ordinal]
        => new(InternalEntry.GetComplexCollectionEntry(Metadata, ordinal));

    /// <summary>
    ///     Gets a <see cref="ComplexEntry{TEntity, TElement}"/> for the complex item at the specified original ordinal.
    /// </summary>
    /// <param name="ordinal">The original ordinal of the complex item to access.</param>
    /// <returns>A <see cref="ComplexEntry{TEntity, TElement}"/> for the original complex item at the specified ordinal.</returns>
    public override ComplexEntry<TEntity, TElement> GetOriginalEntry(int ordinal)
        => new(InternalEntry.GetComplexCollectionOriginalEntry(Metadata, ordinal));
}
