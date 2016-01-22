// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
    public class SqlServerUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
            => new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerationHelper(), new SqlServerTypeMapper());

        [Fact]
        public void AppendBatchHeader_should_append_SET_NOCOUNT_ON()
        {
            var sb = new StringBuilder();

            CreateSqlGenerator().AppendBatchHeader(sb);

            Assert.Equal("SET NOCOUNT ON;" + Environment.NewLine, sb.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine +
                "SELECT [Computed]" + Environment.NewLine +
                "FROM [dbo].[Ducks]" + Environment.NewLine +
                "WHERE @@ROWCOUNT = 1 AND [Id] = @p0;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT [Id], [Computed]" + Environment.NewLine +
                "FROM [dbo].[Ducks]" + Environment.NewLine +
                "WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks]" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT [Id]" + Environment.NewLine +
                "FROM [dbo].[Ducks]" + Environment.NewLine +
                "WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine +
                "SELECT [Id]" + Environment.NewLine +
                "FROM [dbo].[Ducks]" + Environment.NewLine +
                "WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks]" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT [Id], [Computed]" + Environment.NewLine +
                "FROM [dbo].[Ducks]" + Environment.NewLine +
                "WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "DECLARE @inserted0 TABLE ([Computed] uniqueidentifier);" + Environment.NewLine +
                "UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "INTO @inserted0" + Environment.NewLine +
                "WHERE [Id] = @p3 AND [ConcurrencyToken] = @p4;" + Environment.NewLine +
                "SELECT [Computed] FROM @inserted0;" + Environment.NewLine,
                stringBuilder.ToString());
        }
        
        protected override void AppendUpdateOperation_appends_select_for_computed_property_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "DECLARE @inserted0 TABLE ([Computed] uniqueidentifier);" + Environment.NewLine +
                "UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "INTO @inserted0" + Environment.NewLine +
                "WHERE [Id] = @p3;" + Environment.NewLine +
                "SELECT [Computed] FROM @inserted0;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            Assert.Equal(
                "DECLARE @toInsert0 TABLE ([Name] nvarchar(max), [Quacks] int, [ConcurrencyToken] varbinary(max), [_Position] [int]);" + Environment.NewLine +
                "INSERT INTO @toInsert0" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, 0)," + Environment.NewLine +
                "(@p0, @p1, @p2, 1);" + Environment.NewLine +
                "DECLARE @inserted0 TABLE ([Id] int, [Name] nvarchar(max), [Quacks] int, [Computed] uniqueidentifier, [ConcurrencyToken] varbinary(max));" + Environment.NewLine +
                "INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Id], INSERTED.[Name], INSERTED.[Quacks], INSERTED.[Computed], INSERTED.[ConcurrencyToken]" + Environment.NewLine +
                "INTO @inserted0" + Environment.NewLine +
                "SELECT [Name], [Quacks], [ConcurrencyToken] FROM @toInsert0;" + Environment.NewLine +
                "SELECT [Id], [Computed]" + Environment.NewLine +
                "FROM @inserted0 [_t]" + Environment.NewLine +
                "INNER JOIN @toInsert0 [_j] ON ([_t].[Name] = [_j].[Name] OR ([_t].[Name] is NULL AND [_j].[Name] is NULL)) AND ([_t].[Quacks] = [_j].[Quacks]) AND ([_t].[ConcurrencyToken] = [_j].[ConcurrencyToken] OR ([_t].[ConcurrencyToken] is NULL AND [_j].[ConcurrencyToken] is NULL))" + Environment.NewLine +
                "ORDER BY [_Position];" + Environment.NewLine,
                stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NotLastInResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: false, isComputed: false);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3)," + Environment.NewLine +
                "(@p0, @p1, @p2, @p3);" + Environment.NewLine,
                stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NoResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist_default_values_only()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true, defaultsOnly: true);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            var expectedText =
                "DECLARE @inserted0 TABLE ([Id] int, [Computed] uniqueidentifier);" + Environment.NewLine +
                "INSERT INTO [dbo].[Ducks] ([Id])" + Environment.NewLine +
                "OUTPUT INSERTED.[Id], INSERTED.[Computed]" + Environment.NewLine +
                "INTO @inserted0" + Environment.NewLine +
                "VALUES (DEFAULT)," + Environment.NewLine +
                "(DEFAULT);" + Environment.NewLine +
                "SELECT [Id], [Computed] FROM @inserted0;" + Environment.NewLine;
            Assert.Equal(expectedText, stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NotLastInResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist_default_values_only()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: false, isComputed: false, defaultsOnly: true);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            var expectedText = "INSERT INTO [dbo].[Ducks]" + Environment.NewLine +
                               "DEFAULT VALUES;" + Environment.NewLine;
            Assert.Equal(expectedText + expectedText,
                stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NoResultSet, grouping);
        }

        protected override string RowsAffected => "@@ROWCOUNT";

        protected override string Identity
        {
            get { throw new NotImplementedException(); }
        }

        protected override string OpenDelimeter => "[";

        protected override string CloseDelimeter => "]";
    }
}
