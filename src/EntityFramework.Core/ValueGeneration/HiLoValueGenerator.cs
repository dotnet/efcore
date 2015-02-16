// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    /// <summary>
    ///     Acts as a <see cref="ValueGenerator" />  by requesting a block of values from the
    ///     underlying data store and returning them one by one. Will ask the underlying
    ///     data store for another block when the current block is exhausted.
    /// </summary>
    public abstract class HiLoValueGenerator<TValue> : ValueGenerator<TValue>
    {
        private readonly object _lock = new object();
        private HiLoValue _currentValue = new HiLoValue(-1, 0);

        protected HiLoValueGenerator(int blockSize)
        {
            BlockSize = blockSize;
        }

        public virtual int BlockSize { get; }

        public override TValue Next(DbContextService<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(dataStoreServices, nameof(dataStoreServices));

            var newValue = GetNextValue();

            // If the chosen value is outside of the current block then we need a new block.
            // It is possible that other threads will use all of the new block before this thread
            // gets a chance to use the new new value, so use a while here to do it all again.
            while (newValue.Low >= newValue.High)
            {
                lock (_lock)
                {
                    // Once inside the lock check to see if another thread already got a new block, in which
                    // case just get a value out of the new block instead of requesting one.
                    if (newValue.High == _currentValue.High)
                    {
                        var newCurrent = GetNewHighValue(dataStoreServices);
                        newValue = new HiLoValue(newCurrent, newCurrent + BlockSize);
                        _currentValue = newValue;
                    }
                    else
                    {
                        newValue = GetNextValue();
                    }
                }
            }

            return (TValue)Convert.ChangeType(newValue.Low, typeof(TValue));
        }

        protected abstract long GetNewHighValue([NotNull] DbContextService<DataStoreServices> dataStoreServices);

        public override bool GeneratesTemporaryValues => false;

        private HiLoValue GetNextValue()
        {
            HiLoValue originalValue;
            HiLoValue newValue;
            do
            {
                originalValue = _currentValue;
                newValue = originalValue.NextValue();
            }
            while (Interlocked.CompareExchange(ref _currentValue, newValue, originalValue) != originalValue);

            return newValue;
        }

        private class HiLoValue
        {
            public HiLoValue(long low, long high)
            {
                Low = low;
                High = high;
            }

            public long Low { get; }

            public long High { get; }

            public HiLoValue NextValue()
            {
                return new HiLoValue(Low + 1, High);
            }
        }
    }
}
