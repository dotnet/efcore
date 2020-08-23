// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LazyLoadProxySqlServerTest : LazyLoadProxyTestBase<LazyLoadProxySqlServerTest.LoadSqlServerFixture>
    {
        public LazyLoadProxySqlServerTest(LoadSqlServerFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Lazy_load_collection(EntityState state, bool useAttach, bool useDetach)
        {
            base.Lazy_load_collection(state, useAttach, useDetach);

            Assert.Equal(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_many_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
        {
            base.Lazy_load_many_to_one_reference_to_principal(state, useAttach, useDetach);

            Assert.Equal(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
        {
            base.Lazy_load_one_to_one_reference_to_principal(state, useAttach, useDetach);

            Assert.Equal(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_dependent(EntityState state, bool useAttach, bool useDetach)
        {
            base.Lazy_load_one_to_one_reference_to_dependent(state, useAttach, useDetach);

            Assert.Equal(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal(state);

            Assert.Equal(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(state);

            Assert.Equal(
                @"@__p_0='707'

SELECT [s].[Id]
FROM [SinglePkToPk] AS [s]
WHERE [s].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_null_FK(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_null_FK(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_collection_not_found(EntityState state)
        {
            base.Lazy_load_collection_not_found(state);

            Assert.Equal(
                @"@__p_0='767' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_not_found(state);

            Assert.Equal(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_not_found(state);

            Assert.Equal(
                @"@__p_0='787'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_not_found(state);

            Assert.Equal(
                @"@__p_0='767' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_collection_already_loaded(state, cascadeDeleteTiming);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_many_to_one_reference_to_principal_already_loaded(
            EntityState state,
            CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_many_to_one_reference_to_principal_already_loaded(state, cascadeDeleteTiming);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_already_loaded(state);

            Assert.Equal("", Sql);
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_already_loaded(
            EntityState state,
            CascadeTiming cascadeDeleteTiming)
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

        public override void Lazy_load_many_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_alternate_key(state);

            Assert.Equal(
                @"@__p_0='Root' (Size = 450)

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_alternate_key(state);

            Assert.Equal(
                @"@__p_0='Root' (Size = 450)

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_alternate_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_alternate_key(state);

            Assert.Equal(
                @"@__p_0='Root' (Size = 450)

SELECT [s].[Id], [s].[ParentId]
FROM [SingleAk] AS [s]
WHERE [s].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

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

        public override void Lazy_load_collection_shadow_fk(EntityState state)
        {
            base.Lazy_load_collection_shadow_fk(state);

            Assert.Equal(
                @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [ChildShadowFk] AS [c]
WHERE [c].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_many_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_shadow_fk(state);

            Assert.Equal(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_shadow_fk(state);

            Assert.Equal(
                @"@__p_0='707'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_shadow_fk(state);

            Assert.Equal(
                @"@__p_0='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentId]
FROM [SingleShadowFk] AS [s]
WHERE [s].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

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

        public override void Lazy_load_collection_composite_key(EntityState state)
        {
            base.Lazy_load_collection_composite_key(state);

            Assert.Equal(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentAlternateId], [c].[ParentId]
FROM [ChildCompositeKey] AS [c]
WHERE ([c].[ParentAlternateId] = @__p_0) AND ([c].[ParentId] = @__p_1)",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_many_to_one_reference_to_principal_composite_key(EntityState state)
        {
            base.Lazy_load_many_to_one_reference_to_principal_composite_key(state);

            Assert.Equal(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_principal_composite_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_composite_key(state);

            Assert.Equal(
                @"@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT [p].[Id], [p].[AlternateId], [p].[Discriminator]
FROM [Parent] AS [p]
WHERE ([p].[AlternateId] = @__p_0) AND ([p].[Id] = @__p_1)",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Lazy_load_one_to_one_reference_to_dependent_composite_key(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_composite_key(state);

            Assert.Equal(
                @"@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [s].[Id], [s].[ParentAlternateId], [s].[ParentId]
FROM [SingleCompositeKey] AS [s]
WHERE ([s].[ParentAlternateId] = @__p_0) AND ([s].[ParentId] = @__p_1)",
                Sql,
                ignoreLineEndingDifferences: true);
        }

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

        public override async Task Load_collection(EntityState state, bool async)
        {
            await base.Load_collection(state, async);

            if (!async)
            {
                Assert.Equal(
                    @"@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [ConditionalFact(Skip = "Issue#1015")]
        public override void Top_level_projection_track_entities_before_passing_to_client_method()
        {
            base.Top_level_projection_track_entities_before_passing_to_client_method();

            Assert.Equal(
                @"@__p_0='707' (Nullable = true)

            SELECT [c].[Id], [c].[ParentId]
            FROM [Child] AS [c]
            WHERE [c].[ParentId] = @__p_0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override async Task Entity_equality_with_proxy_parameter(bool async)
        {
            await base.Entity_equality_with_proxy_parameter(async);

            Assert.Equal(
                @"@__entity_equality_called_0_Id='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId]
FROM [Child] AS [c]
LEFT JOIN [Parent] AS [p] ON [c].[ParentId] = [p].[Id]
WHERE [p].[Id] = @__entity_equality_called_0_Id",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog()
            => Sql = Fixture.TestSqlLoggerFactory.Sql;

        private string Sql { get; set; }

        public class LoadSqlServerFixture : LoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }
}
