// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Sqlite.Migrations.Operations
{
    public class MoveDataOperation : MigrationOperation
    {
        /// <summary>
        ///     Key = destination column name, value = source column name
        /// </summary>
        public virtual IDictionary<string, string> ColumnMapping { get; [param: NotNull] set; } = new Dictionary<string, string>();

        public virtual string OldTable { get; [param: NotNull] set; }
        public virtual string NewTable { get; [param: NotNull] set; }
    }
}
