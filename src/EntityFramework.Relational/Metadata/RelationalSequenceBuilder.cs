// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalSequenceBuilder
    {
        private Sequence _sequence;
        private readonly Action<Sequence> _updateAction;

        public RelationalSequenceBuilder(
            [NotNull] Sequence sequence,
            [NotNull] Action<Sequence> updateAction)
        {
            Check.NotNull(sequence, nameof(sequence));
            Check.NotNull(updateAction, nameof(updateAction));

            _sequence = sequence;
            _updateAction = updateAction;
        }

        public virtual RelationalSequenceBuilder IncrementBy(int increment)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                increment,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.Type,
                _sequence.Cycle);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Start(long startValue)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                startValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.Type,
                _sequence.Cycle);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Type<T>()
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                typeof(T),
                _sequence.Cycle);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Max(long maximum)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                maximum,
                _sequence.Type,
                _sequence.Cycle);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Min(long minimum)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                minimum,
                _sequence.MaxValue,
                _sequence.Type,
                _sequence.Cycle);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Cycle(bool cycle = true)
        {
            var model = (Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.Type,
                cycle);

            _updateAction(_sequence);

            return this;
        }
    }
}
