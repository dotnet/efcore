// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     <para>
    ///         Acts as a <see cref="ValueGenerator" />  by requesting a block of values from the
    ///         underlying database and returning them one by one. Will ask the underlying
    ///         database for another block when the current block is exhausted.
    ///     </para>
    ///     <para>
    ///         A block is represented by a low value fetched from the database, and then a block size
    ///         that indicates how many sequential values can be used, starting from the low value, before
    ///         a new low value must be fetched from the database.
    ///     </para>
    /// </summary>
    /// <typeparam name="TValue"> The type of values that are generated. </typeparam>
    public abstract class HiLoValueGenerator<TValue> : ValueGenerator<TValue>
    {
        private readonly HiLoValueGeneratorState _generatorState;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HiLoValueGenerator{TValue}" /> class.
        /// </summary>
        /// <param name="generatorState"> The state used to keep track of which value to return next. </param>
        protected HiLoValueGenerator([NotNull] HiLoValueGeneratorState generatorState)
        {
            Check.NotNull(generatorState, nameof(generatorState));

            _generatorState = generatorState;
        }

        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <para>The change tracking entry of the entity for which the value is being generated.</para>
        /// <returns> The value to be assigned to a property. </returns>
        public override TValue Next(EntityEntry entry) => _generatorState.Next<TValue>(GetNewLowValue);

        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <para>The change tracking entry of the entity for which the value is being generated.</para>
        /// <returns> The value to be assigned to a property. </returns>
        public override ValueTask<TValue> NextAsync(
            EntityEntry entry, CancellationToken cancellationToken = default)
            => _generatorState.NextAsync<TValue>(GetNewLowValueAsync);

        /// <summary>
        ///     Gets the low value for the next block of values to be used.
        /// </summary>
        /// <returns> The low value for the next block of values to be used. </returns>
        protected abstract long GetNewLowValue();

        /// <summary>
        ///     Gets the low value for the next block of values to be used.
        /// </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The low value for the next block of values to be used. </returns>
        protected virtual Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetNewLowValue());
    }
}
