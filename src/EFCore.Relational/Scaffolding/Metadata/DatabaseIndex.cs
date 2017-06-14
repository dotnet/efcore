// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class DatabaseIndex : Annotatable
    {
        public virtual DatabaseTable Table { get; [param: CanBeNull] set; }

        public virtual string Name { get; [param: NotNull] set; }
        public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();
        public virtual bool IsUnique { get; [param: NotNull] set; }
        public virtual string Filter { get; [param: CanBeNull] set; }
    }
}
