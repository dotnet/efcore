// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LoadSqlServerTest : LoadTestBase<LoadSqlServerTest.LoadSqlServerFixture>
    {
        public LoadSqlServerTest(LoadSqlServerFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Lazy_load_collection(EntityState state)
        {
            base.Lazy_load_collection(state);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override void Lazy_load_many_to_one_reference_to_principal(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal(state);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_principal(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal(state);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_dependent(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent(state);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal(state);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(state);

            AssertSql(
                @"@__p_0='707'

SELECT [s].[Id]
FROM [SinglePkToPk] AS [s]
WHERE [s].[Id] = @__p_0");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK(state);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK(state);

            AssertSql("");
        }

        public override void Lazy_load_collection_not_found(EntityState state)
        {
            base.Lazy_load_collection_not_found(state);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_not_found(state);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_not_found(state);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_not_found(state);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_collection_already_loaded(state, cascadeDeleteTiming);

            AssertSql("");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_already_loaded(state);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_already_loaded(state);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_already_loaded(
            EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_already_loaded(state, cascadeDeleteTiming);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(state);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(state);

            AssertSql("");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_alternate_key(state);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_alternate_key(state);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_alternate_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_alternate_key(state);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [s].[Id], [s].[ParentId]
FROM [SingleAk] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(state);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(state);

            AssertSql("");
        }

        public override void Lazy_load_collection_shadow_fk(EntityState state)
        {
            base.Lazy_load_collection_shadow_fk(state);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [ChildShadowFk] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_shadow_fk(state);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_shadow_fk(state);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_shadow_fk(state);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [SingleShadowFk] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(state);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(state);

            AssertSql("");
        }

        public override void Lazy_load_collection_composite_key(EntityState state)
        {
            base.Lazy_load_collection_composite_key(state);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentAlternateId], [c].[ParentId]
FROM [ChildCompositeKey] AS [c]
WHERE ([c].[ParentAlternateId] = @__p_0) AND ([c].[ParentId] = @__p_1)");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_composite_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_composite_key(state);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_composite_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_composite_key(state);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)");
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_composite_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_composite_key(state);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentAlternateId], [s].[ParentId]
FROM [SingleCompositeKey] AS [s]
WHERE ([s].[ParentAlternateId] = @__p_0) AND ([s].[ParentId] = @__p_1)");
        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(state);

            AssertSql("");
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(state);

            AssertSql("");
        }

        public override async Task Load_collection(EntityState state, bool async)
        {
            await base.Load_collection(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [s].[Id]
FROM [SinglePkToPk] AS [s]
WHERE [s].[Id] = @__p_0");
        }

        public override async Task Load_collection_using_Query(EntityState state, bool async)
        {
            await base.Load_collection_using_Query(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT TOP(2) [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [s].[Id]
FROM [SinglePkToPk] AS [s]
WHERE [s].[Id] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK(state, async);

            AssertSql("");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        public override async Task Load_collection_not_found(EntityState state, bool async)
        {
            await base.Load_collection_not_found(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_not_found(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_not_found(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_not_found(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_not_found(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_not_found(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_not_found(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_not_found(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT TOP(2) [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_already_loaded(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_collection_already_loaded(state, async, cascadeDeleteTiming);

            AssertSql("");
        }

        public override async Task Load_many_to_one_reference_to_principal_already_loaded(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_already_loaded(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_principal_already_loaded(
            EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_principal_already_loaded(state, async, cascadeDeleteTiming);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_dependent_already_loaded(
            EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded(state, async, cascadeDeleteTiming);

            AssertSql("");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(state, async);

            AssertSql("");
        }

        public override async Task Load_collection_using_Query_already_loaded(
            EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_collection_using_Query_already_loaded(state, async, cascadeDeleteTiming);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded(
            EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded(state, async, cascadeDeleteTiming);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT TOP(2) [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async)
        {
            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [s].[Id]
FROM [SinglePkToPk] AS [s]
WHERE [s].[Id] = @__p_0");
        }

        public override async Task Load_collection_untyped(EntityState state, bool async)
        {
            await base.Load_collection_untyped(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_untyped(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_untyped(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_untyped(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_untyped(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_untyped(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_untyped(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_untyped(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_collection_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(state, async);

            AssertSql(
                @"@__p_0='767' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_already_loaded_untyped(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_collection_already_loaded_untyped(state, async, cascadeDeleteTiming);

            AssertSql("");
        }

        public override async Task Load_many_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_already_loaded_untyped(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_already_loaded_untyped(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_dependent_already_loaded_untyped(
            EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded_untyped(state, async, cascadeDeleteTiming);

            AssertSql("");
        }

        public override async Task Load_collection_using_Query_already_loaded_untyped(
            EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_collection_using_Query_already_loaded_untyped(state, async, cascadeDeleteTiming);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(
            EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(state, async, cascadeDeleteTiming);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_alternate_key(EntityState state, bool async)
        {
            await base.Load_collection_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [c].[Id], [c].[ParentId]
FROM [ChildAk] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [s].[Id], [s].[ParentId]
FROM [SingleAk] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT [c].[Id], [c].[ParentId]
FROM [ChildAk] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_alternate_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)

SELECT TOP(2) [s].[Id], [s].[ParentId]
FROM [SingleAk] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_alternate_key(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_alternate_key(state, async);

            AssertSql("");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        public override async Task Load_collection_shadow_fk(EntityState state, bool async)
        {
            await base.Load_collection_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [ChildShadowFk] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [SingleShadowFk] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_collection_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [ChildShadowFk] AS [c]
WHERE [c].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(state, async);

            AssertSql(
                @"@__p_0='707' (Nullable = true)

SELECT TOP(2) [s].[Id], [s].[ParentId]
FROM [SingleShadowFk] AS [s]
WHERE [s].[ParentId] = @__p_0");
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_shadow_fk(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_shadow_fk(state, async);

            AssertSql("");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        public override async Task Load_collection_composite_key(EntityState state, bool async)
        {
            await base.Load_collection_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentAlternateId], [c].[ParentId]
FROM [ChildCompositeKey] AS [c]
WHERE ([c].[ParentAlternateId] = @__p_0) AND ([c].[ParentId] = @__p_1)");
        }

        public override async Task Load_many_to_one_reference_to_principal_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)");
        }

        public override async Task Load_one_to_one_reference_to_principal_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)");
        }

        public override async Task Load_one_to_one_reference_to_dependent_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentAlternateId], [s].[ParentId]
FROM [SingleCompositeKey] AS [s]
WHERE ([s].[ParentAlternateId] = @__p_0) AND ([s].[ParentId] = @__p_1)");
        }

        public override async Task Load_collection_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_collection_using_Query_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentAlternateId], [c].[ParentId]
FROM [ChildCompositeKey] AS [c]
WHERE ([c].[ParentAlternateId] = @__p_0) AND ([c].[ParentId] = @__p_1)");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)");
        }

        public override async Task Load_one_to_one_reference_to_dependent_using_Query_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_dependent_using_Query_composite_key(state, async);

            AssertSql(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT TOP(2) [s].[Id], [s].[ParentAlternateId], [s].[ParentId]
FROM [SingleCompositeKey] AS [s]
WHERE ([s].[ParentAlternateId] = @__p_0) AND ([s].[ParentId] = @__p_1)");
        }

        public override async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_null_FK_composite_key(state, async);

            AssertSql("");
        }

        public override async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_null_FK_composite_key(state, async);

            AssertSql("");
        }

        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
        {
            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(state, async);

            AssertSql(
                @"SELECT TOP(2) [p].[Id], [p].[AlternateId]
FROM [Parent] AS [p]
WHERE 0 = 1");
        }

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql;

        private const string FileNewLine = @"
";

        private void AssertSql(string expected)
        {
            try
            {
                Assert.Equal(
                    expected,
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
            catch
            {
                var methodCallLine = Environment.StackTrace.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries)[2].Substring(6);

                var testName = methodCallLine.Substring(0, methodCallLine.IndexOf(')') + 1);
                var lineIndex = methodCallLine.LastIndexOf("line", StringComparison.Ordinal);
                var lineNumber = lineIndex > 0 ? methodCallLine.Substring(lineIndex) : "";

                var currentDirectory = Directory.GetCurrentDirectory();
                var logFile = currentDirectory.Substring(
                        0,
                        currentDirectory.LastIndexOf("\\artifacts\\", StringComparison.Ordinal) + 1)
                    + "QueryBaseline.txt";

                var testInfo = testName + " : " + lineNumber + FileNewLine;

                var newBaseLine = $@"            AssertSql(
                {"@\"" + Sql.Replace("\"", "\"\"") + "\""});

";

                var contents = testInfo + newBaseLine + FileNewLine + FileNewLine;

                File.AppendAllText(logFile, contents);

                throw;
            }
        }

        private string Sql { get; set; }

        public class LoadSqlServerFixture : LoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
