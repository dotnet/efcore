// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NullSemanticsQuerySqlServerTest : NullSemanticsQueryTestBase<SqlServerTestStore, NullSemanticsQuerySqlServerFixture>
    {
        public NullSemanticsQuerySqlServerTest(NullSemanticsQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override void Compare_bool_with_bool_equal()
        {
            base.Compare_bool_with_bool_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[NullableBoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_bool_equal()
        {
            base.Compare_negated_bool_with_bool_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_bool_with_negated_bool_equal()
        {
            base.Compare_bool_with_negated_bool_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_negated_bool_equal()
        {
            base.Compare_negated_bool_with_negated_bool_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_bool_with_bool_equal_negated()
        {
            base.Compare_bool_with_bool_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_bool_equal_negated()
        {
            base.Compare_negated_bool_with_bool_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_bool_with_negated_bool_equal_negated()
        {
            base.Compare_bool_with_negated_bool_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_negated_bool_equal_negated()
        {
            base.Compare_negated_bool_with_negated_bool_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_bool_with_bool_not_equal()
        {
            base.Compare_bool_with_bool_not_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_bool_not_equal()
        {
            base.Compare_negated_bool_with_bool_not_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_bool_with_negated_bool_not_equal()
        {
            base.Compare_bool_with_negated_bool_not_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_negated_bool_not_equal()
        {
            base.Compare_negated_bool_with_negated_bool_not_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_bool_with_bool_not_equal_negated()
        {
            base.Compare_bool_with_bool_not_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[NullableBoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_bool_not_equal_negated()
        {
            base.Compare_negated_bool_with_bool_not_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_bool_with_negated_bool_not_equal_negated()
        {
            base.Compare_bool_with_negated_bool_not_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_negated_bool_with_negated_bool_not_equal_negated()
        {
            base.Compare_negated_bool_with_negated_bool_not_equal_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] = [e].[NullableBoolB]) AND [e].[NullableBoolB] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[BoolB]) AND [e].[NullableBoolA] IS NOT NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] = [e].[NullableBoolB]) AND ([e].[NullableBoolA] IS NOT NULL AND [e].[NullableBoolB] IS NOT NULL)) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_equals_method()
        {
            base.Compare_equals_method();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] = [e].[NullableBoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] = [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)",
                Sql);
        }

        public override void Compare_equals_method_negated()
        {
            base.Compare_equals_method_negated();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[BoolA] <> [e].[BoolB]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[BoolA] <> [e].[NullableBoolB]) OR [e].[NullableBoolB] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)",
                Sql);
        }

        public override void Compare_complex_equal_equal_equal()
        {
            base.Compare_complex_equal_equal_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[NullableBoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] = [e].[NullableIntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN ([e].[NullableIntA] = [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Compare_complex_equal_not_equal_equal()
        {
            base.Compare_complex_equal_not_equal_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[NullableBoolA] = [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] = [e].[NullableIntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN ([e].[NullableIntA] = [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Compare_complex_not_equal_equal_equal()
        {
            base.Compare_complex_not_equal_equal_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN ([e].[IntA] = [e].[NullableIntB]) AND [e].[NullableIntB] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Compare_complex_not_equal_not_equal_equal()
        {
            base.Compare_complex_not_equal_not_equal_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] = [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN ([e].[IntA] = [e].[NullableIntB]) AND [e].[NullableIntB] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN (([e].[NullableIntA] = [e].[NullableIntB]) AND ([e].[NullableIntA] IS NOT NULL AND [e].[NullableIntB] IS NOT NULL)) OR ([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Compare_complex_not_equal_equal_not_equal()
        {
            base.Compare_complex_not_equal_equal_not_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [e].[IntA] <> [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN ([e].[IntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN (([e].[NullableIntA] <> [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL)) AND ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Compare_complex_not_equal_not_equal_not_equal()
        {
            base.Compare_complex_not_equal_not_equal_not_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN [e].[BoolA] <> [e].[BoolB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN [e].[IntA] <> [e].[IntB]
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN ([e].[NullableBoolA] <> [e].[BoolB]) OR [e].[NullableBoolA] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN ([e].[IntA] <> [e].[NullableIntB]) OR [e].[NullableIntB] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE CASE
    WHEN (([e].[NullableBoolA] <> [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL OR [e].[NullableBoolB] IS NULL)) AND ([e].[NullableBoolA] IS NOT NULL OR [e].[NullableBoolB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> CASE
    WHEN (([e].[NullableIntA] <> [e].[NullableIntB]) OR ([e].[NullableIntA] IS NULL OR [e].[NullableIntB] IS NULL)) AND ([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Compare_nullable_with_null_parameter_equal()
        {
            base.Compare_nullable_with_null_parameter_equal();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IS NULL",
                Sql);
        }

        public override void Compare_nullable_with_non_null_parameter_not_equal()
        {
            base.Compare_nullable_with_non_null_parameter_not_equal();

            Assert.Equal(
                @"@__prm_0: Foo (Size = 4000)

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] = @__prm_0",
                Sql);
        }

        public override void Join_uses_database_semantics()
        {
            base.Join_uses_database_semantics();

            Assert.Equal(
                @"SELECT [e1].[Id], [e2].[Id], [e1].[NullableIntA], [e2].[NullableIntB]
FROM [NullSemanticsEntity1] AS [e1]
INNER JOIN [NullSemanticsEntity2] AS [e2] ON [e1].[NullableIntA] = [e2].[NullableIntB]",
                Sql);
        }

        public override void Contains_with_local_array_closure_with_null()
        {
            base.Contains_with_local_array_closure_with_null();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL",
                Sql);
        }

        public override void Contains_with_local_array_closure_false_with_null()
        {
            base.Contains_with_local_array_closure_false_with_null();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo') AND [e].[NullableStringA] IS NOT NULL",
                Sql);
        }

        public override void Contains_with_local_array_closure_with_multiple_nulls()
        {
            base.Contains_with_local_array_closure_with_multiple_nulls();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL",
                Sql);
        }

        public override void Where_multiple_ors_with_null()
        {
            base.Where_multiple_ors_with_null();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo', N'Blah') OR [e].[NullableStringA] IS NULL",
                Sql);
        }

        public override void Where_multiple_ands_with_null()
        {
            base.Where_multiple_ands_with_null();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo', N'Blah') AND [e].[NullableStringA] IS NOT NULL",
                Sql);
        }

        public override void Where_multiple_ors_with_nullable_parameter()
        {
            base.Where_multiple_ors_with_nullable_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IN (N'Foo') OR [e].[NullableStringA] IS NULL",
                Sql);
        }

        public override void Where_multiple_ands_with_nullable_parameter_and_constant()
        {
            base.Where_multiple_ands_with_nullable_parameter_and_constant();

            Assert.Equal(
                @"@__prm3_2: Blah (Size = 4000)

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] NOT IN (N'Foo', @__prm3_2) AND [e].[NullableStringA] IS NOT NULL",
                Sql);
        }

        public override void Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized()
        {
            base.Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized();

            Assert.Equal(
                @"@__prm3_2: Blah (Size = 4000)

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ((([e].[NullableStringB] IS NOT NULL AND (([e].[NullableStringA] <> N'Foo') OR [e].[NullableStringA] IS NULL)) AND [e].[NullableStringA] IS NOT NULL) AND [e].[NullableStringA] IS NOT NULL) AND (([e].[NullableStringA] <> @__prm3_2) OR [e].[NullableStringA] IS NULL)",
                Sql);
        }

        public override void Where_equal_nullable_with_null_value_parameter()
        {
            base.Where_equal_nullable_with_null_value_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IS NULL",
                Sql);
        }

        public override void Where_not_equal_nullable_with_null_value_parameter()
        {
            base.Where_not_equal_nullable_with_null_value_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IS NOT NULL",
                Sql);
        }

        public override void Where_equal_with_coalesce()
        {
            base.Where_equal_with_coalesce();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (COALESCE([e].[NullableStringA], [e].[NullableStringB]) = [e].[NullableStringC]) OR (([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) AND [e].[NullableStringC] IS NULL)",
                Sql);
        }

        public override void Where_not_equal_with_coalesce()
        {
            base.Where_not_equal_with_coalesce();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ((COALESCE([e].[NullableStringA], [e].[NullableStringB]) <> [e].[NullableStringC]) OR (([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL) OR [e].[NullableStringC] IS NULL)) AND (([e].[NullableStringA] IS NOT NULL OR [e].[NullableStringB] IS NOT NULL) OR [e].[NullableStringC] IS NOT NULL)",
                Sql);
        }

        public override void Where_equal_with_coalesce_both_sides()
        {
            base.Where_equal_with_coalesce_both_sides();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE COALESCE([e].[NullableStringA], [e].[NullableStringB]) = COALESCE([e].[StringA], [e].[StringB])",
                Sql);
        }

        public override void Where_not_equal_with_coalesce_both_sides()
        {
            base.Where_not_equal_with_coalesce_both_sides();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ((COALESCE([e].[NullableIntA], [e].[NullableIntB]) <> COALESCE([e].[NullableIntC], [e].[NullableIntB])) OR (([e].[NullableIntA] IS NULL AND [e].[NullableIntB] IS NULL) OR ([e].[NullableIntC] IS NULL AND [e].[NullableIntB] IS NULL))) AND (([e].[NullableIntA] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL) OR ([e].[NullableIntC] IS NOT NULL OR [e].[NullableIntB] IS NOT NULL))",
                Sql);
        }

        public override void Where_equal_with_conditional()
        {
            base.Where_equal_with_conditional();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END = [e].[NullableStringC]) OR (CASE
    WHEN ([e].[NullableStringA] = [e].[NullableStringB]) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END IS NULL AND [e].[NullableStringC] IS NULL)",
                Sql);
        }

        public override void Where_not_equal_with_conditional()
        {
            base.Where_not_equal_with_conditional();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE (([e].[NullableStringC] <> CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END) OR ([e].[NullableStringC] IS NULL OR CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END IS NULL)) AND ([e].[NullableStringC] IS NOT NULL OR CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[NullableStringA] ELSE [e].[NullableStringB]
END IS NOT NULL)",
                Sql);
        }

        public override void Where_equal_with_conditional_non_nullable()
        {
            base.Where_equal_with_conditional_non_nullable();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableStringC] <> CASE
    WHEN (([e].[NullableStringA] = [e].[NullableStringB]) AND ([e].[NullableStringA] IS NOT NULL AND [e].[NullableStringB] IS NOT NULL)) OR ([e].[NullableStringA] IS NULL AND [e].[NullableStringB] IS NULL)
    THEN [e].[StringA] ELSE [e].[StringB]
END) OR [e].[NullableStringC] IS NULL",
                Sql);
        }

        public override void Where_equal_with_and_and_contains()
        {
            base.Where_equal_with_and_and_contains();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] LIKE (N'%' + [e].[NullableStringB]) + N'%' AND ([e].[BoolA] = 1)",
                Sql);
        }

        public override void Where_equal_using_relational_null_semantics()
        {
            base.Where_equal_using_relational_null_semantics();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] = [e].[NullableBoolB]",
                Sql);
        }

        public override void Where_nullable_bool()
        {
            base.Where_nullable_bool();

            Assert.Equal(
    @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] = 1",
                Sql);
        }

        public override void Where_nullable_bool_equal_with_constant()
        {
            base.Where_nullable_bool_equal_with_constant();

            Assert.Equal(
    @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] = 1",
                Sql);
        }

        public override void Where_nullable_bool_with_null_check()
        {
            base.Where_nullable_bool_with_null_check();

            Assert.Equal(
    @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] IS NOT NULL AND ([e].[NullableBoolA] = 1)",
                Sql);
        }

        public override void Where_equal_using_relational_null_semantics_with_parameter()
        {
            base.Where_equal_using_relational_null_semantics_with_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] IS NULL",
                Sql);
        }

        public override void Where_equal_using_relational_null_semantics_complex_with_parameter()
        {
            base.Where_equal_using_relational_null_semantics_complex_with_parameter();

            Assert.Equal(
                @"@__prm_0: False

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR (@__prm_0 = 1)",
                Sql);
        }

        public override void Where_not_equal_using_relational_null_semantics()
        {
            base.Where_not_equal_using_relational_null_semantics();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] <> [e].[NullableBoolB]",
                Sql);
        }

        public override void Where_not_equal_using_relational_null_semantics_with_parameter()
        {
            base.Where_not_equal_using_relational_null_semantics_with_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] IS NOT NULL",
                Sql);
        }

        public override void Where_not_equal_using_relational_null_semantics_complex_with_parameter()
        {
            base.Where_not_equal_using_relational_null_semantics_complex_with_parameter();

            Assert.Equal(
                @"@__prm_0: False

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] <> [e].[NullableBoolB]) OR (@__prm_0 = 1)",
                Sql);
        }

        public override void Where_comparison_null_constant_and_null_parameter()
        {
            base.Where_comparison_null_constant_and_null_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE 0 = 1",
                Sql);
        }

        public override void Where_comparison_null_constant_and_nonnull_parameter()
        {
            base.Where_comparison_null_constant_and_nonnull_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE 0 = 1

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]",
                Sql);
        }

        public override void Where_comparison_nonnull_constant_and_null_parameter()
        {
            base.Where_comparison_nonnull_constant_and_null_parameter();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE 0 = 1

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]",
                Sql);

        }
        public override void Where_comparison_null_semantics_optimization_works_with_complex_predicates()
        {
            base.Where_comparison_null_semantics_optimization_works_with_complex_predicates();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableStringA] IS NULL",
                Sql);
        }

        public override void Switching_null_semantics_produces_different_cache_entry()
        {
            base.Switching_null_semantics_produces_different_cache_entry();

            Assert.Equal(
                @"SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE ([e].[NullableBoolA] = [e].[NullableBoolB]) OR ([e].[NullableBoolA] IS NULL AND [e].[NullableBoolB] IS NULL)

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE [e].[NullableBoolA] = [e].[NullableBoolB]",
                Sql);
        }

        public override void Switching_parameter_value_to_null_produces_different_cache_entry()
        {
            base.Switching_parameter_value_to_null_produces_different_cache_entry();

            Assert.Equal(
                @"@__prm_0: Foo (Size = 4000)

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE @__prm_0 = N'Foo'

SELECT [e].[Id]
FROM [NullSemanticsEntity1] AS [e]
WHERE 0 = 1",
                Sql);

        }

        public override void From_sql_composed_with_relational_null_comparison()
        {
            base.From_sql_composed_with_relational_null_comparison();

            Assert.Equal(
                @"SELECT [c].[Id], [c].[BoolA], [c].[BoolB], [c].[BoolC], [c].[IntA], [c].[IntB], [c].[IntC], [c].[NullableBoolA], [c].[NullableBoolB], [c].[NullableBoolC], [c].[NullableIntA], [c].[NullableIntB], [c].[NullableIntC], [c].[NullableStringA], [c].[NullableStringB], [c].[NullableStringC], [c].[StringA], [c].[StringB], [c].[StringC]
FROM (
    SELECT * FROM ""NullSemanticsEntity1""
) AS [c]
WHERE [c].[StringA] = [c].[StringB]",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
