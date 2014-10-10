// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public interface ISqlServerPropertyExtensions : IRelationalPropertyExtensions
    {
        [CanBeNull]
        SqlServerValueGenerationStrategy? ValueGenerationStrategy { get; }

        [CanBeNull]
        string SequenceName { get; }

        [CanBeNull]
        string SequenceSchema { get; }

        [CanBeNull]
        Sequence TryGetSequence();
    }
}
