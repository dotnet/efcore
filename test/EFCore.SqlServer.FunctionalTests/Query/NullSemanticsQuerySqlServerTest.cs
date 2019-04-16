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
    WHEN [e].[BoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[NullableBoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] = [e].[NullableIntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN ([e].[NullableIntA] = [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Compare_complex_equal_not_equal_equal()
        {
            base.Compare_complex_equal_not_equal_equal();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[NullableBoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] = [e].[NullableIntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN ([e].[NullableIntA] = [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Compare_complex_not_equal_equal_equal()
        {
            base.Compare_complex_not_equal_equal_equal();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] = [e].[NullableIntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Compare_complex_not_equal_not_equal_equal()
        {
            base.Compare_complex_not_equal_not_equal_equal();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] = [e].[NullableIntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Compare_complex_not_equal_equal_not_equal()
        {
            base.Compare_complex_not_equal_equal_not_equal();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] <> [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN ([e].[IntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN (([e].[NullableIntA] <> [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL)) AND ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Compare_complex_not_equal_not_equal_not_equal()
        {
            base.Compare_complex_not_equal_not_equal_not_equal();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] <> [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN ([e].[IntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN (([e].[NullableIntA] <> [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL)) AND ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
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
                @"SELECT [e1].[Id] AS [Id1], [e2].[Id] AS [Id2], [e1].[NullableIntA], [e2].[NullableIntB]
FROM [Entities1] AS [e1]
INNER JOIN [Entities2] AS [e2] ON [e1].[NullableIntA] = [e2].[NullableIntB]");
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
WHERE [e].[NullableStringA] IN (N'Foo', N'Blah') OR [e].[NullableStringA] IS NULL");
        }

        public override void Where_multiple_ands_with_null()
        {
            base.Where_multiple_ands_with_null();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo', N'Blah') AND [e].[NullableStringA] IS NOT NULL");
        }

        public override void Where_multiple_ors_with_nullable_parameter()
        {
            base.Where_multiple_ors_with_nullable_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL");
        }

        public override void Where_multiple_ands_with_nullable_parameter_and_constant()
        {
            base.Where_multiple_ands_with_nullable_parameter_and_constant();

            AssertSql(
                @"@__prm3_2='Blah' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo', @__prm3_2) AND [e].[NullableStringA] IS NOT NULL");
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
WHERE COALESCE([e].[NullableBoolA], 1) = 1");
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

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END = [e].[NullableStringC]) OR (CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END IS NULL AND [e].[NullableStringC] IS NULL)");
        }

        public override void Where_not_equal_with_conditional()
        {
            base.Where_not_equal_with_conditional();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE (([e].[NullableStringC] <> CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END) OR ([e].[NullableStringC] IS NULL OR CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END IS NULL)) AND ([e].[NullableStringC] IS NOT NULL OR CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END IS NOT NULL)");
        }

        public override void Where_equal_with_conditional_non_nullable()
        {
            base.Where_equal_with_conditional_non_nullable();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ([e].[NullableStringC] <> CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[StringA] ELSE [e].[StringB]
END) OR [e].[NullableStringC] IS NULL");
        }

        public override void Where_equal_with_and_and_contains()
        {
            base.Where_equal_with_and_and_contains();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE ((CHARINDEX([e].[NullableStringB], [e].[NullableStringA]) > 0) OR ([e].[NullableStringB] = N'')) AND ([e].[BoolA] = 1)");
        }

        public override void Null_comparison_in_selector_with_relational_nulls()
        {
            base.Null_comparison_in_selector_with_relational_nulls();

            AssertSql(
                @"SELECT CASE
    WHEN ([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
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
    WHEN ([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, CASE
    WHEN ([e].[NullableIntB] <> 10) OR [e].[NullableIntB] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Null_comparison_in_join_key_with_relational_nulls()
        {
            base.Null_comparison_in_join_key_with_relational_nulls();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[BoolA], [e1].[BoolB], [e1].[BoolC], [e1].[IntA], [e1].[IntB], [e1].[IntC], [e1].[NullableBoolA], [e1].[NullableBoolB], [e1].[NullableBoolC], [e1].[NullableIntA], [e1].[NullableIntB], [e1].[NullableIntC], [e1].[NullableStringA], [e1].[NullableStringB], [e1].[NullableStringC], [e1].[StringA], [e1].[StringB], [e1].[StringC], [i].[Id], [i].[BoolA], [i].[BoolB], [i].[BoolC], [i].[IntA], [i].[IntB], [i].[IntC], [i].[NullableBoolA], [i].[NullableBoolB], [i].[NullableBoolC], [i].[NullableIntA], [i].[NullableIntB], [i].[NullableIntC], [i].[NullableStringA], [i].[NullableStringB], [i].[NullableStringC], [i].[StringA], [i].[StringB], [i].[StringC]
FROM [Entities1] AS [e1]
INNER JOIN [Entities2] AS [i] ON CASE
    WHEN ([e1].[NullableStringA] <> N'Foo') OR [e1].[NullableStringA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN ([i].[NullableBoolB] <> 1) OR [i].[NullableBoolB] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_conditional_search_condition_in_result()
        {
            base.Where_conditional_search_condition_in_result();

            AssertSql(
                @"@__prm_0='True'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN @__prm_0 = 1
    THEN CASE
        WHEN [e].[StringA] IN (N'Foo', N'Bar')
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END ELSE CAST(0 AS BIT)
END = 1",
                //
                @"@__prm_0='True'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN @__prm_0 = 0
    THEN CAST(1 AS BIT) ELSE CASE
        WHEN [e].[StringA] LIKE N'A' + N'%' AND (LEFT([e].[StringA], LEN(N'A')) = N'A')
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
END = 1");
        }

        public override void Where_nested_conditional_search_condition_in_result()
        {
            base.Where_nested_conditional_search_condition_in_result();

            AssertSql(
                @"@__prm1_0='True'
@__prm2_1='False'

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE CASE
    WHEN @__prm1_0 = 1
    THEN CASE
        WHEN @__prm2_1 = 1
        THEN CASE
            WHEN [e].[BoolA] = 1
            THEN CASE
                WHEN [e].[StringA] LIKE N'A' + N'%' AND (LEFT([e].[StringA], LEN(N'A')) = N'A')
                THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
            END ELSE CAST(0 AS BIT)
        END ELSE CAST(1 AS BIT)
    END ELSE CASE
        WHEN [e].[BoolB] = 1
        THEN CASE
            WHEN [e].[StringA] IN (N'Foo', N'Bar')
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END ELSE CASE
            WHEN [e].[StringB] IN (N'Foo', N'Bar')
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END
    END
END = 1");
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
WHERE [e].[NullableBoolA] = 1");
        }

        public override void Where_nullable_bool_equal_with_constant()
        {
            base.Where_nullable_bool_equal_with_constant();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = 1");
        }

        public override void Where_nullable_bool_with_null_check()
        {
            base.Where_nullable_bool_with_null_check();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] IS NOT NULL AND ([e].[NullableBoolA] = 1)");
        }

        public override void Where_equal_using_relational_null_semantics_with_parameter()
        {
            base.Where_equal_using_relational_null_semantics_with_parameter();

            AssertSql(
                @"@__prm_0=''

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] = @__prm_0");
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
                @"@__prm_0=''

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE [e].[NullableBoolA] <> @__prm_0");
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
                @"@__prm_0='' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__prm_0 IS NULL",
                //
                @"@__prm_0='' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__prm_0 IS NOT NULL");
        }

        public override void Where_comparison_null_constant_and_nonnull_parameter()
        {
            base.Where_comparison_null_constant_and_nonnull_parameter();

            AssertSql(
                @"@__prm_0='Foo' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__prm_0 IS NULL",
                //
                @"@__prm_0='Foo' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__prm_0 IS NOT NULL");
        }

        public override void Where_comparison_nonnull_constant_and_null_parameter()
        {
            base.Where_comparison_nonnull_constant_and_null_parameter();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]");
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
                @"@__prm_0='Foo' (Size = 4000)

SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE @__prm_0 = N'Foo'",
                //
                @"SELECT [e].[Id]
FROM [Entities1] AS [e]
WHERE 0 = 1");
        }

        public override void From_sql_composed_with_relational_null_comparison()
        {
            base.From_sql_composed_with_relational_null_comparison();

            AssertSql(
                @"SELECT [c].[Id], [c].[BoolA], [c].[BoolB], [c].[BoolC], [c].[IntA], [c].[IntB], [c].[IntC], [c].[NullableBoolA], [c].[NullableBoolB], [c].[NullableBoolC], [c].[NullableIntA], [c].[NullableIntB], [c].[NullableIntC], [c].[NullableStringA], [c].[NullableStringB], [c].[NullableStringC], [c].[StringA], [c].[StringB], [c].[StringC]
FROM (
    SELECT * FROM ""Entities1""
) AS [c]
WHERE [c].[StringA] = [c].[StringB]");
        }

        public override void Projecting_nullable_bool_with_coalesce()
        {
            base.Projecting_nullable_bool_with_coalesce();

            AssertSql(
                @"SELECT [e].[Id], CAST(COALESCE([e].[NullableBoolA], 0) AS bit) AS [Coalesce]
FROM [Entities1] AS [e]");
        }

        public override void Projecting_nullable_bool_with_coalesce_nested()
        {
            base.Projecting_nullable_bool_with_coalesce_nested();

            AssertSql(
                @"SELECT [e].[Id], CAST(COALESCE([e].[NullableBoolA], COALESCE([e].[NullableBoolB], 0)) AS bit) AS [Coalesce]
FROM [Entities1] AS [e]");
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
