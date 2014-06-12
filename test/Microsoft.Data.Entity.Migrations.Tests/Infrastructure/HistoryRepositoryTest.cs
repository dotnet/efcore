// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Infrastructure
{
    public class HistoryRepositoryTest
    {
        [Fact]
        public void Get_table_name()
        {
            using (var context = new Context())
            {
                var historyRepository = new HistoryRepository(context.Configuration);

                Assert.Equal("__MigrationHistory", historyRepository.TableName);
            }
        }

        [Fact]
        public void Create_and_cache_history_model()
        {
            using (var context = new Context())
            {
                var historyRepository = new HistoryRepository(context.Configuration);

                var historyModel1 = historyRepository.HistoryModel;
                var historyModel2 = historyRepository.HistoryModel;

                Assert.Same(historyModel1, historyModel2);
                Assert.Equal(1, historyModel1.EntityTypes.Count);

                var entityType = historyModel1.EntityTypes[0];
                Assert.Equal("HistoryRow", entityType.Name);
                Assert.Equal(3, entityType.Properties.Count);
                Assert.Equal(new[] { "ContextKey", "MigrationName", "Timestamp" }, entityType.Properties.Select(p => p.Name));
            }
        }

        [Fact]
        public void Create_history_context_from_user_context()
        {
            using (var context = new Context())
            {
                var historyRepository = new HistoryRepository(context.Configuration);

                using (var historyContext = historyRepository.CreateHistoryContext())
                {
                    Assert.Same(historyRepository.HistoryModel, historyContext.Model);

                    var extensions = context.Configuration.ContextOptions.Extensions;
                    var historyExtensions = historyContext.Configuration.ContextOptions.Extensions;

                    Assert.Equal(extensions.Count, historyExtensions.Count);

                    for (var i = 0; i < extensions.Count; i++)
                    {
                        Assert.Same(extensions[i], historyExtensions[i]);
                    }
                }
            }
        }

        [Fact]
        public void Get_migrations_query()
        {
            using (var context = new Context())
            {
                var historyRepository = new HistoryRepository(context.Configuration);

                using (var historyContext = historyRepository.CreateHistoryContext())
                {
                    var query = historyRepository.GetMigrationsQuery(historyContext);

                    var expression = (MethodCallExpression)query.Expression;

                    Assert.Equal("OrderBy", expression.Method.Name);
                    Assert.Equal("m => m", expression.Arguments[1].ToString());
                    Assert.Equal("value(Microsoft.Data.Entity.Migrations.Infrastructure.MigrationMetadataComparer)", expression.Arguments[2].ToString());

                    expression = (MethodCallExpression)expression.Arguments[0];

                    Assert.Equal("Select", expression.Method.Name);
                    Assert.Equal("h => new MigrationMetadata(h.MigrationName, h.Timestamp)", expression.Arguments[1].ToString());

                    expression = (MethodCallExpression)expression.Arguments[0];

                    Assert.Equal("Where", expression.Method.Name);
                    Assert.Equal(
                        "h => (h.ContextKey == value(Microsoft.Data.Entity.Migrations.Infrastructure.HistoryRepository).GetContextKey())",
                        expression.Arguments[1].ToString());

                    var queryableType = expression.Arguments[0].Type;

                    Assert.True(queryableType.IsGenericType);
                    Assert.Equal("EntityQueryable", queryableType.Name.Remove(queryableType.Name.IndexOf("`", StringComparison.Ordinal)));
                    Assert.Equal(1, queryableType.GenericTypeArguments.Length);
                    Assert.Equal("HistoryRow", queryableType.GenericTypeArguments[0].Name);
                }
            }
        }

        [Fact]
        public void Get_migrations()
        {
            using (var context = new Context())
            {
                var historyRepositoryMock = new Mock<HistoryRepository>(context.Configuration) { CallBase = true };

                historyRepositoryMock
                    .Setup(o => o.GetMigrationsQuery(It.IsAny<DbContext>()))
                    .Returns(() =>
                        new IMigrationMetadata[]
                            {
                                new MigrationMetadata("Migration1", "Timestamp1"),
                                new MigrationMetadata("Migration2", "Timestamp2")
                            }
                            .AsQueryable());

                var migrations = historyRepositoryMock.Object.Migrations;
                Assert.Equal(2, migrations.Count);
                Assert.Equal("Migration1", migrations[0].Name);
                Assert.Equal("Migration2", migrations[1].Name);
            }
        }

        [Fact]
        public void Generate_insert_migration_sql()
        {
            using (var context = new Context())
            {
                var historyRepository = new HistoryRepository(context.Configuration);

                var sqlStatements = historyRepository.GenerateInsertMigrationSql(
                    new MigrationMetadata("Foo", "Bar"), new DmlSqlGenerator());

                Assert.Equal(1, sqlStatements.Count);
                Assert.Equal(
                    @"INSERT INTO ""__MigrationHistory"" (""MigrationName"", ""Timestamp"", ""ContextKey"") VALUES ('Foo', 'Bar', 'Microsoft.Data.Entity.Migrations.Tests.Infrastructure.HistoryRepositoryTest+Context')",
                    sqlStatements[0].Sql);
            }
        }

        [Fact]
        public void Generate_delete_migration_sql()
        {
            using (var context = new Context())
            {
                var historyRepository = new HistoryRepository(context.Configuration);

                var sqlStatements = historyRepository.GenerateDeleteMigrationSql(
                    new MigrationMetadata("Foo", "Bar"), new DmlSqlGenerator());

                Assert.Equal(1, sqlStatements.Count);
                Assert.Equal(
                    @"DELETE FROM ""__MigrationHistory"" WHERE ""MigrationName"" = 'Foo'",
                    sqlStatements[0].Sql);
            }
        }

        #region Fixture

        public class Context : DbContext
        {
        }

        public class DmlSqlGenerator : SqlGenerator
        {
            protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
