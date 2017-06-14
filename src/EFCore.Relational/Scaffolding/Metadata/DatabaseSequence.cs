// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class DatabaseSequence : Annotatable
    {
        public virtual DatabaseModel Database { get; [param: NotNull] set; }

        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }
        public virtual string StoreType { get; [param: CanBeNull] set; }
        public virtual long? StartValue { get; [param: CanBeNull] set; }
        public virtual int? IncrementBy { get; [param: CanBeNull] set; }
        public virtual long? MinValue { get; [param: CanBeNull] set; }
        public virtual long? MaxValue { get; [param: CanBeNull] set; }
        public virtual bool? IsCyclic { get; [param: CanBeNull] set; }
    }
}
