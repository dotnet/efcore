// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

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

        public ComplexCollections(InternalEntryBase entry)
        {
            _entries = new List<InternalComplexEntry>[entry.StructuralType.ComplexCollectionCount];
        }

        public InternalComplexEntry GetEntry(IInternalEntry entry, IComplexProperty property, int ordinal)
        {
            var index = property.GetIndex();

            Check.DebugAssert(index != -1 && index < _entries.Length, "Invalid index on complex property " + property.Name);
            Check.DebugAssert(!IsEmpty, "Complex entries are empty");

            var complexCollectionEntry = _entries[index];
            if (complexCollectionEntry == null)
            {
                complexCollectionEntry = new InternalComplexEntry(entry.StateManager, property.ComplexType, entry, entry[property]);
                _entries[index] = complexCollectionEntry;
            }
            return complexCollectionEntry;
        }

        public void SetValue(object? complexObject, IInternalEntry entry, IComplexProperty property)
        {
            var index = property.GetIndex();
            Check.DebugAssert(index != -1 && index < _entries.Length, "Invalid index on complex property " + property.Name);
            Check.DebugAssert(!IsEmpty, "Complex entries are empty");

            var complexEntry = _entries[index];
            if (complexEntry == null)
            {
                complexEntry = new InternalComplexEntry(entry.StateManager, property.ComplexType, entry, complexObject);
                _entries[index] = complexEntry;
            }
            else
            {
                complexEntry.ComplexObject = complexObject;
            }
        }

        public IEnumerator<IReadOnlyList<InternalComplexEntry>> GetEnumerator()
            => _entries.Where(e => e != null).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _entries.Where(e => e != null).GetEnumerator();

        public bool IsEmpty
            => _entries == null;
    }
}
