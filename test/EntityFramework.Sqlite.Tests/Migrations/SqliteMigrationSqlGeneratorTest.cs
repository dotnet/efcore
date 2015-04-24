// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationSqlGenerator SqlGenerator => new SqliteMigrationSqlGenerator(new SqliteSqlGenerator());

        public override void AlterColumnOperation()
        {
            Assert.Throws<NotImplementedException>(() => base.AlterColumnOperation());
        }

        public override void DropIndexOperation()
        {
            base.DropIndexOperation();

            Assert.Equal(
                "DROP INDEX \"IX_People_Name\";" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameTableOperation()
        {
            Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    NewName = "Person"
                });

            Assert.Equal(
                "ALTER TABLE \"People\" RENAME TO \"Person\";" + EOL,
                Sql);
        }
    }
}
