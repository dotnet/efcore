// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
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
    private readonly struct ComplexCollections : IEnumerable<IReadOnlyList<InternalComplexEntry>>
    {
        private readonly List<InternalComplexEntry>[] _entries;
        private readonly Dictionary<int, List<InternalComplexEntry>> _deletedEntries;
        private readonly BitArray _modifiedComplexProperties;
        private readonly InternalEntryBase _containingEntry;

        public ComplexCollections(InternalEntryBase entry)
        {
            _containingEntry = entry;
            _entries = new List<InternalComplexEntry>[entry.StructuralType.ComplexCollectionCount];
            _deletedEntries = [];
            foreach (var complexProperty in entry.StructuralType.GetFlattenedComplexProperties())
            {
                if (complexProperty.IsCollection)
                {
                    var collection = (IList?)entry[complexProperty];
                    if (collection != null)
                    {
                        var entryList = new List<InternalComplexEntry>(collection.Count);
                        _entries[complexProperty.GetIndex()] = entryList;

                        for (var i = 0; i < collection.Count; i++)
                        {
                            entryList.Add(new InternalComplexEntry((IRuntimeComplexType)complexProperty.ComplexType, entry, i));
                        }
                    }
                }
            }

            _modifiedComplexProperties = new(entry.StructuralType.ComplexPropertyCount);
        }

        public InternalComplexEntry GetEntry(IComplexProperty property, int ordinal)
        {
            var index = property.GetIndex();

            Check.DebugAssert(index != -1 && index < _entries.Length, "Invalid index on complex property " + property.Name);
            Check.DebugAssert(!IsEmpty, "Complex entries are empty");

            return _entries[index][ordinal];
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool IsModified(IComplexProperty property)
            => _modifiedComplexProperties[property.GetIndex()];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void SetIsModified(IComplexProperty property, bool isModified)
        {
            _modifiedComplexProperties[property.GetIndex()] = isModified;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void MarkDeleted(IComplexProperty property, int ordinal)
        {
            var index = property.GetIndex();
            if (!_deletedEntries.TryGetValue(index, out var deletedList))
            {
                deletedList = new List<InternalComplexEntry>();
                _deletedEntries[index] = deletedList;
            }

            deletedList.Add(new InternalComplexEntry((IRuntimeComplexType)property.ComplexType, _containingEntry, ordinal));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IReadOnlyList<InternalComplexEntry> GetDeletedEntries(IComplexProperty property)
        {
            var index = property.GetIndex();
            return _deletedEntries.TryGetValue(index, out var deletedList)
                ? deletedList
                : Array.Empty<InternalComplexEntry>();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IEnumerable<InternalComplexEntry> GetAllDeletedEntries()
            => _deletedEntries.Values.SelectMany(list => list);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IEnumerable<InternalComplexEntry> GetAllEntries(IComplexProperty property)
        {
            var index = property.GetIndex();
            return _entries != null && _entries[index] != null
                ? _entries[index]
                : Array.Empty<InternalComplexEntry>();
        }

        public IEnumerator<IReadOnlyList<InternalComplexEntry>> GetEnumerator()
            => _entries.Where(e => e != null).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _entries.Where(e => e != null).GetEnumerator();

        public bool IsEmpty
            => _entries == null;
    }
}
