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

        public virtual Sequence Metadata => _sequence;

        public virtual RelationalSequenceBuilder IncrementsBy(int increment)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                increment,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.ClrType,
                _sequence.IsCyclic);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder StartsAt(long startValue)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                startValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.ClrType,
                _sequence.IsCyclic);

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
                _sequence.IsCyclic);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder HasMax(long maximum)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                maximum,
                _sequence.ClrType,
                _sequence.IsCyclic);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder HasMin(long minimum)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                minimum,
                _sequence.MaxValue,
                _sequence.ClrType,
                _sequence.IsCyclic);

            _updateAction(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder IsCyclic(bool isCyclic = true)
        {
            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.ClrType,
                isCyclic);

            _updateAction(_sequence);

            return this;
        }
    }
}
