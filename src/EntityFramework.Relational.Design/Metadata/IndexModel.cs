// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class IndexModel : Annotatable
    {
        [CanBeNull]
        public virtual TableModel Table { get; [param: CanBeNull] set; }

        public virtual string Name { get; [param: NotNull] set; }
        public virtual ICollection<IndexColumnModel> IndexColumns { get; [param: NotNull] set; } = new List<IndexColumnModel>();
        public virtual bool IsUnique { get; [param: NotNull] set; }
    }
}
