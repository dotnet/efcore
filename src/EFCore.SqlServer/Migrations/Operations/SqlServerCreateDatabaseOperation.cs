// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class SqlServerCreateDatabaseOperation : MigrationOperation
    {
        public virtual string Name { get; [param: NotNull] set; }

        public virtual string FileName { get; [param: CanBeNull] set; }
    }
}
