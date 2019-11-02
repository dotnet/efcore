// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public override void Compare_bool_with_bool_equal()
        {
            base.Compare_bool_with_bool_equal();

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

        public override void Compare_negated_bool_with_bool_equal()
        {
            base.Compare_negated_bool_with_bool_equal();

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

        public override void Compare_bool_with_negated_bool_equal()
        {
            base.Compare_bool_with_negated_bool_equal();

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

        public override void Compare_negated_bool_with_negated_bool_equal()
        {
            base.Compare_negated_bool_with_negated_bool_equal();

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

        public override void Compare_bool_with_bool_equal_negated()
        {
            base.Compare_bool_with_bool_equal_negated();

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

        public override void Compare_negated_bool_with_bool_equal_negated()
        {
            base.Compare_negated_bool_with_bool_equal_negated();

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

        public override void Compare_bool_with_negated_bool_equal_negated()
        {
            base.Compare_bool_with_negated_bool_equal_negated();

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

        public override void Compare_negated_bool_with_negated_bool_equal_negated()
        {
            base.Compare_negated_bool_with_negated_bool_equal_negated();

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

        public override void Compare_bool_with_bool_not_equal()
        {
            base.Compare_bool_with_bool_not_equal();

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

        public override void Compare_negated_bool_with_bool_not_equal()
        {
            base.Compare_negated_bool_with_bool_not_equal();

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

        public override void Compare_bool_with_negated_bool_not_equal()
        {
            base.Compare_bool_with_negated_bool_not_equal();

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

        public override void Compare_negated_bool_with_negated_bool_not_equal()
        {
            base.Compare_negated_bool_with_negated_bool_not_equal();

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

        public override void Compare_bool_with_bool_not_equal_negated()
        {
            base.Compare_bool_with_bool_not_equal_negated();

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

        public override void Compare_negated_bool_with_bool_not_equal_negated()
        {
            base.Compare_negated_bool_with_bool_not_equal_negated();

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

        public override void Compare_bool_with_negated_bool_not_equal_negated()
        {
            base.Compare_bool_with_negated_bool_not_equal_negated();

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

        public override void Compare_negated_bool_with_negated_bool_not_equal_negated()
        {
            base.Compare_negated_bool_with_negated_bool_not_equal_negated();

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

        public override void Compare_equals_method()
        {
            base.Compare_equals_method();

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

        public override void Compare_equals_method_static()
        {
            base.Compare_equals_method_static();

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

        public override void Compare_equals_method_negated()
        {
            base.Compare_equals_method_negated();

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

        public override void Compare_equals_method_negated_static()
        {
            base.Compare_equals_method_negated_static();

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

        public override void Compare_complex_equal_equal_equal()
        {
            base.Compare_complex_equal_equal_equal();

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

        public override void Compare_complex_equal_not_equal_equal()
        {
            base.Compare_complex_equal_not_equal_equal();

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

        public override void Compare_complex_not_equal_equal_equal()
        {
            base.Compare_complex_not_equal_equal_equal();

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

        public override void Compare_complex_not_equal_not_equal_equal()
        {
            base.Compare_complex_not_equal_not_equal_equal();

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

        public override void Compare_complex_not_equal_equal_not_equal()
        {
            base.Compare_complex_not_equal_equal_not_equal();

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

        public override void Compare_complex_not_equal_not_equal_not_equal()
        {
            base.Compare_complex_not_equal_not_equal_not_equal();

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

        public override void Compare_nullable_with_null_parameter_equal()
        {
            base.Compare_nullable_with_null_parameter_equal();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IS NULL");
        }

        public override void Compare_nullable_with_non_null_parameter_not_equal()
        {
            base.Compare_nullable_with_non_null_parameter_not_equal();

            AssertSql(
                @"@__prm_0='Foo' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] = @__prm_0");
        }

        public override void Join_uses_database_semantics()
        {
            base.Join_uses_database_semantics();

            AssertSql(
                @"SELECT [e].[Id] AS [Id1], [e0].[Id] AS [Id2], [e].[NullableIntA], [e0].[NullableIntB]
FROM [Entities1] AS [e]
INNER JOIN [Entities2] AS [e0] ON [e].[NullableIntA] = [e0].[NullableIntB]");
        }

        public override void Contains_with_local_array_closure_with_null()
        {
            base.Contains_with_local_array_closure_with_null();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override void Contains_with_local_array_closure_false_with_null()
        {
            base.Contains_with_local_array_closure_false_with_null();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo') AND [e].[NullableStringA] IS NOT NULL");
        }

        public override void Contains_with_local_nullable_array_closure_negated()
        {
            base.Contains_with_local_nullable_array_closure_negated();

            AssertSql(
                @"");
        }

        public override void Contains_with_local_array_closure_with_multiple_nulls()
        {
            base.Contains_with_local_array_closure_with_multiple_nulls();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override void Where_multiple_ors_with_null()
        {
            base.Where_multiple_ors_with_null();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringA] = N'Foo') OR ([e].[NullableStringA] = N'Blah')) OR [e].[NullableStringA] IS NULL");
        }

        public override void Where_multiple_ands_with_null()
        {
            base.Where_multiple_ands_with_null();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL) AND (([e].[NullableStringA] <> N'Blah') OR [e].[NullableStringA] IS NULL)) AND [e].[NullableStringA] IS NOT NULL");
        }

        public override void Where_multiple_ors_with_nullable_parameter()
        {
            base.Where_multiple_ors_with_nullable_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableStringA] = N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override void Where_multiple_ands_with_nullable_parameter_and_constant()
        {
            base.Where_multiple_ands_with_nullable_parameter_and_constant();

            AssertSql(
                @"@__prm3_2='Blah' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (((([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL) AND [e].[NullableStringA] IS NOT NULL) AND [e].[NullableStringA] IS NOT NULL) AND (([e].[NullableStringA] <> @__prm3_2) OR [e].[NullableStringA] IS NULL)");
        }

        public override void Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized()
        {
            base.Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized();

            AssertSql(
                @"@__prm3_2='Blah' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((([e].[NullableStringB] IS NOT NULL AND (([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL)) AND [e].[NullableStringA] IS NOT NULL) AND [e].[NullableStringA] IS NOT NULL) AND (([e].[NullableStringA] <> @__prm3_2) OR [e].[NullableStringA] IS NULL)");
        }

        public override void Where_coalesce()
        {
            base.Where_coalesce();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE COALESCE([e].[NullableBoolA], CAST(1 AS bit)) = CAST(1 AS bit)");
        }

        public override void Where_equal_nullable_with_null_value_parameter()
        {
            base.Where_equal_nullable_with_null_value_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IS NULL");
        }

        public override void Where_not_equal_nullable_with_null_value_parameter()
        {
            base.Where_not_equal_nullable_with_null_value_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IS NOT NULL");
        }

        public override void Where_equal_with_coalesce()
        {
            base.Where_equal_with_coalesce();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (COALESCE([e].[NullableStringA], [e].[NullableStringB]) = [e].[NullableStringC]) OR (([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) AND [e].[NullableStringC] IS NULL)");
        }

        public override void Where_not_equal_with_coalesce()
        {
            base.Where_not_equal_with_coalesce();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((COALESCE([e].[NullableStringA], [e].[NullableStringB]) <> [e].[NullableStringC]) OR (([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) OR [e].[NullableStringC] IS NULL)) AND (([e].[NullableStringA] IS NOT NULL OR [e].[NullableStringB] IS NOT NULL) OR [e].[NullableStringC] IS NOT NULL)");
        }

        public override void Where_equal_with_coalesce_both_sides()
        {
            base.Where_equal_with_coalesce_both_sides();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE COALESCE([e].[NullableStringA], [e].[NullableStringB]) = COALESCE([e].[StringA], [e].[StringB])");
        }

        public override void Where_not_equal_with_coalesce_both_sides()
        {
            base.Where_not_equal_with_coalesce_both_sides();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((COALESCE([e].[NullableIntA], [e].[NullableIntB]) <> COALESCE([e].[NullableIntC], [e].[NullableIntB])) OR (([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL) OR ([e].[NullableIntC] IS NULL AND [e].[NullableIntB] IS NULL))) AND (([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL) OR ([e].[NullableIntC] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL))");
        }

        public override void Where_equal_with_conditional()
        {
            base.Where_equal_with_conditional();

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id]
//FROM [Entities1] AS [e]
//WHERE (CASE
//    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
//    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
//END = [e].[NullableStringC]) OR (CASE
//    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
//    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
//END IS NULL AND [e].[NullableStringC] IS NULL)");
        }

        public override void Where_not_equal_with_conditional()
        {
            base.Where_not_equal_with_conditional();

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

        public override void Where_equal_with_conditional_non_nullable()
        {
            base.Where_equal_with_conditional_non_nullable();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableStringC] <> CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) THEN [e].[StringA]
    ELSE [e].[StringB]
END) OR [e].[NullableStringC] IS NULL");
        }

        public override void Where_equal_with_and_and_contains()
        {
            base.Where_equal_with_and_and_contains();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringB] = N'') OR (CHARINDEX([e].[NullableStringB], [e].[NullableStringA]) > 0)) AND ([e].[BoolA] = CAST(1 AS bit))");
        }

        public override void Null_comparison_in_selector_with_relational_nulls()
        {
            base.Null_comparison_in_selector_with_relational_nulls();

            AssertSql(
                @"SELECT CASE
    WHEN [e].[NullableStringA] <> N'Foo' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Entities1] AS [e]");
        }

        public override void Null_comparison_in_order_by_with_relational_nulls()
        {
            base.Null_comparison_in_order_by_with_relational_nulls();

            AssertSql(
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
ORDER BY CASE
    WHEN [e].[NullableStringA] <> N'Foo' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, CASE
    WHEN [e].[NullableIntB] <> 10 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override void Null_comparison_in_join_key_with_relational_nulls()
        {
            base.Null_comparison_in_join_key_with_relational_nulls();

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

        public override void Where_conditional_search_condition_in_result()
        {
            base.Where_conditional_search_condition_in_result();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[StringA] IN (N'Foo', N'Bar')",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[StringA] LIKE N'A%'");
        }

        public override void Where_nested_conditional_search_condition_in_result()
        {
            base.Where_nested_conditional_search_condition_in_result();

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

        public override void Where_nullable_bool()
        {
            base.Where_nullable_bool();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = CAST(1 AS bit)");
        }

        public override void Where_nullable_bool_equal_with_constant()
        {
            base.Where_nullable_bool_equal_with_constant();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = CAST(1 AS bit)");
        }

        public override void Where_nullable_bool_with_null_check()
        {
            base.Where_nullable_bool_with_null_check();

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

        public override void Where_comparison_null_constant_and_null_parameter()
        {
            base.Where_comparison_null_constant_and_null_parameter();

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

        public override void Where_comparison_null_constant_and_nonnull_parameter()
        {
            base.Where_comparison_null_constant_and_nonnull_parameter();

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

        public override void Where_comparison_nonnull_constant_and_null_parameter()
        {
            base.Where_comparison_nonnull_constant_and_null_parameter();

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

        public override void Where_comparison_null_semantics_optimization_works_with_complex_predicates()
        {
            base.Where_comparison_null_semantics_optimization_works_with_complex_predicates();

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

        public override void Projecting_nullable_bool_with_coalesce()
        {
            base.Projecting_nullable_bool_with_coalesce();

            AssertSql(
                @"SELECT [e].[Id], COALESCE([e].[NullableBoolA], CAST(0 AS bit)) AS [Coalesce]
FROM [Entities1] AS [e]");
        }

        public override void Projecting_nullable_bool_with_coalesce_nested()
        {
            base.Projecting_nullable_bool_with_coalesce_nested();

            AssertSql(
                @"SELECT [e].[Id], COALESCE([e].[NullableBoolA], COALESCE([e].[NullableBoolB], CAST(0 AS bit))) AS [Coalesce]
FROM [Entities1] AS [e]");
        }

        public override void Null_semantics_applied_when_comparing_function_with_nullable_argument_to_a_nullable_column()
        {
            base.Null_semantics_applied_when_comparing_function_with_nullable_argument_to_a_nullable_column();

            // issue #15994
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

        public override void Null_semantics_applied_when_comparing_two_functions_with_nullable_arguments()
        {
            base.Null_semantics_applied_when_comparing_two_functions_with_nullable_arguments();

            // issue #15994
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

        public override void Null_semantics_applied_when_comparing_two_functions_with_multiple_nullable_arguments()
        {
            base.Null_semantics_applied_when_comparing_two_functions_with_multiple_nullable_arguments();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (REPLACE([e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC]) = [e].[NullableStringA]) OR (REPLACE([e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC]) IS NULL AND [e].[NullableStringA] IS NULL)",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((REPLACE([e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC]) <> [e].[NullableStringA]) OR (REPLACE([e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC]) IS NULL OR [e].[NullableStringA] IS NULL)) AND (REPLACE([e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC]) IS NOT NULL OR [e].[NullableStringA] IS NOT NULL)");
        }

        public override void Null_semantics_coalesce()
        {
            base.Null_semantics_coalesce();

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

        public override void Null_semantics_conditional()
        {
            base.Null_semantics_conditional();

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

        public override void Null_semantics_function()
        {
            base.Null_semantics_function();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((SUBSTRING([e].[NullableStringA], 0 + 1, [e].[IntA]) <> [e].[NullableStringB]) OR (SUBSTRING([e].[NullableStringA], 0 + 1, [e].[IntA]) IS NULL OR [e].[NullableStringB] IS NULL)) AND (SUBSTRING([e].[NullableStringA], 0 + 1, [e].[IntA]) IS NOT NULL OR [e].[NullableStringB] IS NOT NULL)");
        }

        public override void Null_semantics_join_with_composite_key()
        {
            base.Null_semantics_join_with_composite_key();

            // issue #15994
            //AssertSql(
            //    @"");
        }

        public override void Null_semantics_contains()
        {
            base.Null_semantics_contains();

            AssertSql(
                @"");
        }

        public override void Null_semantics_with_null_check_simple()
        {
            base.Null_semantics_with_null_check_simple();

            AssertSql(
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ([e].[NullableIntA] = [e].[NullableIntB])",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND (([e].[NullableIntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL)",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ([e].[NullableIntA] = [e].[IntC])",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL) AND ([e].[NullableIntA] = [e].[NullableIntB])",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL) AND ([e].[NullableIntA] <> [e].[NullableIntB])");
        }

        public override void Null_semantics_with_null_check_complex()
        {
            base.Null_semantics_with_null_check_complex();

            AssertSql(
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ((([e].[NullableIntC] <> [e].[NullableIntA]) OR [e].[NullableIntC] IS NULL) OR ([e].[NullableIntB] IS NOT NULL AND ([e].[NullableIntA] <> [e].[NullableIntB])))",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL AND ((([e].[NullableIntC] <> [e].[NullableIntA]) OR [e].[NullableIntC] IS NULL) OR (([e].[NullableIntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL))",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL) AND (([e].[NullableIntA] = [e].[NullableIntC]) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntC] IS NULL))");
        }

        public override void IsNull_on_complex_expression()
        {
            base.IsNull_on_complex_expression();

            AssertSql(
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL",
                //
                @"SELECT [e].[Id], [e].[BoolA], [e].[BoolB], [e].[BoolC], [e].[IntA], [e].[IntB], [e].[IntC], [e].[NullableBoolA], [e].[NullableBoolB], [e].[NullableBoolC], [e].[NullableIntA], [e].[NullableIntB], [e].[NullableIntC], [e].[NullableStringA], [e].[NullableStringB], [e].[NullableStringC], [e].[StringA], [e].[StringB], [e].[StringC]
FROM [Entities1] AS [e]
WHERE [e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

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
