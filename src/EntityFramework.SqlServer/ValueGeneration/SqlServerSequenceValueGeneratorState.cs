// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer.ValueGeneration
{
    public class SqlServerSequenceValueGeneratorState : HiLoValueGeneratorState
    {
        public SqlServerSequenceValueGeneratorState([NotNull] string sequenceName, int blockSize, int poolSize)
            : base(blockSize, poolSize)
        {
            Check.NotEmpty(sequenceName, nameof(sequenceName));

            SequenceName = sequenceName;
        }

        public virtual string SequenceName { get; }
    }
}
