// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public sealed partial class InternalEntityEntry
{
    private readonly struct ComplexEntries : IEnumerable<InternalComplexEntry>
    {
        private readonly InternalComplexEntry?[] _entries;

        public ComplexEntries(IInternalEntry entry)
        {
            _entries = new InternalComplexEntry[entry.StructuralType.ComplexPropertyCount];
        }

        public InternalComplexEntry GetEntry(IInternalEntry entry, IComplexProperty property)
        {
            var index = property.GetIndex();

            Check.DebugAssert(index != -1 && index < _entries.Length, "Invalid index on complex property " + property.Name);
            Check.DebugAssert(!IsEmpty, "Complex entries are empty");

            var complexEntry = _entries[index];
            if (complexEntry == null)
            {
                complexEntry = new InternalComplexEntry(entry.StateManager, property.ComplexType, entry, entry[property]);
                _entries[index] = complexEntry;
            }
            return complexEntry;
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

        public IEnumerator<InternalComplexEntry> GetEnumerator()
            => _entries.Where(e => e != null).GetEnumerator()!;

        IEnumerator IEnumerable.GetEnumerator()
            => _entries.Where(e => e != null).GetEnumerator();

        public bool IsEmpty
            => _entries == null;
    }
}
