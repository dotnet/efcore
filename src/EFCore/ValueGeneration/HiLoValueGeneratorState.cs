// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     The thread safe state used by <see cref="HiLoValueGenerator{TValue}" />.
    /// </summary>
    public class HiLoValueGeneratorState
    {
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private HiLoValue _currentValue;
        private readonly int _blockSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HiLoValueGeneratorState" /> class.
        /// </summary>
        /// <param name="blockSize">
        ///     The number of sequential values that can be used, starting from the low value, before
        ///     a new low value must be fetched from the database.
        /// </param>
        public HiLoValueGeneratorState(int blockSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), CoreStrings.HiLoBadBlockSize);
            }

            _blockSize = blockSize;
            _currentValue = new HiLoValue(-1, 0);
        }

        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <typeparam name="TValue"> The type of values being generated. </typeparam>
        /// <param name="getNewLowValue">
        ///     A function to get the next low value if needed.
        /// </param>
        /// <returns> The value to be assigned to a property. </returns>
        public virtual TValue Next<TValue>([NotNull] Func<long> getNewLowValue)
        {
            Check.NotNull(getNewLowValue, nameof(getNewLowValue));

            var newValue = GetNextValue();

            // If the chosen value is outside of the current block then we need a new block.
            // It is possible that other threads will use all of the new block before this thread
            // gets a chance to use the new new value, so use a while here to do it all again.
            while (newValue.Low >= newValue.High)
            {
                using (_asyncLock.Lock())
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

            return ConvertResult<TValue>(newValue);
        }

        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <typeparam name="TValue"> The type of values being generated. </typeparam>
        /// <param name="getNewLowValue">
        ///     A function to get the next low value if needed.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> The value to be assigned to a property. </returns>
        public virtual async Task<TValue> NextAsync<TValue>(
            [NotNull] Func<CancellationToken, Task<long>> getNewLowValue,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(getNewLowValue, nameof(getNewLowValue));

            var newValue = GetNextValue();

            // If the chosen value is outside of the current block then we need a new block.
            // It is possible that other threads will use all of the new block before this thread
            // gets a chance to use the new new value, so use a while here to do it all again.
            while (newValue.Low >= newValue.High)
            {
                using (await _asyncLock.LockAsync())
                {
                    // Once inside the lock check to see if another thread already got a new block, in which
                    // case just get a value out of the new block instead of requesting one.
                    if (newValue.High == _currentValue.High)
                    {
                        var newCurrent = await getNewLowValue(cancellationToken);
                        newValue = new HiLoValue(newCurrent, newCurrent + _blockSize);
                        _currentValue = newValue;
                    }
                    else
                    {
                        newValue = GetNextValue();
                    }
                }
            }

            return ConvertResult<TValue>(newValue);
        }

        private static TValue ConvertResult<TValue>(HiLoValue newValue)
            => (TValue)Convert.ChangeType(newValue.Low, typeof(TValue), CultureInfo.InvariantCulture);

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
