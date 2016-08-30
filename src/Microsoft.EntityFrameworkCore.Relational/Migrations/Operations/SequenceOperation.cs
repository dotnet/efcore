// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class SequenceOperation : MigrationOperation
    {
        public virtual int IncrementBy { get; set; } = 1;
        public virtual long? MaxValue { get; [param: CanBeNull] set; }
        public virtual long? MinValue { get; [param: CanBeNull] set; }
        public virtual bool IsCyclic { get; set; }
    }
}
