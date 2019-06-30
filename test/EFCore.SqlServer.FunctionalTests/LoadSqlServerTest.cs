// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    // Re-enable baselines: Issue #16359
    public class LoadSqlServerTest : LoadTestBase<LoadSqlServerTest.LoadSqlServerFixture>
    {
        public LoadSqlServerTest(LoadSqlServerFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

//        public override void Lazy_load_collection(EntityState state)
//        {
//            base.Lazy_load_collection(state);

//            Assert.Equal(
//                @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_many_to_one_reference_to_principal(EntityState state)
//        {
//            base.Lazy_load_many_to_one_reference_to_principal(state);

//            Assert.Equal(
//                @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_principal(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_principal(state);

//            Assert.Equal(
//                @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_dependent(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_dependent(state);

//            Assert.Equal(
//                @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
//        {
//            base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal(state);

//            Assert.Equal(
//                @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
//        {
//            base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(state);

//            Assert.Equal(
//                @"@__p_0='707'

//SELECT [e].[Id]
//FROM [SinglePkToPk] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
//        {
//            base.Lazy_load_many_to_one_reference_to_principal_null_FK(state);

//            Assert.Equal("", Sql);
//        }

//        public override void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_principal_null_FK(state);

//            Assert.Equal("", Sql);
//        }

//        public override void Lazy_load_collection_not_found(EntityState state)
//        {
//            base.Lazy_load_collection_not_found(state);

//            Assert.Equal(
//                @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
//        {
//            base.Lazy_load_many_to_one_reference_to_principal_not_found(state);

//            Assert.Equal(
//                @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_principal_not_found(state);

//            Assert.Equal(
//                @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_dependent_not_found(state);

//            Assert.Equal(
//                @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

        public override void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_collection_already_loaded(state, cascadeDeleteTiming);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_many_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_already_loaded(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_already_loaded(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_already_loaded(state, cascadeDeleteTiming);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(state);

            Assert.Equal("", Sql);
        }

//        public override void Lazy_load_many_to_one_reference_to_principal_alternate_key(EntityState state)
//        {
//            base.Lazy_load_many_to_one_reference_to_principal_alternate_key(state);

//            Assert.Equal(
//                @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[AlternateId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_principal_alternate_key(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_principal_alternate_key(state);

//            Assert.Equal(
//                @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[AlternateId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_dependent_alternate_key(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_dependent_alternate_key(state);

//            Assert.Equal(
//                @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[ParentId]
//FROM [SingleAk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(state);

            Assert.Equal("", Sql);
        }

//        public override void Lazy_load_collection_shadow_fk(EntityState state)
//        {
//            base.Lazy_load_collection_shadow_fk(state);

//            Assert.Equal(
//                @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [ChildShadowFk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_many_to_one_reference_to_principal_shadow_fk(EntityState state)
//        {
//            base.Lazy_load_many_to_one_reference_to_principal_shadow_fk(state);

//            Assert.Equal(
//                @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_principal_shadow_fk(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_principal_shadow_fk(state);

//            Assert.Equal(
//                @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_dependent_shadow_fk(state);

//            Assert.Equal(
//                @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [SingleShadowFk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(state);

            Assert.Equal("", Sql);
        }

//        public override void Lazy_load_collection_composite_key(EntityState state)
//        {
//            base.Lazy_load_collection_composite_key(state);

//            Assert.Equal(
//                @"@__p_0='Root' (Size = 450)
//@__p_1='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
//FROM [ChildCompositeKey] AS [e]
//WHERE ([e].[ParentAlternateId] = @__p_0) AND ([e].[ParentId] = @__p_1)",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_many_to_one_reference_to_principal_composite_key(EntityState state)
//        {
//            base.Lazy_load_many_to_one_reference_to_principal_composite_key(state);

//            Assert.Equal(
//                @"@__p_0='Root' (Size = 450)
//@__p_1='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE ([e].[AlternateId] = @__p_0) AND ([e].[Id] = @__p_1)",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_principal_composite_key(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_principal_composite_key(state);

//            Assert.Equal(
//                @"@__p_0='Root' (Size = 450)
//@__p_1='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE ([e].[AlternateId] = @__p_0) AND ([e].[Id] = @__p_1)",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

//        public override void Lazy_load_one_to_one_reference_to_dependent_composite_key(EntityState state)
//        {
//            base.Lazy_load_one_to_one_reference_to_dependent_composite_key(state);

//            Assert.Equal(
//                @"@__p_0='Root' (Size = 450)
//@__p_1='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
//FROM [SingleCompositeKey] AS [e]
//WHERE ([e].[ParentAlternateId] = @__p_0) AND ([e].[ParentId] = @__p_1)",
//                Sql,
//                ignoreLineEndingDifferences: true);
//        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(state);

            Assert.Equal("", Sql);
        }

//        public override async Task Load_collection(EntityState state, bool async)
//        {
//            await base.Load_collection(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_PK_to_PK_reference_to_principal(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_PK_to_PK_reference_to_dependent(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id]
//FROM [SinglePkToPk] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_using_Query(EntityState state, bool async)
//        {
//            await base.Load_collection_using_Query(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT TOP(2) [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id]
//FROM [SinglePkToPk] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

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

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_not_found(EntityState state, bool async)
//        {
//            await base.Load_collection_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_not_found(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_not_found(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_not_found(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_using_Query_not_found(EntityState state, bool async)
//        {
//            await base.Load_collection_using_Query_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT TOP(2) [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

        public override async Task Load_collection_already_loaded(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_collection_already_loaded(state, async, cascadeDeleteTiming);

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

        public override async Task Load_one_to_one_reference_to_principal_already_loaded(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_principal_already_loaded(state, async, cascadeDeleteTiming);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

        public override async Task Load_one_to_one_reference_to_dependent_already_loaded(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded(state, async, cascadeDeleteTiming);

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

//        public override async Task Load_collection_using_Query_already_loaded(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
//        {
//            await base.Load_collection_using_Query_already_loaded(state, async, cascadeDeleteTiming);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded(state, async, cascadeDeleteTiming);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT TOP(2) [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id]
//FROM [SinglePkToPk] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_untyped(EntityState state, bool async)
//        {
//            await base.Load_collection_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_untyped(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_using_Query_untyped(EntityState state, bool async)
//        {
//            await base.Load_collection_using_Query_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_collection_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_using_Query_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_collection_using_Query_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='787'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='767' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

        public override async Task Load_collection_already_loaded_untyped(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_collection_already_loaded_untyped(state, async, cascadeDeleteTiming);

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

        public override async Task Load_one_to_one_reference_to_dependent_already_loaded_untyped(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
        {
            await base.Load_one_to_one_reference_to_dependent_already_loaded_untyped(state, async, cascadeDeleteTiming);

            if (!async)
            {
                Assert.Equal("", Sql);
            }
        }

//        public override async Task Load_collection_using_Query_already_loaded_untyped(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
//        {
//            await base.Load_collection_using_Query_already_loaded_untyped(state, async, cascadeDeleteTiming);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Child] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(EntityState state, bool async, CascadeTiming cascadeDeleteTiming)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(state, async, cascadeDeleteTiming);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [Single] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_collection_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[ParentId]
//FROM [ChildAk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[AlternateId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[AlternateId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[ParentId]
//FROM [SingleAk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_using_Query_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_collection_using_Query_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT [e].[Id], [e].[ParentId]
//FROM [ChildAk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[AlternateId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[AlternateId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)

//SELECT TOP(2) [e].[Id], [e].[ParentId]
//FROM [SingleAk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

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

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_collection_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [ChildShadowFk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [SingleShadowFk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_using_Query_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_collection_using_Query_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentId]
//FROM [ChildShadowFk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE [e].[Id] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='707' (Nullable = true)

//SELECT TOP(2) [e].[Id], [e].[ParentId]
//FROM [SingleShadowFk] AS [e]
//WHERE [e].[ParentId] = @__p_0",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

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

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_composite_key(EntityState state, bool async)
//        {
//            await base.Load_collection_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
//FROM [ChildCompositeKey] AS [e]
//WHERE ([e].[ParentAlternateId] = @__p_0) AND ([e].[ParentId] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_composite_key(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE ([e].[AlternateId] = @__p_0) AND ([e].[Id] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_composite_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707'

//SELECT [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE ([e].[AlternateId] = @__p_0) AND ([e].[Id] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_composite_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
//FROM [SingleCompositeKey] AS [e]
//WHERE ([e].[ParentAlternateId] = @__p_0) AND ([e].[ParentId] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_collection_using_Query_composite_key(EntityState state, bool async)
//        {
//            await base.Load_collection_using_Query_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707' (Nullable = true)

//SELECT [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
//FROM [ChildCompositeKey] AS [e]
//WHERE ([e].[ParentAlternateId] = @__p_0) AND ([e].[ParentId] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE ([e].[AlternateId] = @__p_0) AND ([e].[Id] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707'

//SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE ([e].[AlternateId] = @__p_0) AND ([e].[Id] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_dependent_using_Query_composite_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_dependent_using_Query_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"@__p_0='Root' (Size = 450)
//@__p_1='707' (Nullable = true)

//SELECT TOP(2) [e].[Id], [e].[ParentAlternateId], [e].[ParentId]
//FROM [SingleCompositeKey] AS [e]
//WHERE ([e].[ParentAlternateId] = @__p_0) AND ([e].[ParentId] = @__p_1)",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_null_FK_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal("", Sql);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_null_FK_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal("", Sql);
//            }
//        }

//        public override async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
//        {
//            await base.Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

//        public override async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
//        {
//            await base.Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(state, async);

//            if (!async)
//            {
//                Assert.Equal(
//                    @"SELECT TOP(2) [e].[Id], [e].[AlternateId]
//FROM [Parent] AS [e]
//WHERE 0 = 1",
//                    Sql,
//                    ignoreLineEndingDifferences: true);
//            }
//        }

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql;

        private string Sql { get; set; }

        public class LoadSqlServerFixture : LoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
