// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering.Model
{
    public class IndexInfo
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string SchemaName { get; [param: NotNull] set; }
        public virtual string TableName { get; [param: NotNull] set; }
        public virtual List<string> Columns { get; [param: NotNull] set; } = new List<string>();
        public virtual bool IsUnique { get; [param: NotNull] set; }
        public virtual string CreateStatement { get; [param: NotNull] set; }
    }
}
