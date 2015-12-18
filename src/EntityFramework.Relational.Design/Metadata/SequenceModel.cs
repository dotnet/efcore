// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class SequenceModel : Annotatable
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string SchemaName { get; [param: CanBeNull] set; }
        public virtual string DataType { get; [param: CanBeNull] set; }
        public virtual long? Start { get; [param: CanBeNull] set; }
        public virtual int? IncrementBy { get; [param: CanBeNull] set; }
        public virtual long? Min { get; [param: CanBeNull] set; }
        public virtual long? Max { get; [param: CanBeNull] set; }
        public virtual bool? IsCyclic { get; [param: CanBeNull] set; }
    }
}
