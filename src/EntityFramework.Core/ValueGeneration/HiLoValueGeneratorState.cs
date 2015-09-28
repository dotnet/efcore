// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class HiLoValueGeneratorState
    {
        private readonly object _lock;
        private HiLoValue _currentValue;
        private readonly int _blockSize;

        public HiLoValueGeneratorState(int blockSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), CoreStrings.HiLoBadBlockSize);
            }

            _blockSize = blockSize;
            _currentValue = new HiLoValue(-1, 0);
            _lock = new object();
        }

        public virtual TValue Next<TValue>([NotNull] Func<long> getNewLowValue)
        {
            Check.NotNull(getNewLowValue, nameof(getNewLowValue));

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
                        var newCurrent = getNewLowValue();
                        newValue = new HiLoValue(newCurrent, newCurrent + _blockSize);
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

            public HiLoValue NextValue() => new HiLoValue(Low + 1, High);
        }
    }
}
