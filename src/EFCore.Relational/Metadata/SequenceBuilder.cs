// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SequenceBuilder
    {
        private readonly IMutableSequence _sequence;

        public SequenceBuilder([NotNull] IMutableSequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            _sequence = sequence;
        }

        public virtual IMutableSequence Metadata => _sequence;

        public virtual SequenceBuilder IncrementsBy(int increment)
        {
            _sequence.IncrementBy = increment;

            return this;
        }

        public virtual SequenceBuilder StartsAt(long startValue)
        {
            _sequence.StartValue = startValue;

            return this;
        }

        public virtual SequenceBuilder HasMax(long maximum)
        {
            _sequence.MaxValue = maximum;

            return this;
        }

        public virtual SequenceBuilder HasMin(long minimum)
        {
            _sequence.MinValue = minimum;

            return this;
        }

        public virtual SequenceBuilder IsCyclic(bool cyclic = true)
        {
            _sequence.IsCyclic = cyclic;

            return this;
        }
    }
}
