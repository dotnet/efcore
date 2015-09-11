// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class SqlServerSequenceValueGeneratorState : HiLoValueGeneratorState
    {
        public SqlServerSequenceValueGeneratorState([NotNull] ISequence sequence, int poolSize)
            : base(Check.NotNull(sequence, nameof(sequence)).IncrementBy, poolSize)
        {
            Sequence = sequence;
        }

        public virtual ISequence Sequence{ get; }
    }
}
