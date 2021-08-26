// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullSemanticsQuerySqliteTest : NullSemanticsQueryTestBase<NullSemanticsQuerySqliteFixture>
    {
        public NullSemanticsQuerySqliteTest(NullSemanticsQuerySqliteFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override async Task Bool_equal_nullable_bool_HasValue(bool async)
        {
            await base.Bool_equal_nullable_bool_HasValue(async);

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE ""e"".""NullableBoolA"" IS NOT NULL",
                //
                @"@__prm_0='False' (DbType = String)

SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE @__prm_0 = (""e"".""NullableBoolA"" IS NOT NULL)",
                //
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE ""e"".""BoolB"" = (""e"".""NullableBoolA"" IS NOT NULL)");
        }

        public override async Task Bool_equal_nullable_bool_compared_to_null(bool async)
        {
            await base.Bool_equal_nullable_bool_compared_to_null(async);

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE ""e"".""NullableBoolA"" IS NULL",
                //
                @"@__prm_0='False' (DbType = String)

SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE @__prm_0 = (""e"".""NullableBoolA"" IS NOT NULL)");
        }

        public override async Task Bool_not_equal_nullable_bool_HasValue(bool async)
        {
            await base.Bool_not_equal_nullable_bool_HasValue(async);

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE ""e"".""NullableBoolA"" IS NULL",
                //
                @"@__prm_0='False' (DbType = String)

SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE @__prm_0 <> (""e"".""NullableBoolA"" IS NOT NULL)",
                //
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE ""e"".""BoolB"" <> (""e"".""NullableBoolA"" IS NOT NULL)");
        }

        public override async Task Bool_not_equal_nullable_bool_compared_to_null(bool async)
        {
            await base.Bool_not_equal_nullable_bool_compared_to_null(async);

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE ""e"".""NullableBoolA"" IS NOT NULL",
                //
                @"@__prm_0='False' (DbType = String)

SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE @__prm_0 <> (""e"".""NullableBoolA"" IS NOT NULL)");
        }

        public override async Task Bool_logical_operation_with_nullable_bool_HasValue(bool async)
        {
            await base.Bool_logical_operation_with_nullable_bool_HasValue(async);

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""",
                //
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE 0",
                //
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE ""e"".""BoolB"" | (""e"".""NullableBoolA"" IS NOT NULL)");
        }

        public override async Task Comparison_compared_to_null_check_on_bool(bool async)
        {
            await base.Comparison_compared_to_null_check_on_bool(async);

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE (""e"".""IntA"" = ""e"".""IntB"") <> (""e"".""NullableBoolA"" IS NOT NULL)",
                //
                @"SELECT ""e"".""Id"", ""e"".""BoolA"", ""e"".""BoolB"", ""e"".""BoolC"", ""e"".""IntA"", ""e"".""IntB"", ""e"".""IntC"", ""e"".""NullableBoolA"", ""e"".""NullableBoolB"", ""e"".""NullableBoolC"", ""e"".""NullableIntA"", ""e"".""NullableIntB"", ""e"".""NullableIntC"", ""e"".""NullableStringA"", ""e"".""NullableStringB"", ""e"".""NullableStringC"", ""e"".""StringA"", ""e"".""StringB"", ""e"".""StringC""
FROM ""Entities1"" AS ""e""
WHERE (""e"".""IntA"" <> ""e"".""IntB"") = (""e"".""NullableBoolA"" IS NOT NULL)");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
        {
            var options = new DbContextOptionsBuilder(Fixture.CreateOptions());
            if (useRelationalNulls)
            {
                new SqliteDbContextOptionsBuilder(options).UseRelationalNulls();
            }

            var context = new NullSemanticsContext(options.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }
}
