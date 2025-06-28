// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class InternalEntryBase
{
    private struct InternalComplexCollectionEntry(InternalEntryBase entry, IComplexProperty complexCollection)
    {
        private List<InternalComplexEntry?>? _entries;
        private List<InternalComplexEntry?>? _originalEntries;
        private bool _isModified;
        private readonly InternalEntryBase _containingEntry = entry;
        private readonly IComplexProperty _complexCollection = complexCollection;

        public List<InternalComplexEntry?> GetOrCreateEntries(
            bool original,
            EntityState defaultState = EntityState.Detached)
        {
            var collection = original
                ? (IList?)_containingEntry.GetOriginalValue(_complexCollection)
                : (IList?)_containingEntry[_complexCollection];
            var entries = EnsureCapacity(collection?.Count ?? 0, original, trim: false);
            if (collection != null
                && defaultState != EntityState.Detached
                && (defaultState != EntityState.Deleted || original)
                && (defaultState != EntityState.Added || !original))
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i] != null)
                    {
                        continue;
                    }

                    var targetState = defaultState;
                    var newEntry = new InternalComplexEntry((IRuntimeComplexType)_complexCollection.ComplexType, _containingEntry, i);
                    if (original)
                    {
                        var otherEntries = _entries;
                        if (defaultState != EntityState.Deleted
                            && otherEntries != null)
                        {
                            if (otherEntries.Count <= i || otherEntries[i] != null)
                            {
                                var nullIndex = otherEntries.FindIndex(e => e == null);
                                if (nullIndex >= 0)
                                {
                                    newEntry.Ordinal = nullIndex;
                                }
                                else
                                {
                                    targetState = EntityState.Deleted;
                                    newEntry.Ordinal = -1;
                                }
                            }
                            else
                            {
                                newEntry.Ordinal = i;
                            }
                        }
                    }
                    else
                    {
                        var otherEntries = _originalEntries;
                        if (defaultState != EntityState.Added
                            && otherEntries != null)
                        {
                            if (otherEntries.Count <= i || otherEntries[i] != null)
                            {
                                var nullIndex = otherEntries.FindIndex(e => e == null);
                                if (nullIndex >= 0)
                                {
                                    newEntry.OriginalOrdinal = nullIndex;
                                }
                                else
                                {
                                    targetState = EntityState.Added;
                                    newEntry.OriginalOrdinal = -1;
                                }
                            }
                            else
                            {
                                newEntry.OriginalOrdinal = i;
                            }
                        }
                    }

                    newEntry.SetEntityState(targetState, acceptChanges: true, modifyProperties: false);
                }
            }

            return entries;
        }

        public List<InternalComplexEntry?> EnsureCapacity(int capacity, bool original, bool trim = true)
        {
            if (original)
            {
                _originalEntries ??= new List<InternalComplexEntry?>(capacity);
                for (var i = _originalEntries.Count; i < capacity; i++)
                {
                    _originalEntries.Add(null);
                }

                if (trim)
                {
                    for (var i = _originalEntries.Count - 1; i >= capacity; i--)
                    {
                        var entry = _originalEntries[i];
                        Check.DebugAssert(entry == null || _containingEntry.EntityState == EntityState.Added,
                            $"Complex entry at original ordinal {i} is not null for property {_complexCollection.Name}.");

                        _originalEntries.RemoveAt(i);
                    }
                }

                return _originalEntries;
            }
            else
            {
                _entries ??= new List<InternalComplexEntry?>(capacity);
                for (var i = _entries.Count; i < capacity; i++)
                {
                    _entries.Add(null);
                }

                if (trim)
                {
                    for (var i = _entries.Count - 1; i >= capacity; i--)
                    {
                        var entry = _entries[i];
                        Check.DebugAssert(entry == null || _containingEntry.EntityState == EntityState.Deleted,
                            $"Complex entry at original ordinal {i} is not null for property {_complexCollection.Name}.");

                        _entries.RemoveAt(i);
                    }
                }

                return _entries;
            }
        }

        public InternalComplexEntry GetEntry(int ordinal, bool original = false)
        {
            if (original)
            {
                if (_containingEntry.EntityState == EntityState.Added)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionOriginalEntryAddedEntity(ordinal, _complexCollection.DeclaringType.ShortNameChain(), _complexCollection.Name));
                }

                if (_containingEntry.GetOriginalValue(_complexCollection) == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionEntryOriginalNull(_complexCollection.DeclaringType.ShortNameChain(), _complexCollection.Name));
                }
            }
            else
            {
                if (_containingEntry.EntityState == EntityState.Deleted)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionEntryDeletedEntity(ordinal, _complexCollection.DeclaringType.ShortNameChain(), _complexCollection.Name));
                }
                if (_containingEntry[_complexCollection] == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionNotInitialized(_complexCollection.DeclaringType.ShortNameChain(), _complexCollection.Name));
                }
            }

            var entries = GetOrCreateEntries(original);
            if (ordinal < 0 || ordinal >= entries.Count)
            {
                if (original)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionEntryOriginalOrdinalInvalid(ordinal, _complexCollection.DeclaringType.ShortNameChain(), _complexCollection.Name, entries.Count));
                }
                else
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionEntryOrdinalInvalid(ordinal, _complexCollection.DeclaringType.ShortNameChain(), _complexCollection.Name, entries.Count));
                }
            }

            var complexEntry = entries[ordinal];
            if (complexEntry != null)
            {
                return complexEntry;
            }

            // The entry is created in Detached state, so it's not added to the entries list yet.
            // HandleStateChange will add it when the state changes.
            return new InternalComplexEntry((IRuntimeComplexType)_complexCollection.ComplexType, _containingEntry, ordinal);
        }

        public readonly void MoveEntry(int fromOrdinal, int toOrdinal, bool original = false)
        {
            if (fromOrdinal == toOrdinal)
            {
                return;
            }

            var entries = original ? _originalEntries : _entries;
            Check.DebugAssert(entries != null, $"Property {_complexCollection.Name} should have{(original ? " original" : "")} entries initialized.");
            if (fromOrdinal < 0 || fromOrdinal >= entries.Count
                || toOrdinal < 0 || toOrdinal >= entries.Count)
            {
                throw new ArgumentOutOfRangeException(
                    CoreStrings.ComplexCollectionMoveInvalidOrdinals(fromOrdinal, toOrdinal, entries.Count));
            }

            var entry = entries[fromOrdinal];
            entries.RemoveAt(fromOrdinal);
            entries.Insert(toOrdinal, entry);

            var start = Math.Min(fromOrdinal, toOrdinal);
            var end = Math.Max(fromOrdinal, toOrdinal);
            for (var i = start; i <= end; i++)
            {
                if (original)
                {
                    entries[i]?.OriginalOrdinal = i;
                }
                else
                {
                    entries[i]?.Ordinal = i;
                }
            }
        }

        public readonly bool IsModified()
        {
            Check.DebugAssert(_complexCollection.IsCollection, $"Property {_complexCollection.Name} should be a collection");
            return _isModified;
        }

        public void SetIsModified(bool isModified)
        {
            Check.DebugAssert(_complexCollection.IsCollection, $"Property {_complexCollection.Name} should be a collection");
            _isModified = isModified;
        }

        public void SetState(EntityState oldState, EntityState newState, bool acceptChanges, bool modifyProperties)
        {
            var setOriginalState = false;
            var setCurrentState = false;
            if (oldState == EntityState.Detached)
            {
                if (newState == EntityState.Deleted)
                {
                    setOriginalState = true;
                }
                else
                {
                    setCurrentState = true;
                }
            }
            else if (oldState == EntityState.Deleted)
            {
                setOriginalState = true;
            }
            else if (oldState == EntityState.Added)
            {
                setCurrentState = true;
            }
            else if (newState is EntityState.Modified or EntityState.Unchanged or EntityState.Added)
            {
                setCurrentState = true;
            }
            else if (newState is EntityState.Deleted or EntityState.Detached)
            {
                setOriginalState = true;
            }

            EnsureCapacity(((IList?)_containingEntry.GetOriginalValue(_complexCollection))?.Count ?? 0,
                original: true);
            EnsureCapacity(((IList?)_containingEntry[_complexCollection])?.Count ?? 0,
                original: false);

            var defaultState = newState == EntityState.Modified && !modifyProperties
                ? EntityState.Unchanged
                : newState;
            var originalEntries = GetOrCreateEntries(original: true, defaultState).ToArray();
            var currentEntries = GetOrCreateEntries(original: false, defaultState).ToArray();
            if (setOriginalState)
            {
                foreach (var originalEntry in originalEntries)
                {
                    originalEntry?.SetEntityState(newState, acceptChanges, modifyProperties);
                }
            }

            if (setCurrentState)
            {
                foreach (var entry in currentEntries)
                {
                    if (entry?.EntityState == EntityState.Unchanged && newState == EntityState.Modified && !modifyProperties)
                    {
                        continue;
                    }

                    entry?.SetEntityState(newState, acceptChanges, modifyProperties);
                }
            }
        }

        public int ValidateOrdinal(InternalComplexEntry entry, bool original)
            => ValidateOrdinal(entry, original, GetOrCreateEntries(original));

        public readonly int ValidateOrdinal(InternalComplexEntry entry, bool original, List<InternalComplexEntry?> entries)
        {
            var ordinal = original ? entry.OriginalOrdinal : entry.Ordinal;
            if (ordinal < 0 || ordinal >= entries.Count)
            {
                var property = entry.ComplexProperty;
                if (original)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionEntryOriginalOrdinalInvalid(ordinal, property.ComplexType.ShortNameChain(), property.Name, ((IList?)_containingEntry.GetOriginalValue(_complexCollection))?.Count ?? 0));
                }
                else
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionEntryOrdinalInvalid(ordinal, property.ComplexType.ShortNameChain(), property.Name, ((IList?)_containingEntry.GetCurrentValue(_complexCollection))?.Count ?? 0));
                }
            }

            return ordinal;
        }

        public void HandleStateChange(InternalComplexEntry entry, EntityState oldState, EntityState newState)
        {
            Check.DebugAssert(oldState != newState, "State didn't change");

            var property = entry.ComplexProperty;
            Check.DebugAssert(property == _complexCollection, $"Expected {_complexCollection.Name}, got {property.Name}");
            if (oldState is EntityState.Detached)
            {
                if (newState is not EntityState.Deleted)
                {
                    InsertEntry(entry, original: false);
                }

                if (newState is not EntityState.Added)
                {
                    InsertEntry(entry, original: true);
                }
            }
            else if (oldState is EntityState.Deleted)
            {
                if (newState is not EntityState.Detached)
                {
                    InsertEntry(entry, original: false);
                }

                // When going from Deleted to Unchanged, restore the entry to the original collection
                if (newState == EntityState.Unchanged)
                {
                    InsertEntry(entry, original: true);
                }
            }
            else if (oldState == EntityState.Added
                && newState is not EntityState.Detached)
            {
                InsertEntry(entry, original: true);
            }

            switch (newState)
            {
                case EntityState.Detached:
                    if (oldState is not EntityState.Deleted)
                    {
                        _entries?[entry.Ordinal] = null;
                    }

                    if (oldState is not EntityState.Added)
                    {
                        _originalEntries?[entry.OriginalOrdinal] = null;
                    }

                    var currentEntries = GetOrCreateEntries(original: false);
                    var originalEntries = GetOrCreateEntries(original: true);

                    if (originalEntries.All(e => e == null || (e.EntityState == EntityState.Unchanged && e.Ordinal == e.OriginalOrdinal))
                        && currentEntries.All(e => e == null || (e.EntityState == EntityState.Unchanged && e.Ordinal == e.OriginalOrdinal)))
                    {
                        _containingEntry.SetPropertyModified(property, false);
                    }
                    break;
                case EntityState.Deleted:
                    if (oldState is not EntityState.Detached
                        && _containingEntry.EntityState != EntityState.Deleted)
                    {
                        RemoveEntry(entry, original: false);
                    }
                    entry.Ordinal = -1;
                    _containingEntry.SetPropertyModified(property, true);
                    break;
                case EntityState.Added:
                    if (oldState is not EntityState.Detached
                        && _containingEntry.EntityState != EntityState.Added)
                    {
                        RemoveEntry(entry, original: true);
                    }
                    entry.OriginalOrdinal = -1;
                    _containingEntry.SetPropertyModified(property, true);
                    break;
                case EntityState.Modified:
                    _containingEntry.SetPropertyModified(property, true);
                    break;
                case EntityState.Unchanged:
                    if (GetOrCreateEntries(original: false).All(e => e == null || (e.EntityState == EntityState.Unchanged && e.Ordinal == e.OriginalOrdinal))
                        && GetOrCreateEntries(original: true).All(e => e == null || (e.EntityState == EntityState.Unchanged && e.Ordinal == e.OriginalOrdinal)))
                    {
                        _containingEntry.SetPropertyModified(property, false);
                    }
                    break;
            }

#if DEBUG
            {
                var currentEntries = GetOrCreateEntries(original: false);
                if (newState is not EntityState.Detached and not EntityState.Deleted)
                {
                    var currentOrdinal = entry.Ordinal;
                    Check.DebugAssert(currentOrdinal >= 0 && currentOrdinal < currentEntries.Count,
                         $"ComplexEntry ordinal {currentOrdinal} is invalid for property {property.Name}.");
                    Check.DebugAssert(currentEntries[currentOrdinal] == entry, $"ComplexEntry at ordinal {currentOrdinal} does not match the provided entry for property {property.Name}.");
                }

                var originalEntries = GetOrCreateEntries(original: true);
                if (newState is not EntityState.Detached and not EntityState.Added)
                {
                    var originalOrdinal = entry.OriginalOrdinal;
                    Check.DebugAssert(originalOrdinal >= 0 && originalOrdinal < originalEntries.Count,
                        $"ComplexEntry original ordinal {originalOrdinal} is invalid for property {property.Name}.");
                    Check.DebugAssert(originalEntries[originalOrdinal] == entry,
                        $"ComplexEntry at OriginalOrdinal {originalOrdinal} does not match the provided entry for property {property.Name}.");
                }
            }
#endif
        }

        private readonly void RemoveEntry(InternalComplexEntry entry, bool original = false)
        {
            var property = entry.ComplexProperty;
            IList? collection;
            List<InternalComplexEntry?>? entries;
            if (original)
            {
                collection = (IList?)_containingEntry.GetOriginalValue(property);
                entries = _originalEntries;
            }
            else
            {
                collection = (IList?)_containingEntry[property];
                entries = _entries;
            }

            if (entries == null)
            {
                return;
            }

            var ordinal = ValidateOrdinal(entry, original, entries);
            if ((collection?.Count ?? 0) < entries.Count
                && (entries[ordinal] == entry
                    || entries[ordinal] == null))
            {
                entries.RemoveAt(ordinal);
                for (var i = ordinal; i < entries.Count; i++)
                {
                    if (original)
                    {
                        entries[i]?.OriginalOrdinal = i;
                    }
                    else
                    {
                        entries[i]?.Ordinal = i;
                    }
                }
            }
            else
            {
                entries[ordinal] = null;
            }
        }

        public readonly void InsertEntry(InternalComplexEntry entry, bool original = false)
        {
            var property = entry.ComplexProperty;
            IList? collection;
            List<InternalComplexEntry?>? entries;
            if (original)
            {
                collection = (IList?)_containingEntry.GetOriginalValue(property);
                entries = _originalEntries;
            }
            else
            {
                collection = (IList?)_containingEntry[property];
                entries = _entries;
            }

            if (collection == null
                || entries == null)
            {
                return;
            }

            var ordinal = ValidateOrdinal(entry, original, entries);
            if (entries[ordinal] == entry
                || entries[ordinal] == null)
            {
                entries[ordinal] = entry;
                return;
            }

            var firstNullIndex = entries.FindIndex(e => e == null || e == entry);
            if (firstNullIndex != -1)
            {
                entries[firstNullIndex] = entry;
                if (firstNullIndex != ordinal)
                {
                    MoveEntry(firstNullIndex, ordinal, original);
                }
            }
            else
            {
                entries.Insert(ordinal, entry);
                for (var i = ordinal + 1; i < entries.Count; i++)
                {
                    if (original)
                    {
                        entries[i]?.OriginalOrdinal = i;
                    }
                    else
                    {
                        entries[i]?.Ordinal = i;
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public readonly override string ToString()
            => ToDebugString(ChangeTrackerDebugStringOptions.ShortDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public readonly DebugView DebugView
        {
            get
            {
                var instance = this;
                return new DebugView(
                        () => instance.ToDebugString(ChangeTrackerDebugStringOptions.ShortDefault),
                        () => instance.ToDebugString());
            }
        }

        private readonly string ToDebugString(
            ChangeTrackerDebugStringOptions options = ChangeTrackerDebugStringOptions.LongDefault)
        {
            var builder = new StringBuilder();
            try
            {
                builder.Append(_complexCollection.ToDebugString(MetadataDebugStringOptions.SingleLineDefault));

                if (IsModified())
                {
                    builder.Append(" Modified");
                }
            }
            catch (Exception exception)
            {
                builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
            }

            return builder.ToString();
        }
    }
}
