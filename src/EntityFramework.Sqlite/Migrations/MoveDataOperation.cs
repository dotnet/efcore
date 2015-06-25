// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class MoveDataOperation : MigrationOperation
    {
        // TODO handle renaming columns
        public MoveDataOperation()
        {
            IsDestructiveChange = true;
        }

        public virtual string[] Columns { get; [param: NotNull] set; }
        public virtual string OldTable { get; [param: NotNull] set; }
        public virtual string NewTable { get; [param: NotNull] set; }
    }
}
