// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class SqlGeneratorTest
    {
        [Fact]
        public void AppendDeleteOperation_creates_full_delete_command_text()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateDeleteOperations(concurrencyToken: false);

            new ConcreteSqlGenerator().AppendDeleteOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "DELETE FROM [Ducks] WHERE [Id] = @p1",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateDeleteOperations();

            new ConcreteSqlGenerator().AppendDeleteOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "DELETE FROM [Ducks] WHERE [Id] = @p1 AND [ConcurrencyToken] = @p5",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateInsertOperations();

            new ConcreteSqlGenerator().AppendInsertOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "INSERT INTO [Ducks] ([Name], [Quacks], [ConcurrencyToken]) VALUES (@p2, @p3, @p5);" + Environment.NewLine +
                "SELECT [Id], [Computed] FROM [Ducks] WHERE [Id] = provider_specific_identity()",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_only_insert_if_no_store_generated_columns_exist_or_conditions_exist()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateInsertOperations(identityKey: false, computedProperty: false);

            new ConcreteSqlGenerator().AppendInsertOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "INSERT INTO [Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken]) VALUES (@p1, @p2, @p3, @p5)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateInsertOperations(identityKey: false, computedProperty: true);

            new ConcreteSqlGenerator().AppendInsertOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "INSERT INTO [Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken]) VALUES (@p1, @p2, @p3, @p5);" + Environment.NewLine +
                "SELECT [Computed] FROM [Ducks] WHERE [Id] = @p1",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_for_only_identity()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateInsertOperations(identityKey: true, computedProperty: false);

            new ConcreteSqlGenerator().AppendInsertOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "INSERT INTO [Ducks] ([Name], [Quacks], [ConcurrencyToken]) VALUES (@p2, @p3, @p5);" + Environment.NewLine +
                "SELECT [Id] FROM [Ducks] WHERE [Id] = provider_specific_identity()",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateInsertOperations().Where(p => !p.IsWrite).ToArray();

            new ConcreteSqlGenerator().AppendInsertOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "INSERT INTO [Ducks] DEFAULT VALUES;" + Environment.NewLine +
                "SELECT [Id], [Computed] FROM [Ducks] WHERE [Id] = provider_specific_identity()",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_coluns()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateInsertOperations(computedProperty: false).Where(p => !p.IsWrite).ToArray();

            new ConcreteSqlGenerator().AppendInsertOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "INSERT INTO [Ducks] DEFAULT VALUES;" + Environment.NewLine +
                "SELECT [Id] FROM [Ducks] WHERE [Id] = provider_specific_identity()",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateUpdateOperations();

            new ConcreteSqlGenerator().AppendUpdateOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "UPDATE [Ducks] SET [Name] = @p2, [Quacks] = @p3, [ConcurrencyToken] = @p5 WHERE [Id] = @p1 AND [ConcurrencyToken] = @p5;" + Environment.NewLine +
                "SELECT [Computed] FROM [Ducks] WHERE [Id] = @p1",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_does_not_append_select_if_store_generated_columns_dont_exist()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateUpdateOperations(computedProperty: false, concurrencyToken: false);

            new ConcreteSqlGenerator().AppendUpdateOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "UPDATE [Ducks] SET [Name] = @p2, [Quacks] = @p3, [ConcurrencyToken] = @p5 WHERE [Id] = @p1",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_select_for_concurrency_token()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateUpdateOperations(computedProperty: false, concurrencyToken: true);

            new ConcreteSqlGenerator().AppendUpdateOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "UPDATE [Ducks] SET [Name] = @p2, [Quacks] = @p3, [ConcurrencyToken] = @p5 WHERE [Id] = @p1 AND [ConcurrencyToken] = @p5",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateOperation_appends_select_for_computed_property()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateUpdateOperations(computedProperty: true, concurrencyToken: false);

            new ConcreteSqlGenerator().AppendUpdateOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "UPDATE [Ducks] SET [Name] = @p2, [Quacks] = @p3, [ConcurrencyToken] = @p5 WHERE [Id] = @p1;" + Environment.NewLine +
                "SELECT [Computed] FROM [Ducks] WHERE [Id] = @p1",
                stringBuilder.ToString());
        }

        [Fact]
        public void Default_BatchCommandSeparator_is_semicolon()
        {
            Assert.Equal(";", new ConcreteSqlGenerator().BatchCommandSeparator);
        }

        private class ConcreteSqlGenerator : SqlGenerator
        {
            protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            {
                commandStringBuilder
                    .Append(QuoteIdentifier(columnModification.ColumnName))
                    .Append(" = ")
                    .Append("provider_specific_identity()");
            }
        }

        private ColumnModification[] CreateInsertOperations(bool identityKey = true, bool computedProperty = true)
        {
            using (var context = new DuckDuckGooseContext())
            {
                var entry = context.ChangeTracker.Entry(context.Add(new Duck())).StateEntry;

                return new[]
                    {
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Id"), "@p1",
                            isRead: identityKey, isWrite: !identityKey, isKey: true, isCondition: true),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Name"), "@p2",
                            isRead: false, isWrite: true, isKey: false, isCondition: false),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Quacks"), "@p3",
                            isRead: false, isWrite: true, isKey: false, isCondition: false),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Computed"), "@p4",
                            isRead: computedProperty, isWrite: false, isKey: false, isCondition: false),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("ConcurrencyToken"), "@p5",
                            isRead: false, isWrite: true, isKey: false, isCondition: false)
                    };
            }
        }

        private ColumnModification[] CreateUpdateOperations(bool computedProperty = true, bool concurrencyToken = true)
        {
            using (var context = new DuckDuckGooseContext())
            {
                var entry = context.ChangeTracker.Entry(context.Add(new Duck())).StateEntry;

                return new[]
                    {
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Id"), "@p1",
                            isRead: false, isWrite: false, isKey: true, isCondition: true),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Name"), "@p2",
                            isRead: false, isWrite: true, isKey: false, isCondition: false),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Quacks"), "@p3",
                            isRead: false, isWrite: true, isKey: false, isCondition: false),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Computed"), "@p4",
                            isRead: computedProperty, isWrite: false, isKey: false, isCondition: false),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("ConcurrencyToken"), "@p5",
                            isRead: false, isWrite: true, isKey: false, isCondition: concurrencyToken)
                    };
            }
        }

        private ColumnModification[] CreateDeleteOperations(bool concurrencyToken = true)
        {
            using (var context = new DuckDuckGooseContext())
            {
                var entry = context.ChangeTracker.Entry(context.Add(new Duck())).StateEntry;

                return new[]
                    {
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Id"), "@p1",
                            isRead: false, isWrite: false, isKey: true, isCondition: true),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("ConcurrencyToken"), "@p5",
                            isRead: false, isWrite: false, isKey: false, isCondition: concurrencyToken)
                    };
            }
        }

        private class DuckDuckGooseContext : DbContext
        {
            public DuckDuckGooseContext()
                : base(new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection
                    .BuildServiceProvider())
            {
            }

            public DbSet<Duck> Blogs { get; set; }
        }

        private class Duck
        {
            private int Id { get; set; }
            private int Computed { get; set; }
            private string Name { get; set; }
            private bool Quacks { get; set; }
            private int ConcurrencyToken { get; set; }
        }
    }
}
