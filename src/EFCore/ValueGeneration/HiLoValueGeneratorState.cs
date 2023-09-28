// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     The thread safe state used by <see cref="HiLoValueGenerator{TValue}" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
public class HiLoValueGeneratorState : IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
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
    /// <typeparam name="TValue">The type of values being generated.</typeparam>
    /// <param name="getNewLowValue">
    ///     A function to get the next low value if needed.
    /// </param>
    /// <returns>The value to be assigned to a property.</returns>
    public virtual TValue Next<TValue>(Func<long> getNewLowValue)
    {
        var newValue = GetNextValue();

        // If the chosen value is outside of the current block then we need a new block.
        // It is possible that other threads will use all of the new block before this thread
        // gets a chance to use the new value, so use a while here to do it all again.
        while (newValue.Low >= newValue.High)
        {
            _semaphoreSlim.Wait();
            try
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
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        return ConvertResult<TValue>(newValue);
    }

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <typeparam name="TValue">The type of values being generated.</typeparam>
    /// <param name="getNewLowValue">
    ///     A function to get the next low value if needed.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The value to be assigned to a property.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async ValueTask<TValue> NextAsync<TValue>(
        Func<CancellationToken, Task<long>> getNewLowValue,
        CancellationToken cancellationToken = default)
    {
        var newValue = GetNextValue();

        // If the chosen value is outside of the current block then we need a new block.
        // It is possible that other threads will use all of the new block before this thread
        // gets a chance to use the new value, so use a while here to do it all again.
        while (newValue.Low >= newValue.High)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Once inside the lock check to see if another thread already got a new block, in which
                // case just get a value out of the new block instead of requesting one.
                if (newValue.High == _currentValue.High)
                {
                    var newCurrent = await getNewLowValue(cancellationToken).ConfigureAwait(false);
                    newValue = new HiLoValue(newCurrent, newCurrent + _blockSize);
                    _currentValue = newValue;
                }
                else
                {
                    newValue = GetNextValue();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
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

    private sealed class HiLoValue
    {
        public HiLoValue(long low, long high)
        {
            Low = low;
            High = high;
        }

        public long Low { get; }

        public long High { get; }

        public HiLoValue NextValue()
            => new(Low + 1, High);
    }

    /// <summary>
    ///     Releases the allocated resources for this instance.
    /// </summary>
    public virtual void Dispose()
        => _semaphoreSlim.Dispose();
}
