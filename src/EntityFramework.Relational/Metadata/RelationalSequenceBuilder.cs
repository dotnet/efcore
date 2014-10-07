// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalSequenceBuilder
    {
        private Sequence _sequence;

        public RelationalSequenceBuilder([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            _sequence = sequence;
        }

        public virtual RelationalSequenceBuilder IncrementBy(int increment)
        {
            var model = (Entity.Metadata.Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name, 
                _sequence.Schema, 
                _sequence.StartValue, 
                increment, 
                _sequence.MinValue, 
                _sequence.MaxValue, 
                _sequence.Type);

            model.Relational().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Start(long startValue)
        {
            var model = (Entity.Metadata.Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                startValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.Type);

            model.Relational().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Type<T>()
        {
            var model = (Entity.Metadata.Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                typeof(T));

            model.Relational().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Max(long maximum)
        {
            var model = (Entity.Metadata.Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                maximum,
                _sequence.Type);

            model.Relational().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual RelationalSequenceBuilder Min(long minimum)
        {
            var model = (Entity.Metadata.Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                minimum,
                _sequence.MaxValue,
                _sequence.Type);

            model.Relational().AddOrReplaceSequence(_sequence);

            return this;
        }
    }
}
