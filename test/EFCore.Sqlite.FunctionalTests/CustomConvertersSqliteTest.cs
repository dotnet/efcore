// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CustomConvertersSqliteTest : CustomConvertersTestBase<CustomConvertersSqliteTest.CustomConvertersSqliteFixture>
    {
        public CustomConvertersSqliteTest(CustomConvertersSqliteFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        // Disabled: SQLite database is case-sensitive
        public override void Can_insert_and_read_back_with_case_insensitive_string_key()
        {
        }

        [ConditionalFact]
        public override void Value_conversion_is_appropriately_used_for_join_condition()
        {
            base.Value_conversion_is_appropriately_used_for_join_condition();

            AssertSql(
                @"@__blogId_0='1' (DbType = String)

SELECT ""b"".""Url""
FROM ""Blog"" AS ""b""
INNER JOIN ""Post"" AS ""p"" ON ((""b"".""BlogId"" = ""p"".""BlogId"") AND (""b"".""IsVisible"" = 'Y')) AND (""b"".""BlogId"" = @__blogId_0)
WHERE ""b"".""IsVisible"" = 'Y'");
        }

        [ConditionalFact]
        public override void Value_conversion_is_appropriately_used_for_left_join_condition()
        {
            base.Value_conversion_is_appropriately_used_for_left_join_condition();

            AssertSql(
                @"@__blogId_0='1' (DbType = String)

SELECT ""b"".""Url""
FROM ""Blog"" AS ""b""
LEFT JOIN ""Post"" AS ""p"" ON ((""b"".""BlogId"" = ""p"".""BlogId"") AND (""b"".""IsVisible"" = 'Y')) AND (""b"".""BlogId"" = @__blogId_0)
WHERE ""b"".""IsVisible"" = 'Y'");
        }

        [ConditionalFact]
        public override void Where_bool_gets_converted_to_equality_when_value_conversion_is_used()
        {
            base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used();

            AssertSql(
                @"SELECT ""b"".""BlogId"", ""b"".""Discriminator"", ""b"".""IndexerVisible"", ""b"".""IsVisible"", ""b"".""Url"", ""b"".""RssUrl""
FROM ""Blog"" AS ""b""
WHERE ""b"".""IsVisible"" = 'Y'");
        }

        public override void Where_bool_with_value_conversion_inside_comparison_doesnt_get_converted_twice()
        {
            base.Where_bool_with_value_conversion_inside_comparison_doesnt_get_converted_twice();

            AssertSql(
                @"SELECT ""b"".""BlogId"", ""b"".""Discriminator"", ""b"".""IndexerVisible"", ""b"".""IsVisible"", ""b"".""Url"", ""b"".""RssUrl""
FROM ""Blog"" AS ""b""
WHERE ""b"".""IsVisible"" = 'Y'",
                //
                @"SELECT ""b"".""BlogId"", ""b"".""Discriminator"", ""b"".""IndexerVisible"", ""b"".""IsVisible"", ""b"".""Url"", ""b"".""RssUrl""
FROM ""Blog"" AS ""b""
WHERE ""b"".""IsVisible"" <> 'Y'");
        }

        public override void Select_bool_with_value_conversion_is_used()
        {
            base.Select_bool_with_value_conversion_is_used();

            AssertSql(
                @"SELECT ""b"".""IsVisible""
FROM ""Blog"" AS ""b""");
        }

        [ConditionalFact]
        public override void Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty()
        {
            base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty();

            AssertSql(
                @"SELECT ""b"".""BlogId"", ""b"".""Discriminator"", ""b"".""IndexerVisible"", ""b"".""IsVisible"", ""b"".""Url"", ""b"".""RssUrl""
FROM ""Blog"" AS ""b""
WHERE ""b"".""IsVisible"" = 'Y'");
        }

        [ConditionalFact]
        public override void Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer()
        {
            base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer();

            AssertSql(
                @"SELECT ""b"".""BlogId"", ""b"".""Discriminator"", ""b"".""IndexerVisible"", ""b"".""IsVisible"", ""b"".""Url"", ""b"".""RssUrl""
FROM ""Blog"" AS ""b""
WHERE ""b"".""IndexerVisible"" <> 'Aye'");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class CustomConvertersSqliteFixture : CustomConvertersFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            public override bool SupportsDecimalComparisons => false;

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();
        }
    }
}
