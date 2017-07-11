// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LoadSqlServerTest
        : LoadTestBase<SqlServerTestStore, LoadSqlServerTest.LoadSqlServerFixture>
    {
        public LoadSqlServerTest(LoadSqlServerFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override async Task Load_collection(EntityState state, bool async)
        {
            await base.Load_collection(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id]
FROM [SinglePkToPks] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_using_Query(EntityState state, bool async)
        {
            await base.Load_collection_using_Query(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id]
FROM [SinglePkToPks] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override async Task Load_collection_not_found(EntityState state, bool async)
        {
            await base.Load_collection_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_not_found(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_already_loaded(EntityState state, bool async)
        {
            await base.Load_collection_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_already_loaded(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_collection_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id]
FROM [SinglePkToPks] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_untyped(EntityState state, bool async)
        {
            await base.Load_collection_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_collection_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='787'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='767' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_collection_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_collection_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Children] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [Singles] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_alternate_key(EntityState state, bool async)
        {
            await base.Load_collection_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildrenAks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT [e].[Id], [e].[ParentId]
FROM [SingleAks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildrenAks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[AlternateId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [SingleAks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override async Task Load_collection_shadow_fk(EntityState state, bool async)
        {
            await base.Load_collection_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildrenShadowFks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [SingleShadowFks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_collection_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentId]
FROM [ChildrenShadowFks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE [e].[Id] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='707' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentId]
FROM [SingleShadowFks] AS [e]
WHERE [e].[ParentId] = @__get_Item_0",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override async Task Load_collection_composite_key(EntityState state, bool async)
        {
            await base.Load_collection_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [ChildrenCompositeKeys] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707'

SELECT [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [SingleCompositeKeys] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_collection_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707' (Nullable = true)

SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [ChildrenCompositeKeys] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707'

SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE ([e].[AlternateId] = @__get_Item_0) AND ([e].[Id] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__get_Item_0='Root' (Size = 450)
@__get_Item_1='707' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
FROM [SingleCompositeKeys] AS [e]
WHERE ([e].[ParentAlternateId] = @__get_Item_0) AND ([e].[ParentId] = @__get_Item_1)",
                    Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_composite_key(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_composite_key(state, async);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
FROM [Parents] AS [e]
WHERE 0 = 1",
                    Sql);
            }
        }

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private const string FileLineEnding = @"
";

        private string Sql { get; set; }

        public class LoadSqlServerFixture : LoadFixtureBase
        {
            private const string DatabaseName = "LoadTest";
            private readonly DbContextOptions _options;

            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public LoadSqlServerFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                    .BuildServiceProvider(validateScopes: true);

                _options = new DbContextOptionsBuilder()
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(serviceProvider)
                    .EnableSensitiveDataLogging()
                    .Options;
            }

            public override SqlServerTestStore CreateTestStore()
            {
                return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        using (var context = new LoadContext(_options))
                        {
                            context.Database.EnsureCreated();
                            Seed(context);
                        }
                    });
            }

            public override DbContext CreateContext(SqlServerTestStore testStore)
                => new LoadContext(_options);
        }
    }
}
