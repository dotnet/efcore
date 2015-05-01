// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class HiLoValueGeneratorState
    {
        private readonly object[] _locks;
        private readonly HiLoValue[] _pool;
        private int _poolIndex;
        private readonly int _blockSize;

        public HiLoValueGeneratorState(int blockSize, int poolSize)
        {
            _blockSize = blockSize;

            _pool = new HiLoValue[poolSize];
            _locks = new object[poolSize];
            for (var i = 0; i < poolSize; i++)
            {
                _pool[i] = new HiLoValue(-1, 0);
                _locks[i] = new object();
                ;
            }
        }

        public virtual TValue Next<TValue>([NotNull] Func<long> getNewLowValue)
        {
            Check.NotNull(getNewLowValue, nameof(getNewLowValue));

            var poolIndexToUse = _poolIndex;
            _poolIndex = (_poolIndex + 1) % _pool.Length;

            var newValue = GetNextValue(poolIndexToUse);

            // If the chosen value is outside of the current block then we need a new block.
            // It is possible that other threads will use all of the new block before this thread
            // gets a chance to use the new new value, so use a while here to do it all again.
            while (newValue.Low >= newValue.High)
            {
                lock (_locks[_poolIndex])
                {
                    // Once inside the lock check to see if another thread already got a new block, in which
                    // case just get a value out of the new block instead of requesting one.
                    if (newValue.High == _pool[poolIndexToUse].High)
                    {
                        var newCurrent = getNewLowValue();
                        newValue = new HiLoValue(newCurrent, newCurrent + _blockSize);
                        _pool[poolIndexToUse] = newValue;
                    }
                    else
                    {
                        newValue = GetNextValue(poolIndexToUse);
                    }
                }
            }

            return (TValue)Convert.ChangeType(newValue.Low, typeof(TValue));
        }

        private HiLoValue GetNextValue(int poolIndexToUse)
        {
            HiLoValue originalValue;
            HiLoValue newValue;
            do
            {
                originalValue = _pool[poolIndexToUse];
                newValue = originalValue.NextValue();
            }
            while (Interlocked.CompareExchange(ref _pool[poolIndexToUse], newValue, originalValue) != originalValue);

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

            public HiLoValue NextValue() => new HiLoValue(Low + 1, High);
        }
    }
}
