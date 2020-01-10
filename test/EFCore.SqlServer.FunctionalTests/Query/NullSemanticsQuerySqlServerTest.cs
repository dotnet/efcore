// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullSemanticsQuerySqlServerTest : NullSemanticsQueryTestBase<NullSemanticsQuerySqlServerFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public NullSemanticsQuerySqlServerTest(NullSemanticsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Compare_bool_with_bool_equal(bool async)
        {
            await base.Compare_bool_with_bool_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[NullableBoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_negated_bool_with_bool_equal(bool async)
        {
            await base.Compare_negated_bool_with_bool_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_bool_with_negated_bool_equal(bool async)
        {
            await base.Compare_bool_with_negated_bool_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_negated_bool_with_negated_bool_equal(bool async)
        {
            await base.Compare_negated_bool_with_negated_bool_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_bool_with_bool_equal_negated(bool async)
        {
            await base.Compare_bool_with_bool_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_negated_bool_with_bool_equal_negated(bool async)
        {
            await base.Compare_negated_bool_with_bool_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_bool_with_negated_bool_equal_negated(bool async)
        {
            await base.Compare_bool_with_negated_bool_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_negated_bool_with_negated_bool_equal_negated(bool async)
        {
            await base.Compare_negated_bool_with_negated_bool_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_bool_with_bool_not_equal(bool async)
        {
            await base.Compare_bool_with_bool_not_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_negated_bool_with_bool_not_equal(bool async)
        {
            await base.Compare_negated_bool_with_bool_not_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_bool_with_negated_bool_not_equal(bool async)
        {
            await base.Compare_bool_with_negated_bool_not_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_negated_bool_with_negated_bool_not_equal(bool async)
        {
            await base.Compare_negated_bool_with_negated_bool_not_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_bool_with_bool_not_equal_negated(bool async)
        {
            await base.Compare_bool_with_bool_not_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_negated_bool_with_bool_not_equal_negated(bool async)
        {
            await base.Compare_negated_bool_with_bool_not_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_bool_with_negated_bool_not_equal_negated(bool async)
        {
            await base.Compare_bool_with_negated_bool_not_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_negated_bool_with_negated_bool_not_equal_negated(bool async)
        {
            await base.Compare_negated_bool_with_negated_bool_not_equal_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_equals_method(bool async)
        {
            await base.Compare_equals_method(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[NullableBoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_equals_method_static(bool async)
        {
            await base.Compare_equals_method_static(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = [e].[NullableBoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)");
        }

        public override async Task Compare_equals_method_negated(bool async)
        {
            await base.Compare_equals_method_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_equals_method_negated_static(bool async)
        {
            await base.Compare_equals_method_negated_static(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Compare_complex_equal_equal_equal(bool async)
        {
            await base.Compare_complex_equal_equal_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] = [e].[BoolB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN [e].[IntA] = [e].[IntB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] = [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN ([e].[IntA] = [e].[NullableIntB]) AND [e].[NullableIntB] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] = [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Compare_complex_equal_not_equal_equal(bool async)
        {
            await base.Compare_complex_equal_not_equal_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] = [e].[BoolB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN [e].[IntA] = [e].[IntB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] = [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN ([e].[IntA] = [e].[NullableIntB]) AND [e].[NullableIntB] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] = [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Compare_complex_not_equal_equal_equal(bool async)
        {
            await base.Compare_complex_not_equal_equal_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN [e].[IntA] = [e].[IntB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN ([e].[IntA] = [e].[NullableIntB]) AND [e].[NullableIntB] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Compare_complex_not_equal_not_equal_equal(bool async)
        {
            await base.Compare_complex_not_equal_not_equal_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN [e].[IntA] = [e].[IntB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN ([e].[IntA] = [e].[NullableIntB]) AND [e].[NullableIntB] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Compare_complex_not_equal_equal_not_equal(bool async)
        {
            await base.Compare_complex_not_equal_equal_not_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN [e].[IntA] <> [e].[IntB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN ([e].[IntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN (([e].[NullableIntA] <> [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL)) AND ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Compare_complex_not_equal_not_equal_not_equal(bool async)
        {
            await base.Compare_complex_not_equal_not_equal_not_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN [e].[IntA] <> [e].[IntB] THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN ([e].[IntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> CASE
    WHEN (([e].[NullableIntA] <> [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL)) AND ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Compare_nullable_with_null_parameter_equal(bool async)
        {
            await base.Compare_nullable_with_null_parameter_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IS NULL");
        }

        public override async Task Compare_nullable_with_non_null_parameter_not_equal(bool async)
        {
            await base.Compare_nullable_with_non_null_parameter_not_equal(async);

            AssertSql(
                @"@__prm_0='Foo' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] = @__prm_0");
        }

        public override async Task Join_uses_database_semantics(bool async)
        {
            await base.Join_uses_database_semantics(async);

            AssertSql(
                @"SELECT [e].[Id] AS [Id1], [e0].[Id] AS [Id2], [e].[NullableIntA], [e0].[NullableIntB]
FROM [Entities1] AS [e]
INNER JOIN [Entities2] AS [e0] ON [e].[NullableIntA] = [e0].[NullableIntB]");
        }

        public override async Task Contains_with_local_array_closure_with_null(bool async)
        {
            await base.Contains_with_local_array_closure_with_null(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override async Task Contains_with_local_array_closure_false_with_null(bool async)
        {
            await base.Contains_with_local_array_closure_false_with_null(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo') AND [e].[NullableStringA] IS NOT NULL");
        }

        public override async Task Contains_with_local_nullable_array_closure_negated(bool async)
        {
            await base.Contains_with_local_nullable_array_closure_negated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override async Task Contains_with_local_array_closure_with_multiple_nulls(bool async)
        {
            await base.Contains_with_local_array_closure_with_multiple_nulls(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override async Task Where_multiple_ors_with_null(bool async)
        {
            await base.Where_multiple_ors_with_null(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringA] = N'Foo') OR ([e].[NullableStringA] = N'Blah')) OR [e].[NullableStringA] IS NULL");
        }

        public override async Task Where_multiple_ands_with_null(bool async)
        {
            await base.Where_multiple_ands_with_null(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL) AND (([e].[NullableStringA] <> N'Blah') OR [e].[NullableStringA] IS NULL)) AND [e].[NullableStringA] IS NOT NULL");
        }

        public override async Task Where_multiple_ors_with_nullable_parameter(bool async)
        {
            await base.Where_multiple_ors_with_nullable_parameter(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableStringA] = N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override async Task Where_multiple_ands_with_nullable_parameter_and_constant(bool async)
        {
            await base.Where_multiple_ands_with_nullable_parameter_and_constant(async);

            AssertSql(
                @"@__prm3_2='Blah' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL) AND [e].[NullableStringA] IS NOT NULL) AND ([e].[NullableStringA] <> @__prm3_2)");
        }

        public override async Task Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized(bool async)
        {
            await base.Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized(async);

            AssertSql(
                @"@__prm3_2='Blah' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringB] IS NOT NULL AND (([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL)) AND [e].[NullableStringA] IS NOT NULL) AND ([e].[NullableStringA] <> @__prm3_2)");
        }

        public override async Task Where_coalesce(bool async)
        {
            await base.Where_coalesce(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE COALESCE([e].[NullableBoolA], CAST(1 AS bit)) = CAST(1 AS bit)");
        }

        public override async Task Where_equal_nullable_with_null_value_parameter(bool async)
        {
            await base.Where_equal_nullable_with_null_value_parameter(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IS NULL");
        }

        public override async Task Where_not_equal_nullable_with_null_value_parameter(bool async)
        {
            await base.Where_not_equal_nullable_with_null_value_parameter(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IS NOT NULL");
        }

        public override async Task Where_equal_with_coalesce(bool async)
        {
            await base.Where_equal_with_coalesce(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (COALESCE([e].[NullableStringA], [e].[NullableStringB]) = [e].[NullableStringC]) OR (([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) AND [e].[NullableStringC] IS NULL)");
        }

        public override async Task Where_not_equal_with_coalesce(bool async)
        {
            await base.Where_not_equal_with_coalesce(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((COALESCE([e].[NullableStringA], [e].[NullableStringB]) <> [e].[NullableStringC]) OR (([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) OR [e].[NullableStringC] IS NULL)) AND (([e].[NullableStringA] IS NOT NULL OR [e].[NullableStringB] IS NOT NULL) OR [e].[NullableStringC] IS NOT NULL)");
        }

        public override async Task Where_equal_with_coalesce_both_sides(bool async)
        {
            await base.Where_equal_with_coalesce_both_sides(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE COALESCE([e].[NullableStringA], [e].[NullableStringB]) = COALESCE([e].[StringA], [e].[StringB])");
        }

        public override async Task Where_not_equal_with_coalesce_both_sides(bool async)
        {
            await base.Where_not_equal_with_coalesce_both_sides(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((COALESCE([e].[NullableIntA], [e].[NullableIntB]) <> COALESCE([e].[NullableIntC], [e].[NullableIntB])) OR (([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL) OR ([e].[NullableIntC] IS NULL AND [e].[NullableIntB] IS NULL))) AND (([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL) OR ([e].[NullableIntC] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL))");
        }

        public override async Task Where_equal_with_conditional(bool async)
        {
            await base.Where_equal_with_conditional(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) THEN [e].[NullableStringA]
    ELSE [e].[NullableStringB]
END = [e].[NullableStringC]) OR (CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) THEN [e].[NullableStringA]
    ELSE [e].[NullableStringB]
END IS NULL AND [e].[NullableStringC] IS NULL)");
        }

        public override async Task Where_not_equal_with_conditional(bool async)
        {
            await base.Where_not_equal_with_conditional(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringC] <> CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) THEN [e].[NullableStringA]
    ELSE [e].[NullableStringB]
END) OR ([e].[NullableStringC] IS NULL OR CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) THEN [e].[NullableStringA]
    ELSE [e].[NullableStringB]
END IS NULL)) AND ([e].[NullableStringC] IS NOT NULL OR CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) THEN [e].[NullableStringA]
    ELSE [e].[NullableStringB]
END IS NOT NULL)");
        }

        public override async Task Where_equal_with_conditional_non_nullable(bool async)
        {
            await base.Where_equal_with_conditional_non_nullable(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableStringC] <> CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) THEN [e].[StringA]
    ELSE [e].[StringB]
END) OR [e].[NullableStringC] IS NULL");
        }

        public override async Task Where_equal_with_and_and_contains(bool async)
        {
            await base.Where_equal_with_and_and_contains(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringB] = N'') OR (CHARINDEX([e].[NullableStringB], [e].[NullableStringA]) > 0)) AND ([e].[BoolA] = CAST(1 AS bit))");
        }

        public override async Task Null_comparison_in_selector_with_relational_nulls(bool async)
        {
            await base.Null_comparison_in_selector_with_relational_nulls(async);

            AssertSql(
                @"SELECT CASE
    WHEN ([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Entities1] AS [e]");
        }

        public override async Task Null_comparison_in_order_by_with_relational_nulls(bool async)
        {
            await base.Null_comparison_in_order_by_with_relational_nulls(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
ORDER BY CASE
    WHEN ([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, CASE
    WHEN ([e].[NullableIntB] <> 10) OR [e].[NullableIntB] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Null_comparison_in_join_key_with_relational_nulls(bool async)
        {
            await base.Null_comparison_in_join_key_with_relational_nulls(async);

            AssertSql(
                @"SELECT [e1].[Id], [e1].[BoolA], [e1].[BoolB], [e1].[BoolC], [e1].[IntA], [e1].[IntB], [e1].[IntC], [e1].[NullableBoolA], [e1].[NullableBoolB], [e1].[NullableBoolC], [e1].[NullableIntA], [e1].[NullableIntB], [e1].[NullableIntC], [e1].[NullableStringA], [e1].[NullableStringB], [e1].[NullableStringC], [e1].[StringA], [e1].[StringB], [e1].[StringC], [e2].[Id], [e2].[BoolA], [e2].[BoolB], [e2].[BoolC], [e2].[IntA], [e2].[IntB], [e2].[IntC], [e2].[NullableBoolA], [e2].[NullableBoolB], [e2].[NullableBoolC], [e2].[NullableIntA], [e2].[NullableIntB], [e2].[NullableIntC], [e2].[NullableStringA], [e2].[NullableStringB], [e2].[NullableStringC], [e2].[StringA], [e2].[StringB], [e2].[StringC]
FROM [Entities1] AS [e1]
INNER JOIN [Entities2] AS [e2] ON CASE
    WHEN [e1].[NullableStringA] <> N'Foo'
    THEN CAST(1 AS bit) ELSE CAST(0 AS bit)
END = CASE
    WHEN [e2].[NullableBoolB] <> CAST(1 AS bit)
    THEN CAST(1 AS bit) ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_conditional_search_condition_in_result(bool async)
        {
            await base.Where_conditional_search_condition_in_result(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[StringA] IN (N'Foo', N'Bar')",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[StringA] LIKE N'A%'");
        }

        public override async Task Where_nested_conditional_search_condition_in_result(bool async)
        {
            await base.Where_nested_conditional_search_condition_in_result(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]");
        }

        public override void Where_equal_using_relational_null_semantics()
        {
            base.Where_equal_using_relational_null_semantics();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = [e].[NullableBoolB]");
        }

        public override async Task Where_nullable_bool(bool async)
        {
            await base.Where_nullable_bool(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = CAST(1 AS bit)");
        }

        public override async Task Where_nullable_bool_equal_with_constant(bool async)
        {
            await base.Where_nullable_bool_equal_with_constant(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = CAST(1 AS bit)");
        }

        public override async Task Where_nullable_bool_with_null_check(bool async)
        {
            await base.Where_nullable_bool_with_null_check(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] IS NOT NULL AND ([e].[NullableBoolA] = CAST(1 AS bit))");
        }

        public override void Where_equal_using_relational_null_semantics_with_parameter()
        {
            base.Where_equal_using_relational_null_semantics_with_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] IS NULL");
        }

        public override void Where_equal_using_relational_null_semantics_complex_with_parameter()
        {
            base.Where_equal_using_relational_null_semantics_complex_with_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = [e].[NullableBoolB]");
        }

        public override void Where_not_equal_using_relational_null_semantics()
        {
            base.Where_not_equal_using_relational_null_semantics();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] <> [e].[NullableBoolB]");
        }

        public override void Where_not_equal_using_relational_null_semantics_with_parameter()
        {
            base.Where_not_equal_using_relational_null_semantics_with_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] IS NOT NULL");
        }

        public override void Where_not_equal_using_relational_null_semantics_complex_with_parameter()
        {
            base.Where_not_equal_using_relational_null_semantics_complex_with_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] <> [e].[NullableBoolB]");
        }

        public override async Task Where_comparison_null_constant_and_null_parameter(bool async)
        {
            await base.Where_comparison_null_constant_and_null_parameter(async);

            AssertSql(
                @"@__p_0='True'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)",
                //
                @"@__p_0='False'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        public override async Task Where_comparison_null_constant_and_nonnull_parameter(bool async)
        {
            await base.Where_comparison_null_constant_and_nonnull_parameter(async);

            AssertSql(
                @"@__p_0='False'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)",
                //
                @"@__p_0='True'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        public override async Task Where_comparison_nonnull_constant_and_null_parameter(bool async)
        {
            await base.Where_comparison_nonnull_constant_and_null_parameter(async);

            AssertSql(
                @"@__p_0='False'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)",
                //
                @"@__p_0='True'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        public override async Task Where_comparison_null_semantics_optimization_works_with_complex_predicates(bool async)
        {
            await base.Where_comparison_null_semantics_optimization_works_with_complex_predicates(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IS NULL");
        }

        public override void Switching_null_semantics_produces_different_cache_entry()
        {
            base.Switching_null_semantics_produces_different_cache_entry();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = [e].[NullableBoolB]");
        }

        public override void Switching_parameter_value_to_null_produces_different_cache_entry()
        {
            base.Switching_parameter_value_to_null_produces_different_cache_entry();

            AssertSql(
                @"@__p_0='True'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)",
                //
                @"@__p_0='False'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        public override void From_sql_composed_with_relational_null_comparison()
        {
            base.From_sql_composed_with_relational_null_comparison();

            AssertSql(
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM (
    SELECT * FROM ""Entities1""
) AS [e]
WHERE [e].[StringA] = [e].[StringB]");
        }

        public override async Task Projecting_nullable_bool_with_coalesce(bool async)
        {
            await base.Projecting_nullable_bool_with_coalesce(async);

            AssertSql(
                @"SELECT [e].[Id], COALESCE([e].[NullableBoolA], CAST(0 AS bit)) AS [Coalesce]
FROM [Entities1] AS [e]");
        }

        public override async Task Projecting_nullable_bool_with_coalesce_nested(bool async)
        {
            await base.Projecting_nullable_bool_with_coalesce_nested(async);

            AssertSql(
                @"SELECT [e].[Id], COALESCE([e].[NullableBoolA], COALESCE([e].[NullableBoolB], CAST(0 AS bit))) AS [Coalesce]
FROM [Entities1] AS [e]");
        }

        public override async Task Null_semantics_applied_when_comparing_function_with_nullable_argument_to_a_nullable_column(bool async)
        {
            await base.Null_semantics_applied_when_comparing_function_with_nullable_argument_to_a_nullable_column(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END = [e].[NullableIntA]) OR (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NULL AND [e].[NullableIntA] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringA]) AS int) - 1
END = [e].[NullableIntA]) OR (CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringA]) AS int) - 1
END IS NULL AND [e].[NullableIntA] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END <> [e].[NullableIntB]) OR (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NULL OR [e].[NullableIntB] IS NULL)) AND (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NOT NULL OR [e].[NullableIntB] IS NOT NULL)");
            // issue #18773
            //            AssertSql(
            //                @"SELECT [e].[Id]
            //FROM [Entities1] AS [e]
            //WHERE ((CHARINDEX(N'oo', [e].[NullableStringA]) - 1) = [e].[NullableIntA]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableIntA] IS NULL)",
            //                //
            //                @"SELECT [e].[Id]
            //FROM [Entities1] AS [e]
            //WHERE ((CHARINDEX(N'ar', [e].[NullableStringA]) - 1) = [e].[NullableIntA]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableIntA] IS NULL)",
            //                //
            //                @"SELECT [e].[Id]
            //FROM [Entities1] AS [e]
            //WHERE (((CHARINDEX(N'oo', [e].[NullableStringA]) - 1) <> [e].[NullableIntB]) OR ([e].[NullableStringA] IS NULL OR [e].[NullableIntB] IS NULL)) AND ([e].[NullableStringA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL)");
        }

        public override async Task Where_IndexOf_empty(bool async)
        {
            await base.Where_IndexOf_empty(async);

            AssertSql(
                @"");
        }

        public override async Task Select_IndexOf(bool async)
        {
            await base.Select_IndexOf(async);

            AssertSql(
                @"SELECT CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END
FROM [Entities1] AS [e]
ORDER BY [e].[Id]");
        }

        public override async Task Null_semantics_applied_when_comparing_two_functions_with_nullable_arguments(bool async)
        {
            await base.Null_semantics_applied_when_comparing_two_functions_with_nullable_arguments(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END = CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringB]) AS int) - 1
END) OR (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NULL AND CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringB]) AS int) - 1
END IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END <> CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringB]) AS int) - 1
END) OR (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NULL OR CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringB]) AS int) - 1
END IS NULL)) AND (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NOT NULL OR CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringB]) AS int) - 1
END IS NOT NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END <> CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringA]) AS int) - 1
END) OR (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NULL OR CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringA]) AS int) - 1
END IS NULL)) AND (CASE
    WHEN N'oo' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'oo', [e].[NullableStringA]) AS int) - 1
END IS NOT NULL OR CASE
    WHEN N'ar' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'ar', [e].[NullableStringA]) AS int) - 1
END IS NOT NULL)");
            // issue #18773
            //            AssertSql(
            //                @"SELECT [e].[Id]
            //FROM [Entities1] AS [e]
            //WHERE ((CHARINDEX(N'oo', [e].[NullableStringA]) - 1) = (CHARINDEX(N'ar', [e].[NullableStringB]) - 1)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)",
            //                //
            //                @"SELECT [e].[Id]
            //FROM [Entities1] AS [e]
            //WHERE (((CHARINDEX(N'oo', [e].[NullableStringA]) - 1) <> (CHARINDEX(N'ar', [e].[NullableStringB]) - 1)) OR ([e].[NullableStringA] IS NULL OR [e].[NullableStringB] IS NULL)) AND ([e].[NullableStringA] IS NOT NULL OR [e].[NullableStringB] IS NOT NULL)",
            //                //
            //                @"SELECT [e].[Id]
            //FROM [Entities1] AS [e]
            //WHERE (((CHARINDEX(N'oo', [e].[NullableStringA]) - 1) <> (CHARINDEX(N'ar', [e].[NullableStringA]) - 1)) OR [e].[NullableStringA] IS NULL) AND [e].[NullableStringA] IS NOT NULL");
        }

        public override async Task Null_semantics_applied_when_comparing_two_functions_with_multiple_nullable_arguments(bool async)
        {
            await base.Null_semantics_applied_when_comparing_two_functions_with_multiple_nullable_arguments(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (REPLACE([e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC]) = [e].[NullableStringA]) OR ((([e].[NullableStringA] IS NULL OR [e].[NullableStringB] IS NULL) OR [e].[NullableStringC] IS NULL) AND [e].[NullableStringA] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((REPLACE([e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC]) <> [e].[NullableStringA]) OR ((([e].[NullableStringA] IS NULL OR [e].[NullableStringB] IS NULL) OR [e].[NullableStringC] IS NULL) OR [e].[NullableStringA] IS NULL)) AND ((([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL) AND [e].[NullableStringC] IS NOT NULL) OR [e].[NullableStringA] IS NOT NULL)");
        }

        public override async Task Null_semantics_coalesce(bool async)
        {
            await base.Null_semantics_coalesce(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = COALESCE([e].[NullableBoolB], [e].[BoolC])",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableBoolA] = COALESCE([e].[NullableBoolB], [e].[NullableBoolC])) OR ([e].[NullableBoolA] IS NULL AND ([e].[NullableBoolB] IS NULL AND [e].[NullableBoolC] IS NULL))",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (COALESCE([e].[NullableBoolB], [e].[BoolC]) <> [e].[NullableBoolA]) OR [e].[NullableBoolA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((COALESCE([e].[NullableBoolB], [e].[NullableBoolC]) <> [e].[NullableBoolA]) OR (([e].[NullableBoolB] IS NULL AND [e].[NullableBoolC] IS NULL) OR [e].[NullableBoolA] IS NULL)) AND (([e].[NullableBoolB] IS NOT NULL OR [e].[NullableBoolC] IS NOT NULL) OR [e].[NullableBoolA] IS NOT NULL)");
        }

        public override async Task Null_semantics_conditional(bool async)
        {
            await base.Null_semantics_conditional(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[BoolA] = CASE
    WHEN [e].[BoolB] = CAST(1 AS bit) THEN [e].[NullableBoolB]
    ELSE [e].[NullableBoolC]
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL) THEN [e].[BoolB]
    ELSE [e].[BoolC]
END = [e].[BoolA]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN CASE
        WHEN [e].[BoolA] = CAST(1 AS bit) THEN CASE
            WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL) THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END
        ELSE [e].[BoolC]
    END <> [e].[BoolB] THEN [e].[BoolA]
    ELSE CASE
        WHEN (([e].[NullableBoolB] = [e].[NullableBoolC]) AND ([e].[NullableBoolB] IS NOT NULL AND [e].[NullableBoolC] IS NOT NULL)) OR ([e].[NullableBoolB] IS NULL AND [e].[NullableBoolC] IS NULL) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END = CAST(1 AS bit)");
        }

        public override async Task Null_semantics_function(bool async)
        {
            await base.Null_semantics_function(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((SUBSTRING([e].[NullableStringA], 0 + 1, [e].[IntA]) <> [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL OR [e].[NullableStringB] IS NULL)) AND ([e].[NullableStringA] IS NOT NULL OR [e].[NullableStringB] IS NOT NULL)");
        }

        public override async Task Null_semantics_join_with_composite_key(bool async)
        {
            await base.Null_semantics_join_with_composite_key(async);

            AssertSql(
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC], [e0].[Id], [e0].[BoolA], [e0].[BoolB], [e0].[BoolC], [e0].[IntA], [e0].[IntB], [e0].[IntC], [e0].[NullableBoolA], [e0].[NullableBoolB], [e0].[NullableBoolC], [e0].[NullableIntA], [e0].[NullableIntB], [e0].[NullableIntC], [e0].[NullableStringA], [e0].[NullableStringB], [e0].[NullableStringC], [e0].[StringA], [e0].[StringB], [e0].[StringC]
FROM [Entities1] AS [e]
INNER JOIN [Entities2] AS [e0] ON (([e].[NullableStringA] = [e0].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e0].[NullableStringB] IS NULL)) AND (CASE
    WHEN (([e].[NullableStringB] <> [e].[NullableStringC]) OR ([e].[NullableStringB] IS NULL OR [e].[NullableStringC] IS NULL)) AND ([e].[NullableStringB] IS NOT NULL OR [e].[NullableStringC] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = COALESCE([e0].[NullableBoolA], [e0].[BoolC]))");
        }

        public override async Task Null_semantics_contains(bool async)
        {
            await base.Null_semantics_contains(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IN (1, 2)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] NOT IN (1, 2) OR [e].[NullableIntA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IN (1, 2) OR [e].[NullableIntA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] NOT IN (1, 2) AND [e].[NullableIntA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IN (1, 2)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] NOT IN (1, 2) OR [e].[NullableIntA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IN (1, 2) OR [e].[NullableIntA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] NOT IN (1, 2) AND [e].[NullableIntA] IS NOT NULL");
        }

        public override async Task Null_semantics_contains_array_with_no_values(bool async)
        {
            await base.Null_semantics_contains_array_with_no_values(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL");
        }

        public override async Task Null_semantics_contains_non_nullable_argument(bool async)
        {
            await base.Null_semantics_contains_non_nullable_argument(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] IN (1, 2)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] NOT IN (1, 2)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] IN (1, 2)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] NOT IN (1, 2)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]");
        }

        public override async Task Null_semantics_with_null_check_simple(bool async)
        {
            await base.Null_semantics_with_null_check_simple(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ([e].[NullableIntA] = [e].[NullableIntB])",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND (([e].[NullableIntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ([e].[NullableIntA] = [e].[IntC])",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL) AND ([e].[NullableIntA] = [e].[NullableIntB])",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL) AND ([e].[NullableIntA] <> [e].[NullableIntB])");
        }

        public override async Task Null_semantics_with_null_check_complex(bool async)
        {
            await base.Null_semantics_with_null_check_complex(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ((([e].[NullableIntC] <> [e].[NullableIntA]) OR [e].[NullableIntC] IS NULL) OR ([e].[NullableIntB] IS NOT NULL AND ([e].[NullableIntA] <> [e].[NullableIntB])))",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ((([e].[NullableIntC] <> [e].[NullableIntA]) OR [e].[NullableIntC] IS NULL) OR (([e].[NullableIntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL))",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL) AND (([e].[NullableIntA] = [e].[NullableIntC]) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntC] IS NULL))");
        }

        public override async Task Null_semantics_with_null_check_complex2(bool async)
        {
            await base.Null_semantics_with_null_check_complex2(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL) AND (([e].[NullableBoolB] <> [e].[NullableBoolA]) OR [e].[NullableBoolC] IS NOT NULL)) AND (([e].[NullableBoolC] <> [e].[NullableBoolB]) OR [e].[NullableBoolC] IS NULL)) OR (([e].[NullableBoolC] <> [e].[BoolB]) OR [e].[NullableBoolC] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL) AND (([e].[NullableBoolB] <> [e].[NullableBoolA]) OR [e].[NullableBoolC] IS NOT NULL)) AND (([e].[NullableBoolC] <> [e].[NullableBoolB]) OR [e].[NullableBoolC] IS NULL)) OR (([e].[NullableBoolB] <> [e].[BoolB]) OR [e].[NullableBoolB] IS NULL)");
        }

        public override async Task IsNull_on_complex_expression(bool async)
        {
            await base.IsNull_on_complex_expression(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL");
        }

        public override async Task Coalesce_not_equal(bool async)
        {
            await base.Coalesce_not_equal(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE COALESCE([e].[NullableIntA], 0) <> 0");
        }

        public override async Task Negated_order_comparison_on_non_nullable_arguments_gets_optimized(bool async)
        {
            await base.Negated_order_comparison_on_non_nullable_arguments_gets_optimized(async);

            AssertSql(
                @"@__i_0='1'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] <= @__i_0",
                //
                @"@__i_0='1'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] < @__i_0",
                //
                @"@__i_0='1'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] >= @__i_0",
                //
                @"@__i_0='1'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[IntA] > @__i_0");
        }

        public override async Task Negated_order_comparison_on_nullable_arguments_doesnt_get_optimized(bool async)
        {
            await base.Negated_order_comparison_on_nullable_arguments_doesnt_get_optimized(async);

            AssertSql(
                @"");
        }

        public override async Task Nullable_column_info_propagates_inside_binary_AndAlso(bool async)
        {
            await base.Nullable_column_info_propagates_inside_binary_AndAlso(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL) AND ([e].[NullableStringA] <> [e].[NullableStringB])");
        }

        public override async Task Nullable_column_info_doesnt_propagate_inside_binary_OrElse(bool async)
        {
            await base.Nullable_column_info_doesnt_propagate_inside_binary_OrElse(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableStringA] IS NOT NULL OR [e].[NullableStringB] IS NOT NULL) AND ((([e].[NullableStringA] <> [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL OR [e].[NullableStringB] IS NULL)) AND ([e].[NullableStringA] IS NOT NULL OR [e].[NullableStringB] IS NOT NULL))");
        }

        public override async Task Nullable_column_info_propagates_inside_binary_OrElse_when_info_is_duplicated(bool async)
        {
            await base.Nullable_column_info_propagates_inside_binary_OrElse_when_info_is_duplicated(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL) OR [e].[NullableStringA] IS NOT NULL) AND (([e].[NullableStringA] <> [e].[NullableStringB]) OR [e].[NullableStringB] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL) OR ([e].[NullableStringB] IS NOT NULL AND [e].[NullableStringA] IS NOT NULL)) AND ([e].[NullableStringA] <> [e].[NullableStringB])");
        }

        public override async Task Nullable_column_info_propagates_inside_conditional(bool async)
        {
            await base.Nullable_column_info_propagates_inside_conditional(async);

            AssertSql(
                @"SELECT CASE
    WHEN [e].[NullableStringA] IS NOT NULL THEN CASE
        WHEN [e].[NullableStringA] <> [e].[StringA] THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE [e].[BoolA]
END
FROM [Entities1] AS [e]");
        }

        public override async Task Nullable_column_info_doesnt_propagate_between_projections(bool async)
        {
            await base.Nullable_column_info_doesnt_propagate_between_projections(async);

            AssertSql(
                @"SELECT CASE
    WHEN [e].[NullableStringA] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Foo], CASE
    WHEN ([e].[NullableStringA] <> [e].[StringA]) OR [e].[NullableStringA] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Bar]
FROM [Entities1] AS [e]");
        }

        public override async Task Nullable_column_info_doesnt_propagate_between_different_parts_of_select(bool async)
        {
            await base.Nullable_column_info_doesnt_propagate_between_different_parts_of_select(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
INNER JOIN [Entities1] AS [e0] ON [e].[NullableBoolA] IS NULL
WHERE (([e].[NullableBoolA] <> [e0].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e0].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e0].[NullableBoolB] IS NOT NULL)");
        }

        public override async Task Nullable_column_info_propagation_complex(bool async)
        {
            await base.Nullable_column_info_propagation_complex(async);

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL) AND [e].[NullableStringC] IS NOT NULL) AND (([e].[NullableBoolB] <> [e].[NullableBoolC]) OR [e].[NullableBoolC] IS NULL)");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
        {
            var options = new DbContextOptionsBuilder(Fixture.CreateOptions());
            if (useRelationalNulls)
            {
                new SqlServerDbContextOptionsBuilder(options).UseRelationalNulls();
            }

            var context = new NullSemanticsContext(options.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }
}
