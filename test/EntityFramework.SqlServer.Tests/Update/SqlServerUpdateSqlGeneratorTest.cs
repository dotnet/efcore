// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Update.Internal;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
            => new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerator());

        [Fact]
        public void AppendBatchHeader_should_append_SET_NOCOUNT_OFF()
        {
            var sb = new StringBuilder();

            CreateSqlGenerator().AppendBatchHeader(sb);

            Assert.Equal("SET NOCOUNT OFF;" + Environment.NewLine, sb.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Id], INSERTED.[Computed]" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks]" + Environment.NewLine +
                "OUTPUT INSERTED.[Id]" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Id]" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [dbo].[Ducks]" + Environment.NewLine +
                "OUTPUT INSERTED.[Id], INSERTED.[Computed]" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "WHERE [Id] = @p3 AND [ConcurrencyToken] = @p4;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendUpdateOperation_appends_select_for_computed_property_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE [dbo].[Ducks] SET [Name] = @p0, [Quacks] = @p1, [ConcurrencyToken] = @p2" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "WHERE [Id] = @p3;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command });

            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Id], INSERTED.[Computed]" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2)," + Environment.NewLine +
                "(@p0, @p1, @p2);" + Environment.NewLine,
                stringBuilder.ToString());
            Assert.Equal(SqlServerUpdateSqlGenerator.ResultsGrouping.OneResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: false, isComputed: false);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command });

            Assert.Equal(
                "INSERT INTO [dbo].[Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "VALUES (@p0, @p1, @p2, @p3)," + Environment.NewLine +
                "(@p0, @p1, @p2, @p3);" + Environment.NewLine +
                "SELECT @@ROWCOUNT;" + Environment.NewLine,
                stringBuilder.ToString());
            Assert.Equal(SqlServerUpdateSqlGenerator.ResultsGrouping.OneResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist_default_values_only()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true, defaultsOnly: true);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command });

            var expectedText = "INSERT INTO [dbo].[Ducks]" + Environment.NewLine +
                               "OUTPUT INSERTED.[Id], INSERTED.[Computed]" + Environment.NewLine +
                               "DEFAULT VALUES;" + Environment.NewLine;
            Assert.Equal(expectedText + expectedText,
                stringBuilder.ToString());
            Assert.Equal(SqlServerUpdateSqlGenerator.ResultsGrouping.OneCommandPerResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist_default_values_only()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: false, isComputed: false, defaultsOnly: true);

            var sqlGenerator = (ISqlServerUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command });

            var expectedText = "INSERT INTO [dbo].[Ducks]" + Environment.NewLine +
                               "DEFAULT VALUES;" + Environment.NewLine +
                               "SELECT @@ROWCOUNT;" + Environment.NewLine;
            Assert.Equal(expectedText + expectedText,
                stringBuilder.ToString());
            Assert.Equal(SqlServerUpdateSqlGenerator.ResultsGrouping.OneCommandPerResultSet, grouping);
        }

        protected override string RowsAffected
        {
            get { return "@@ROWCOUNT"; }
        }

        protected override string Identity
        {
            get { throw new NotImplementedException(); }
        }

        protected override string OpenDelimeter
        {
            get { return "["; }
        }

        protected override string CloseDelimeter
        {
            get { return "]"; }
        }
    }
}
