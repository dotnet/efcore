// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsSqliteTest : MigrationsTestBase<MigrationsSqliteTest.MigrationsSqliteFixture>
    {
        public MigrationsSqliteTest(MigrationsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Create_table()
        {
            await base.Create_table();

            AssertSql(
                @"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_People"" PRIMARY KEY AUTOINCREMENT,
    ""Name"" TEXT NULL
);");
        }

        // SQLite does not support schemas, check constraints, etc.
        public override Task Create_table_all_settings() => Task.CompletedTask;

        public override async Task Create_table_with_comments()
        {
            await Test(
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name").HasComment("Column comment");
                        e.HasComment("Table comment");
                    }),
                model =>
                {
                    // Reverse-engineering of comments isn't supported in Sqlite
                    var table = Assert.Single(model.Tables);
                    Assert.Null(table.Comment);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Null(column.Comment);
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- Table comment

    ""Id"" INTEGER NOT NULL,

    -- Column comment
    ""Name"" TEXT NULL
);");
        }

        [ConditionalFact]
        public override async Task Create_table_with_multiline_comments()
        {
            var tableComment = @"This is a multi-line
table comment.
More information can
be found in the docs.";
            var columnComment = @"This is a multi-line
column comment.
More information can
be found in the docs.";

            await Test(
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name").HasComment(columnComment);
                        e.HasComment(tableComment);
                    }),
                model =>
                {
                    // Reverse-engineering of comments isn't supported in Sqlite
                    var table = Assert.Single(model.Tables);
                    Assert.Null(table.Comment);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Null(column.Comment);
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- This is a multi-line
    -- table comment.
    -- More information can
    -- be found in the docs.

    ""Id"" INTEGER NOT NULL,

    -- This is a multi-line
    -- column comment.
    -- More information can
    -- be found in the docs.
    ""Name"" TEXT NULL
);");
        }

        // In Sqlite, comments are only generated when creating a table
        public override async Task Alter_table_add_comment()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").HasComment("Table comment").Property<int>("Id"),
                model => Assert.Null(Assert.Single(model.Tables).Comment));

            AssertSql();
        }

        // In Sqlite, comments are only generated when creating a table
        public override async Task Alter_table_add_comment_non_default_schema()
        {
            await Test(
                builder => builder.Entity("People")
                    .ToTable("People", "SomeOtherSchema")
                    .Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People")
                    .ToTable("People", "SomeOtherSchema")
                    .HasComment("Table comment"),
                model => Assert.Null(Assert.Single(model.Tables).Comment));
        }

        // In Sqlite, comments are only generated when creating a table
        public override async Task Alter_table_change_comment()
        {
            await Test(
                builder => builder.Entity("People").HasComment("Table comment1").Property<int>("Id"),
                builder => builder.Entity("People").HasComment("Table comment2").Property<int>("Id"),
                model => Assert.Null(Assert.Single(model.Tables).Comment));

            AssertSql();
        }

        // In Sqlite, comments are only generated when creating a table
        public override async Task Alter_table_remove_comment()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").HasComment("Table comment1"),
                builder => builder.Entity("People").HasComment("Table comment2"),
                model => Assert.Null(Assert.Single(model.Tables).Comment));

            AssertSql();
        }

        public override async Task Rename_table()
        {
            var ex = await Assert.ThrowsAsync<SqliteException>(base.Rename_table);
            Assert.Contains("there is already another table or index with this name", ex.Message);
        }

        public override Task Rename_table_with_primary_key()
            => AssertNotSupportedAsync(
                base.Rename_table_with_primary_key, SqliteStrings.InvalidMigrationOperation("DropPrimaryKeyOperation"));

        // SQLite does not support schemas.
        public override Task Move_table()
            => Test(
                builder => builder.Entity("TestTable").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("TestTable").ToTable("TestTable", "TestTableSchema"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Null(table.Schema);
                    Assert.Equal("TestTable", table.Name);
                });

        // SQLite does not support schemas
        public override Task Create_schema()
            => Test(
                builder => { },
                builder => builder.Entity("People")
                    .ToTable("People", "SomeOtherSchema")
                    .Property<int>("Id"),
                model => Assert.Null(Assert.Single(model.Tables).Schema));

        public override async Task Add_column_with_defaultValue_datetime()
        {
            await base.Add_column_with_defaultValue_datetime();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Birthday"" TEXT NOT NULL DEFAULT '2015-04-12 17:05:00';");
        }

        public override async Task Add_column_with_defaultValueSql()
        {
            var ex = await Assert.ThrowsAsync<SqliteException>(base.Add_column_with_defaultValueSql);
            Assert.Contains("Cannot add a column with non-constant default", ex.Message);
        }

        public override Task Add_column_with_computedSql()
            => AssertNotSupportedAsync(base.Add_column_with_computedSql, SqliteStrings.ComputedColumnsNotSupported);

        public override async Task Add_column_with_max_length()
        {
            await base.Add_column_with_max_length();

            // See issue #3698
            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Name"" TEXT NULL;");
        }

        // In Sqlite, comments are only generated when creating a table
        public override async Task Add_column_with_comment()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("FullName").HasComment("My comment"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "FullName");
                    Assert.Null(column.Comment);
                });

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""FullName"" TEXT NULL;");
        }

        public override Task Alter_column_make_required()
            => AssertNotSupportedAsync(base.Alter_column_make_required, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Alter_column_make_required_with_index()
            => AssertNotSupportedAsync(
                base.Alter_column_make_required_with_index, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Alter_column_make_required_with_composite_index()
            => AssertNotSupportedAsync(
                base.Alter_column_make_required_with_composite_index, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Alter_column_make_computed()
            => AssertNotSupportedAsync(base.Alter_column_make_computed, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Alter_column_change_computed()
            => AssertNotSupportedAsync(base.Alter_column_change_computed, SqliteStrings.ComputedColumnsNotSupported);

        public override Task Alter_column_add_comment()
            => AssertNotSupportedAsync(base.Alter_column_add_comment, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Alter_column_change_comment()
            => AssertNotSupportedAsync(base.Alter_column_change_comment, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Alter_column_remove_comment()
            => AssertNotSupportedAsync(base.Alter_column_remove_comment, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Drop_column()
            => AssertNotSupportedAsync(base.Drop_column, SqliteStrings.InvalidMigrationOperation("DropColumnOperation"));

        public override Task Drop_column_primary_key()
            => AssertNotSupportedAsync(base.Drop_column_primary_key, SqliteStrings.InvalidMigrationOperation("DropPrimaryKeyOperation"));

        public override async Task Rename_column()
        {
            await base.Rename_column();

            AssertSql(
                @"ALTER TABLE ""People"" RENAME COLUMN ""SomeColumn"" TO ""somecolumn"";");
        }

        public override Task Create_index_with_filter()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                    }),
                builder => { },
                builder => builder.Entity("People").HasIndex("Name").HasFilter($"{DelimitIdentifier("Name")} IS NOT NULL"),
                // Reverse engineering of index filters isn't supported in SQLite
                model => Assert.Null(model.Tables.Single().Indexes.Single().Filter));

        public override Task Create_unique_index_with_filter()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                    }),
                builder => { },
                builder => builder.Entity("People").HasIndex("Name").IsUnique()
                    .HasFilter($"{DelimitIdentifier("Name")} IS NOT NULL AND {DelimitIdentifier("Name")} <> ''"),
                // Reverse engineering of index filters isn't supported in SQLite
                model => Assert.Null(model.Tables.Single().Indexes.Single().Filter));

        public override async Task Rename_index()
        {
            await base.Rename_index();

            AssertSql(
                @"DROP INDEX ""Foo"";
CREATE INDEX ""foo"" ON ""People"" (""FirstName"");");
        }

        public override Task Add_primary_key()
            => AssertNotSupportedAsync(base.Add_primary_key, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Add_primary_key_with_name()
            => AssertNotSupportedAsync(base.Add_primary_key_with_name, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Add_primary_key_composite_with_name()
            => AssertNotSupportedAsync(
                base.Add_primary_key_composite_with_name, SqliteStrings.InvalidMigrationOperation("AlterColumnOperation"));

        public override Task Drop_primary_key()
            => AssertNotSupportedAsync(base.Drop_primary_key, SqliteStrings.InvalidMigrationOperation("DropPrimaryKeyOperation"));

        public override Task Add_foreign_key()
            => AssertNotSupportedAsync(base.Add_foreign_key, SqliteStrings.InvalidMigrationOperation("AddForeignKeyOperation"));

        public override Task Add_foreign_key_with_name()
            => AssertNotSupportedAsync(base.Add_foreign_key_with_name, SqliteStrings.InvalidMigrationOperation("AddForeignKeyOperation"));

        public override Task Drop_foreign_key()
            => AssertNotSupportedAsync(base.Drop_foreign_key, SqliteStrings.InvalidMigrationOperation("DropForeignKeyOperation"));

        public override Task Add_unique_constraint()
            => AssertNotSupportedAsync(base.Add_unique_constraint, SqliteStrings.InvalidMigrationOperation("AddUniqueConstraintOperation"));

        public override Task Add_unique_constraint_composite_with_name()
            => AssertNotSupportedAsync(
                base.Add_unique_constraint_composite_with_name, SqliteStrings.InvalidMigrationOperation("AddUniqueConstraintOperation"));

        public override Task Drop_unique_constraint()
            => AssertNotSupportedAsync(
                base.Drop_unique_constraint, SqliteStrings.InvalidMigrationOperation("DropUniqueConstraintOperation"));

        public override Task Add_check_constraint_with_name()
            => AssertNotSupportedAsync(
                base.Add_check_constraint_with_name, SqliteStrings.InvalidMigrationOperation("CreateCheckConstraintOperation"));

        public override Task Drop_check_constraint()
            => AssertNotSupportedAsync(base.Drop_check_constraint, SqliteStrings.InvalidMigrationOperation("DropCheckConstraintOperation"));

        public override Task Create_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Create_sequence_all_settings()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Alter_sequence_all_settings()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Alter_sequence_increment_by()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Drop_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Rename_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Move_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        protected virtual async Task AssertNotSupportedAsync(Func<Task> action, string? message = null)
        {
            var ex = await Assert.ThrowsAsync<NotSupportedException>(action);
            if (message != null)
            {
                Assert.Equal(message, ex.Message);
            }
        }

        public class MigrationsSqliteFixture : MigrationsFixtureBase
        {
            protected override string StoreName { get; } = nameof(MigrationsSqliteTest);
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public override TestHelpers TestHelpers => SqliteTestHelpers.Instance;

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, SqliteDatabaseModelFactory>();
        }
    }
}
