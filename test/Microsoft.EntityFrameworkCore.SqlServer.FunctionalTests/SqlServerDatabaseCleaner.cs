// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class SqlServerDatabaseCleaner : RelationalDatabaseCleaner
    {
        protected override IInternalDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
            => new SqlServerDatabaseModelFactory(loggerFactory);

        protected override bool AcceptIndex(IndexModel index)
            => !index.Name.StartsWith("PK_", StringComparison.Ordinal)
               && !index.Name.StartsWith("AK_", StringComparison.Ordinal);

        protected override DropTableOperation Drop(TableModel table)
            => AddMemoryOptimizedAnnotation(base.Drop(table), table);

        protected override DropForeignKeyOperation Drop(ForeignKeyModel foreignKey)
            => AddMemoryOptimizedAnnotation(base.Drop(foreignKey), foreignKey.Table);

        protected override DropIndexOperation Drop(IndexModel index)
            => AddMemoryOptimizedAnnotation(base.Drop(index), index.Table);

        private static TOperation AddMemoryOptimizedAnnotation<TOperation>(TOperation operation, TableModel table)
            where TOperation : MigrationOperation
        {
            operation[SqlServerFullAnnotationNames.Instance.MemoryOptimized]
                = table[SqlServerFullAnnotationNames.Instance.MemoryOptimized] as bool?;

            return operation;
        }
    }
}
