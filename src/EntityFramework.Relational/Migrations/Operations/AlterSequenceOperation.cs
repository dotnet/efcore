// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Migrations.Operations
{
    public class AlterSequenceOperation : MigrationOperation
    {
        public virtual bool Cycle { get; set; }
        public virtual int? IncrementBy { get; set; }
        public virtual long? MaxValue { get; [param: CanBeNull] set; }
        public virtual long? MinValue { get; [param: CanBeNull] set; }
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
    }
}
