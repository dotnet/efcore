// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class LoadSqlServerTest
        : LoadTestBase<LoadSqlServerTest.LoadSqlServerFixture>
    {
        public LoadSqlServerTest(LoadSqlServerFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        public override async Task Load_collection(bool async)
        {
            await base.Load_collection(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal(bool async)
        {
            await base.Load_many_to_one_reference_to_principal(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal(bool async)
        {
            await base.Load_one_to_one_reference_to_principal(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id]
FROM [SinglePkToPk] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query(bool async)
        {
            await base.Load_collection_using_Query(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id]
FROM [SinglePkToPk] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_null_FK(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_null_FK(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_not_found(bool async)
        {
            await base.Load_collection_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_not_found(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_not_found(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_not_found(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_not_found(bool async)
        {
            await base.Load_collection_using_Query_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_already_loaded(bool async)
        {
            await base.Load_collection_already_loaded(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_already_loaded(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_already_loaded(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_already_loaded(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_already_loaded(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_already_loaded(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_already_loaded(bool async)
        {
            await base.Load_collection_using_Query_already_loaded(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id]
FROM [SinglePkToPk] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_untyped(bool async)
        {
            await base.Load_collection_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_untyped(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_untyped(bool async)
        {
            await base.Load_collection_using_Query_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_untyped(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_not_found_untyped(bool async)
        {
            await base.Load_collection_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_not_found_untyped(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_not_found_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_not_found_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_not_found_untyped(bool async)
        {
            await base.Load_collection_using_Query_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 787

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 767 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_already_loaded_untyped(bool async)
        {
            await base.Load_collection_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_already_loaded_untyped(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_already_loaded_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_already_loaded_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_already_loaded_untyped(bool async)
        {
            await base.Load_collection_using_Query_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Child] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Single] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_alternate_key(bool async)
        {
            await base.Load_collection_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildAk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_alternate_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_alternate_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_alternate_key(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT [e].[Id], [e].[ParentId]
FROM [SingleAk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_alternate_key(bool async)
        {
            await base.Load_collection_using_Query_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildAk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_alternate_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_alternate_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_alternate_key(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [SingleAk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_null_FK_alternate_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_alternate_key(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_null_FK_alternate_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_alternate_key(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_shadow_fk(bool async)
        {
            await base.Load_collection_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildShadowFk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_shadow_fk(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_shadow_fk(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_shadow_fk(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [SingleShadowFk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_shadow_fk(bool async)
        {
            await base.Load_collection_using_Query_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildShadowFk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: 707 (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [SingleShadowFk] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_null_FK_shadow_fk(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_shadow_fk(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_null_FK_shadow_fk(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_shadow_fk(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_composite_key(bool async)
        {
            await base.Load_collection_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [ChildCompositeKey] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_composite_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_composite_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707

SELECT [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_composite_key(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [SingleCompositeKey] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_collection_using_Query_composite_key(bool async)
        {
            await base.Load_collection_using_Query_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707 (Nullable = true)

SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [ChildCompositeKey] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_composite_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_composite_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_dependent_using_Query_composite_key(bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0: Root (Size = 450)
@__get_Item_1: 707 (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [SingleCompositeKey] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_composite_key(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_composite_key(async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        [Theory]
        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        [Theory]
        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parent] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override void Dispose() => TestSqlLoggerFactory.Reset();

        public override void ClearLog() => TestSqlLoggerFactory.Reset();

        public override void RecordLog() => Sql = TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private const string FileLineEnding = @"
";

        private static string Sql { get; set; }

        public class LoadSqlServerFixture : LoadFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            public LoadSqlServerFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory, TestSqlLoggerFactory>()
                    .BuildServiceProvider();
            }

            public override void CreateTestStore()
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureClean();
                    Seed(context);
                    TestSqlLoggerFactory.Reset();
                }
            }

            public override DbContext CreateContext()
                => new LoadContext(new DbContextOptionsBuilder()
                    .EnableSensitiveDataLogging()
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("LoadTest"))
                    .UseInternalServiceProvider(_serviceProvider)
                    .Options);
        }
    }
}
