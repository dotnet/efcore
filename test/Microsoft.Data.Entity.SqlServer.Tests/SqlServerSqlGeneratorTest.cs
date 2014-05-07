// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSqlGeneratorTest
    {
        [Fact]
        public void AppendBatchHeader_should_append_SET_NOCOUNT_OFF()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator().AppendBatchHeader(sb);

            Assert.Equal("SET NOCOUNT OFF", sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_Select_for_insert_operation_with_identity_key()
        {
            var stringBuilder = new StringBuilder();
            var operations = CreateInsertOperations();

            new SqlServerSqlGenerator().AppendInsertOperation(stringBuilder, "Ducks", operations);

            Assert.Equal(
                "INSERT INTO [Ducks] ([Name], [Quacks]) VALUES (@p2, @p3);" + Environment.NewLine +
                "SELECT [Id] FROM [Ducks] WHERE [Id] = scope_identity()",
                stringBuilder.ToString());
        }

        private ColumnModification[] CreateInsertOperations()
        {
            using (var context = new DuckDuckGooseContext())
            {
                var entry = context.ChangeTracker.Entry(context.Add(new Duck())).StateEntry;

                return new[]
                    {
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Id"), "@p1",
                            isRead: true, isWrite: false, isKey: true, isCondition: true),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Name"), "@p2",
                            isRead: false, isWrite: true, isKey: false, isCondition: false),
                        new ColumnModification(
                            entry, entry.EntityType.GetProperty("Quacks"), "@p3",
                            isRead: false, isWrite: true, isKey: false, isCondition: false)
                    };
            }
        }

        private class DuckDuckGooseContext : DbContext
        {
            public DuckDuckGooseContext()
                : base(new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .BuildServiceProvider())
            {
            }

            public DbSet<Duck> Blogs { get; set; }
        }

        private class Duck
        {
            private int Id { get; set; }
            private string Name { get; set; }
            private bool Quacks { get; set; }
        }
    }
}
