// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     Acts as a <see cref="ValueGenerator" />  by requesting a block of values from the
///     underlying database and returning them one by one. Will ask the underlying
///     database for another block when the current block is exhausted.
/// </summary>
/// <remarks>
///     A block is represented by a low value fetched from the database, and then a block size
///     that indicates how many sequential values can be used, starting from the low value, before
///     a new low value must be fetched from the database.
/// </remarks>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
/// <typeparam name="TValue">The type of values that are generated.</typeparam>
public abstract class HiLoValueGenerator<TValue> : ValueGenerator<TValue>
{
    private readonly HiLoValueGeneratorState _generatorState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HiLoValueGenerator{TValue}" /> class.
    /// </summary>
    /// <param name="generatorState">The state used to keep track of which value to return next.</param>
    protected HiLoValueGenerator(HiLoValueGeneratorState generatorState)
    {
        _generatorState = generatorState;
    }

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The value to be assigned to a property.</returns>
    public override TValue Next(EntityEntry entry)
        => _generatorState.Next<TValue>(GetNewLowValue);

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The value to be assigned to a property.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public override ValueTask<TValue> NextAsync(
        EntityEntry entry,
        CancellationToken cancellationToken = default)
        => _generatorState.NextAsync<TValue>(GetNewLowValueAsync, cancellationToken);

    /// <summary>
    ///     Gets the low value for the next block of values to be used.
    /// </summary>
    /// <returns>The low value for the next block of values to be used.</returns>
    protected abstract long GetNewLowValue();

    /// <summary>
    ///     Gets the low value for the next block of values to be used.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The low value for the next block of values to be used.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(GetNewLowValue());
}
