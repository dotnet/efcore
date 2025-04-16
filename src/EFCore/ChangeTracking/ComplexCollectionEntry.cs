// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

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
public class ComplexCollectionEntry : MemberEntry, IEnumerable<ComplexEntry>
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
        get => (IEnumerable?)base.CurrentValue;
        set => base.CurrentValue = value;
    }

    /// <summary>
    ///     Gets a <see cref="ComplexEntry"/> for the complex item at the specified index.
    /// </summary>
    /// <param name="index">The index of the complex item to access.</param>
    /// <returns>A <see cref="ComplexEntry"/> for the complex item at the specified index.</returns>
    public virtual ComplexEntry this[int index]
    {
        get
        {
            var currentValue = CurrentValue;
            if (currentValue == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.CollectionNotFound(Metadata.Name, InternalEntry.EntityType.DisplayName()));
            }

            var enumerableValue = currentValue as IList ?? currentValue.Cast<object>().ToList();

            if (index < 0 || index >= enumerableValue.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var element = enumerableValue[index];

            return new ComplexEntry(InternalEntry.GetComplexCollectionEntry(Metadata, index));
        }
    }

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
        get
        {
            var stateManager = InternalEntry.StateManager;

            if (Metadata is ISkipNavigation skipNavigation)
            {
                if (InternalEntry.EntityState != EntityState.Unchanged
                    && InternalEntry.EntityState != EntityState.Detached)
                {
                    return true;
                }

                var joinEntityType = skipNavigation.JoinEntityType;
                var foreignKey = skipNavigation.ForeignKey;
                var inverseForeignKey = skipNavigation.Inverse.ForeignKey;
                foreach (var joinEntry in stateManager.Entries)
                {
                    if (joinEntry.EntityType == joinEntityType
                        && stateManager.FindPrincipal(joinEntry, foreignKey) == InternalEntry
                        && (joinEntry.EntityState == EntityState.Added
                            || joinEntry.EntityState == EntityState.Deleted
                            || foreignKey.Properties.Any(joinEntry.IsModified)
                            || inverseForeignKey.Properties.Any(joinEntry.IsModified)
                            || (stateManager.FindPrincipal(joinEntry, inverseForeignKey)?.EntityState == EntityState.Deleted)))
                    {
                        return true;
                    }
                }
            }
            else
            {
                var navigationValue = CurrentValue;
                if (navigationValue != null)
                {
                    var targetEntityType = Metadata.TargetEntityType;
                    var foreignKey = ((INavigation)Metadata).ForeignKey;

                    foreach (var relatedEntity in navigationValue)
                    {
                        var relatedEntry = stateManager.TryGetEntry(relatedEntity, targetEntityType);

                        if (relatedEntry != null
                            && (relatedEntry.EntityState == EntityState.Added
                                || relatedEntry.EntityState == EntityState.Deleted
                                || foreignKey.Properties.Any(relatedEntry.IsModified)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        set
        {
            var stateManager = InternalEntry.StateManager;

            if (Metadata is ISkipNavigation skipNavigation)
            {
                var joinEntityType = skipNavigation.JoinEntityType;
                var foreignKey = skipNavigation.ForeignKey;
                foreach (var joinEntry in stateManager
                             .GetEntriesForState(added: !value, modified: !value, deleted: !value, unchanged: value).Where(
                                 e => e.EntityType == joinEntityType
                                     && stateManager.FindPrincipal(e, foreignKey) == InternalEntry)
                             .ToList())
                {
                    joinEntry.SetEntityState(value ? EntityState.Modified : EntityState.Unchanged);
                }
            }
            else
            {
                var foreignKey = ((INavigation)Metadata).ForeignKey;
                var navigationValue = CurrentValue;
                if (navigationValue != null)
                {
                    foreach (var relatedEntity in navigationValue)
                    {
                        var relatedEntry = InternalEntry.StateManager.TryGetEntry(relatedEntity, Metadata.TargetEntityType);
                        if (relatedEntry != null)
                        {
                            var anyNonPk = foreignKey.Properties.Any(p => !p.IsPrimaryKey());
                            foreach (var property in foreignKey.Properties)
                            {
                                if (anyNonPk
                                    && !property.IsPrimaryKey())
                                {
                                    relatedEntry.SetPropertyModified(property, isModified: value, acceptChanges: false);
                                }
                            }
                        }
                    }
                }
            }
        }
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

        foreach (var _ in currentValue)
        {
            yield return new ComplexEntry(InternalEntry, (IComplexProperty)Metadata);
        }
    }

    /// <summary>
    ///     Gets an enumerator over all complex entries in this collection.
    /// </summary>
    /// <returns>An enumerator over all complex entries in this collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
