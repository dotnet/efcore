// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Model
{
    public class Index
    {
        [CanBeNull]
        public virtual Table Table { get; [param: CanBeNull] set; }

        public virtual string Name { get; [param: NotNull] set; }
        public virtual IList<Column> Columns { get; [param: NotNull] set; } = new List<Column>();
        public virtual bool IsUnique { get; [param: NotNull] set; }
    }
}
