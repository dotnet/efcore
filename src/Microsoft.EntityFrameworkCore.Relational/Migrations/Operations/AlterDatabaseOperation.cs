// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class AlterDatabaseOperation : MigrationOperation, IAlterMigrationOperation
    {
        public virtual Annotatable OldDatabase { get; } = new Annotatable();
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations => OldDatabase;
    }
}
