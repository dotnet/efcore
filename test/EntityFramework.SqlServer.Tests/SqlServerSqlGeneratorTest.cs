// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSqlGeneratorTest : SqlGeneratorTestBase
    {
        protected override SqlGenerator CreateSqlGenerator()
        {
            return new SqlServerSqlGenerator();
        }

        [Fact]
        public void AppendBatchHeader_should_append_SET_NOCOUNT_OFF()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator().AppendBatchHeader(sb);

            Assert.Equal("SET NOCOUNT OFF;" + Environment.NewLine, sb.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [Ducks] ([Id], [Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "VALUES (@p1, @p2, @p3, @p5);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Id], INSERTED.[Computed]" + Environment.NewLine +
                "VALUES (@p2, @p3, @p5);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [Ducks]" + Environment.NewLine +
                "OUTPUT INSERTED.[Id]" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [Ducks] ([Name], [Quacks], [ConcurrencyToken])" + Environment.NewLine +
                "OUTPUT INSERTED.[Id]" + Environment.NewLine +
                "VALUES (@p2, @p3, @p5);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO [Ducks]" + Environment.NewLine +
                "OUTPUT INSERTED.[Id], INSERTED.[Computed]" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE [Ducks] SET [Name] = @p2, [Quacks] = @p3, [ConcurrencyToken] = @p5" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "WHERE [Id] = @o1 AND [ConcurrencyToken] = @o5;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendUpdateOperation_appends_select_for_computed_property_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE [Ducks] SET [Name] = @p2, [Quacks] = @p3, [ConcurrencyToken] = @p5" + Environment.NewLine +
                "OUTPUT INSERTED.[Computed]" + Environment.NewLine +
                "WHERE [Id] = @o1;" + Environment.NewLine,
                stringBuilder.ToString());
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
