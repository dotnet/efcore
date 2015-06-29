// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerSequenceBuilder
    {
        private Sequence _sequence;

        public SqlServerSequenceBuilder([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            _sequence = sequence;
        }

        public virtual SqlServerSequenceBuilder IncrementBy(int increment)
        {
            var model = (Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                increment,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.Type,
                _sequence.Cycle);

            model.SqlServer().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual SqlServerSequenceBuilder Start(long startValue)
        {
            var model = (Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                startValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                _sequence.Type,
                _sequence.Cycle);

            model.SqlServer().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual SqlServerSequenceBuilder Type<T>()
        {
            var model = (Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                _sequence.MaxValue,
                typeof(T),
                _sequence.Cycle);

            model.SqlServer().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual SqlServerSequenceBuilder Max(long maximum)
        {
            var model = (Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                _sequence.MinValue,
                maximum,
                _sequence.Type,
                _sequence.Cycle);

            model.SqlServer().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual SqlServerSequenceBuilder Min(long minimum)
        {
            var model = (Model)_sequence.Model;

            _sequence = new Sequence(
                _sequence.Name,
                _sequence.Schema,
                _sequence.StartValue,
                _sequence.IncrementBy,
                minimum,
                _sequence.MaxValue,
                _sequence.Type,
                _sequence.Cycle);

            model.SqlServer().AddOrReplaceSequence(_sequence);

            return this;
        }

        public virtual SqlServerSequenceBuilder Cycle(bool cycle = true)
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

            model.SqlServer().AddOrReplaceSequence(_sequence);

            return this;
        }
    }
}
