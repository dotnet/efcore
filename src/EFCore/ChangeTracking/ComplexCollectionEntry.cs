// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking and loading information for a collection
///     navigation complexProperty that associates this entity to a collection of another entities.
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
public class ComplexCollectionEntry : MemberEntry
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexCollectionEntry(IInternalEntry internalEntry, IComplexProperty complexProperty)
        : base(internalEntry, complexProperty)
    {
        if (!complexProperty.IsCollection)
        {
            throw new InvalidOperationException(
                CoreStrings.ComplexCollectionIsReference(
                    internalEntry.StructuralType.DisplayName(), complexProperty.Name,
                    nameof(ChangeTracking.EntityEntry.ComplexCollection), nameof(ChangeTracking.EntityEntry.ComplexProperty)));
        }

        DetectChanges();
    }

    private void DetectChanges()
    {
        var context = InternalEntry.Context;
        if (!context.ChangeTracker.AutoDetectChangesEnabled
            || ((IRuntimeModel)context.Model).SkipDetectChanges)
        {
            return;
        }

        var changeDetector = context.GetDependencies().ChangeDetector;
        foreach (var complexEntry in InternalEntry.GetFlattenedComplexEntries())
        {
            changeDetector.DetectChanges(complexEntry);
        }
    }

    /// <summary>
    ///     Gets or sets the value currently assigned to this complexProperty. If the current value is set using this complexProperty,
    ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
    ///     for the context to detect the change.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    public new virtual IEnumerable? CurrentValue
    {
        get => this.GetInfrastructure().GetCurrentValue<IEnumerable?>(Metadata);
        set => base.CurrentValue = value;
    }

    /// <summary>
    ///     Gets a <see cref="ComplexEntry"/> for the complex item at the specified ordinal.
    /// </summary>
    /// <param name="ordinal">The ordinal of the complex item to access.</param>
    /// <returns>A <see cref="ComplexEntry"/> for the complex item at the specified ordinal.</returns>
    public virtual ComplexEntry this[int ordinal]
        => new(InternalEntry.GetComplexCollectionEntry(Metadata, ordinal));

    /// <summary>
    ///     Gets a <see cref="ComplexEntry"/> for the original complex item at the specified ordinal.
    /// </summary>
    /// <param name="ordinal">The original ordinal of the complex item to access.</param>
    /// <returns>A <see cref="ComplexEntry"/> for the complex item at the specified original ordinal.</returns>
    public virtual ComplexEntry GetOriginalEntry(int ordinal)
        => new(InternalEntry.GetComplexCollectionOriginalEntry(Metadata, ordinal));

    /// <summary>
    ///     Gets the metadata that describes the facets of this property and how it maps to the database.
    /// </summary>
    public new virtual IComplexProperty Metadata
        => (IComplexProperty)base.Metadata;

    /// <summary>
    ///     Gets or sets a value indicating whether any of foreign key complexProperty values associated
    ///     with this navigation complexProperty have been modified and should be updated in the database
    ///     when <see cref="DbContext.SaveChanges()" /> is called.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-entity-entries">Accessing tracked entities in EF Core</see>
    ///     and <see href="https://aka.ms/efcore-docs-changing-relationships">Changing foreign keys and navigations</see>
    ///     for more information and examples.
    /// </remarks>
    public override bool IsModified
    {
        get => InternalEntry.IsModified(Metadata);
        set => InternalEntry.SetPropertyModified(Metadata, isModified: value, recurse: true);
    }

    /// <summary>
    ///     Gets an enumerator over all complex entries in this collection.
    /// </summary>
    /// <returns>An enumerator over all complex entries in this collection.</returns>
    public virtual IEnumerator<ComplexEntry> GetEnumerator()
    {
        var currentValue = CurrentValue;
        if (currentValue == null)
        {
            yield break;
        }

        foreach (var complexEntry in InternalEntry.GetFlattenedComplexEntries())
        {
            yield return new ComplexEntry(complexEntry);
        }
    }
}
