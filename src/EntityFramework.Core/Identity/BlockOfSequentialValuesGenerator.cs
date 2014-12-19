// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    /// <summary>
    ///     Acts as a <see cref="IValueGenerator" />  by requesting a block of values from the
    ///     underlying data store and returning them one by one. Will ask the underlying
    ///     data store for another block when the current block is exhausted.
    /// </summary>
    public abstract class BlockOfSequentialValuesGenerator : IValueGenerator
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private SequenceValue _currentValue = new SequenceValue(-1, 0);

        protected BlockOfSequentialValuesGenerator(
            [NotNull] string sequenceName,
            int blockSize)
        {
            Check.NotEmpty(sequenceName, "sequenceName");

            SequenceName = sequenceName;
            BlockSize = blockSize;
        }

        public virtual string SequenceName { get; }

        public virtual int BlockSize { get; }

        public virtual object Next(IProperty property, DbContextService<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(property, "property");
            Check.NotNull(dataStoreServices, "dataStoreServices");

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
                        var newCurrent = GetNewCurrentValue(property, dataStoreServices);
                        newValue = new SequenceValue(newCurrent, newCurrent + BlockSize);
                        _currentValue = newValue;
                    }
                    else
                    {
                        newValue = GetNextValue();
                    }
                }
            }

            return Convert.ChangeType(newValue.Current, property.PropertyType.UnwrapNullableType());
        }

        public virtual async Task<object> NextAsync(
            IProperty property,
            DbContextService<DataStoreServices> dataStoreServices,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(property, "property");
            Check.NotNull(dataStoreServices, "dataStoreServices");

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
                        var newCurrent = await GetNewCurrentValueAsync(property, dataStoreServices, cancellationToken);
                        newValue = new SequenceValue(newCurrent, newCurrent + BlockSize);
                        _currentValue = newValue;
                    }
                    else
                    {
                        newValue = GetNextValue();
                    }
                }
            }

            return Convert.ChangeType(newValue.Current, property.PropertyType.UnwrapNullableType());
        }

        protected abstract long GetNewCurrentValue(
            [NotNull] IProperty property,
            [NotNull] DbContextService<DataStoreServices> dataStoreServices);

        protected abstract Task<long> GetNewCurrentValueAsync(
            [NotNull] IProperty property,
            [NotNull] DbContextService<DataStoreServices> dataStoreServices,
            CancellationToken cancellationToken);

        public virtual bool GeneratesTemporaryValues => false;

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
            public SequenceValue(long current, long max)
            {
                Current = current;
                Max = max;
            }

            public long Current { get; }

            public long Max { get; }

            public SequenceValue NextValue()
            {
                return new SequenceValue(Current + 1, Max);
            }
        }
    }
}
