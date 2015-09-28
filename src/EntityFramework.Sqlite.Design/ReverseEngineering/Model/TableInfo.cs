// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering.Model
{
    public class TableInfo
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string SchemaName { get; [param: NotNull] set; }
        public virtual string CreateStatement { get; [param: NotNull] set; }
    }
}
