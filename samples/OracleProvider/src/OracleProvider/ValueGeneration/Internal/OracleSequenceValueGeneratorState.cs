// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Oracle.ValueGeneration.Internal
{
    public class OracleSequenceValueGeneratorState : HiLoValueGeneratorState
    {
        public OracleSequenceValueGeneratorState([NotNull] ISequence sequence)
            : base(Check.NotNull(sequence, nameof(sequence)).IncrementBy)
        {
            Sequence = sequence;
        }

        public virtual ISequence Sequence { get; }
    }
}
