// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSqlGeneratorTest : SqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
        {
            return new SqlServerUpdateSqlGenerator();
        }

        [Fact]
        public void AppendBatchHeader_should_append_SET_NOCOUNT_OFF()
        {
            var sb = new StringBuilder();

            new SqlServerUpdateSqlGenerator().AppendBatchHeader(sb);

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

        [Fact]
        public override void BatchSeparator_returns_seperator()
        {
            Assert.Equal("GO", CreateSqlGenerator().BatchSeparator);
        }

        [Fact]
        public override void GenerateLiteral_returns_ByteArray_literal()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("0xDA7A", literal);
        }

        [Fact]
        public override void GenerateLiteral_returns_bool_literal_when_true()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(true);
            Assert.Equal("1", literal);
        }

        [Fact]
        public override void GenerateLiteral_returns_bool_literal_when_false()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(false);
            Assert.Equal("0", literal);
        }

        [Fact]
        public override void GenerateLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.3710000'", literal);
        }

        [Fact]
        public override void GenerateLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.3710000-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("c6f43a9e-91e1-45ef-a320-832ea23b7292", literal);
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
