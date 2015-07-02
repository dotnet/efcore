// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Sqlite.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public abstract class SqliteOperationTransformBase
    {
        protected abstract SqliteOperationTransformer CreateTransformer();

        protected Action<MigrationOperation> AssertCreateIndex(
            string tableName,
            string[] columns,
            bool unique = false)
            => operation =>
                {
                    var index = Assert.IsType<CreateIndexOperation>(operation);
                    Assert.Equal(tableName, index.Table);
                    Assert.Equal(unique, index.IsUnique);
                    Assert.Equal(index.Columns, columns);
                };

        protected static Action<MigrationOperation> AssertCreateTable(
            string tableName,
            string[] columns,
            string[] primaryKey = null)
            => operation =>
                {
                    var create = Assert.IsType<CreateTableOperation>(operation);
                    Assert.Equal(tableName, create.Name);
                    Assert.Equal(columns, create.Columns.Select(c => c.Name).ToArray());
                    if (primaryKey != null)
                    {
                        Assert.Equal(primaryKey, create.PrimaryKey.Columns);
                    }
                };

        protected static Action<MigrationOperation> AssertDropTemp(string name)
            => operation =>
                {
                    var drop = Assert.IsType<DropTableOperation>(operation);
                    Assert.Equal(name + "_temp", drop.Name);
                };

        protected static Action<MigrationOperation> AssertMoveData(
            string tableName,
            string[] oldColumns,
            string[] newColumns)
            => op =>
                {
                    var move = Assert.IsType<MoveDataOperation>(op);
                    Assert.Equal(tableName + "_temp", move.OldTable);
                    Assert.Equal(tableName, move.NewTable);
                    Assert.Equal(oldColumns, move.ColumnMapping.Values.ToArray());
                    Assert.Equal(newColumns, move.ColumnMapping.Keys.ToArray());
                };

        protected static Action<MigrationOperation> AssertRenameTemp(string name)
            => operation =>
                {
                    var rename = Assert.IsType<RenameTableOperation>(operation);
                    Assert.Equal(name, rename.Name);
                    Assert.Equal(name + "_temp", rename.NewName);
                };

        protected IReadOnlyList<MigrationOperation> Transform(Action<MigrationBuilder> migrationBuilder, Action<ModelBuilder> modelBuilder)
        {
            var migration = new MigrationBuilder();
            migrationBuilder(migration);
            var model = new ModelBuilder(new ConventionSet(), new Model());
            modelBuilder(model);
            var transformer = CreateTransformer();
            return transformer.Transform(migration.Operations.ToList(), model.Model);
        }
    }
}
