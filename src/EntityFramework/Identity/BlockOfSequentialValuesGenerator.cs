// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    /// <summary>
    /// Acts as a <see cref="IValueGenerator"> by requesting a block of values from the
    /// underlying data store and returning them one by one. Will ask the underlying
    /// data store for another block when the current block is exhausted.
    /// </summary>
    public abstract class BlockOfSequentialValuesGenerator : IValueGenerator
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly string _sequenceName;
        private readonly int _blockSize;
        private SequenceValue _currentValue = new SequenceValue(-1, 0);

        public BlockOfSequentialValuesGenerator(
            [NotNull] string sequenceName,
            int blockSize)
        {
            Check.NotEmpty(sequenceName, "sequenceName");

            _sequenceName = sequenceName;
            _blockSize = blockSize;
        }

        public string SequenceName
        {
            get { return _sequenceName; }
        }

        public int BlockSize
        {
            get { return _blockSize; }
        }

        public void Next(StateEntry stateEntry, IProperty property)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            var newValue = GetNextValue();

            // If the chosen value is outside of the current block then we need a new block.
            // It is possible that other threads will use all of the new block before this thread
            // gets a chance to use the new new value, so use a while here to do it all again.
            while (newValue.Current >= newValue.Max)
            {
                using (_lock.Lock())
                {
                    // Once inside the lock check to see if another thread already got a new block, in which
                    // case just get a value out of the new block instead of requesting one.
                    if (newValue.Max == _currentValue.Max)
                    {
                        var newCurrent = GetNewCurrentValue(stateEntry, property);
                        newValue = new SequenceValue(newCurrent, newCurrent + _blockSize);
                        _currentValue = newValue;
                    }
                    else
                    {
                        newValue = GetNextValue();
                    }
                }
            }

            stateEntry[property] = Convert.ChangeType(newValue.Current, property.PropertyType);
        }

        public async Task NextAsync(StateEntry stateEntry, IProperty property, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            var newValue = GetNextValue();

            // If the chosen value is outside of the current block then we need a new block.
            // It is possible that other threads will use all of the new block before this thread
            // gets a chance to use the new new value, so use a while here to do it all again.
            while (newValue.Current >= newValue.Max)
            {
                // Once inside the lock check to see if another thread already got a new block, in which
                // case just get a value out of the new block instead of requesting one.
                using (await _lock.LockAsync(cancellationToken).WithCurrentCulture())
                {
                    if (newValue.Max == _currentValue.Max)
                    {
                        var newCurrent = await GetNewCurrentValueAsync(stateEntry, property, cancellationToken);
                        newValue = new SequenceValue(newCurrent, newCurrent + _blockSize);
                        _currentValue = newValue;
                    }
                    else
                    {
                        newValue = GetNextValue();
                    }
                }
            }

            stateEntry[property] = Convert.ChangeType(newValue.Current, property.PropertyType);
        }

        public abstract long GetNewCurrentValue([NotNull] StateEntry stateEntry, [NotNull] IProperty property);

        public abstract Task<long> GetNewCurrentValueAsync(
            [NotNull] StateEntry stateEntry, [NotNull] IProperty property, CancellationToken cancellationToken);

        private SequenceValue GetNextValue()
        {
            SequenceValue originalValue;
            SequenceValue newValue;
            do
            {
                originalValue = _currentValue;
                newValue = originalValue.NextValue();
            }
            while (Interlocked.CompareExchange(ref _currentValue, newValue, originalValue) != originalValue);

            return newValue;
        }

        private class SequenceValue
        {
            private readonly long _current;
            private readonly long _max;

            public SequenceValue(long current, long max)
            {
                _current = current;
                _max = max;
            }

            public long Current
            {
                get { return _current; }
            }

            public long Max
            {
                get { return _max; }
            }

            public SequenceValue NextValue()
            {
                return new SequenceValue(_current + 1, _max);
            }
        }
    }
}
