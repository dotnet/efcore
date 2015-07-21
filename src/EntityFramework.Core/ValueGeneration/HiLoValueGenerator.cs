// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    /// <summary>
    ///     Acts as a <see cref="ValueGenerator" />  by requesting a block of values from the
    ///     underlying database and returning them one by one. Will ask the underlying
    ///     database for another block when the current block is exhausted.
    /// </summary>
    public abstract class HiLoValueGenerator<TValue> : ValueGenerator<TValue>
    {
        private readonly HiLoValueGeneratorState _generatorState;

        protected HiLoValueGenerator([NotNull] HiLoValueGeneratorState generatorState)
        {
            Check.NotNull(generatorState, nameof(generatorState));

            _generatorState = generatorState;
        }

        public override TValue Next() => _generatorState.Next<TValue>(GetNewLowValue);

        protected abstract long GetNewLowValue();
    }
}
