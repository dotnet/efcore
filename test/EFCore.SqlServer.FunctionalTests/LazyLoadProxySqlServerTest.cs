// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

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

        AssertSql(
            state == EntityState.Detached && useAttach
                ? ""
                : """
@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId], [c].[Culture_Rating], [c].[Culture_Species], [c].[Culture_Subspecies], [c].[Culture_Validation], [c].[Culture_License_Charge], [c].[Culture_License_Title], [c].[Culture_License_Tag_Text], [c].[Culture_License_Tog_Text], [c].[Culture_Manufacturer_Name], [c].[Culture_Manufacturer_Rating], [c].[Culture_Manufacturer_Tag_Text], [c].[Culture_Manufacturer_Tog_Text], [c].[Milk_Rating], [c].[Milk_Species], [c].[Milk_Subspecies], [c].[Milk_Validation], [c].[Milk_License_Charge], [c].[Milk_License_Title], [c].[Milk_License_Tag_Text], [c].[Milk_License_Tog_Text], [c].[Milk_Manufacturer_Name], [c].[Milk_Manufacturer_Rating], [c].[Milk_Manufacturer_Tag_Text], [c].[Milk_Manufacturer_Tog_Text]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
    {
        base.Lazy_load_many_to_one_reference_to_principal(state, useAttach, useDetach);

        AssertSql(
            state == EntityState.Detached && useAttach
                ? ""
                : """
@__p_0='707'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
    {
        base.Lazy_load_one_to_one_reference_to_principal(state, useAttach, useDetach);

        AssertSql(
            state == EntityState.Detached && useAttach
                ? ""
                : """
@__p_0='707'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_dependent(EntityState state, bool useAttach, bool useDetach)
    {
        base.Lazy_load_one_to_one_reference_to_dependent(state, useAttach, useDetach);

        AssertSql(
            state == EntityState.Detached && useAttach
                ? ""
                : """
@__p_0='707' (Nullable = true)

SELECT TOP(1) [s].[Id], [s].[ParentId], [s].[Culture_Rating], [s].[Culture_Species], [s].[Culture_Subspecies], [s].[Culture_Validation], [s].[Culture_License_Charge], [s].[Culture_License_Title], [s].[Culture_License_Tag_Text], [s].[Culture_License_Tog_Text], [s].[Culture_Manufacturer_Name], [s].[Culture_Manufacturer_Rating], [s].[Culture_Manufacturer_Tag_Text], [s].[Culture_Manufacturer_Tog_Text], [s].[Milk_Rating], [s].[Milk_Species], [s].[Milk_Subspecies], [s].[Milk_Validation], [s].[Milk_License_Charge], [s].[Milk_License_Title], [s].[Milk_License_Tag_Text], [s].[Milk_License_Tog_Text], [s].[Milk_Manufacturer_Name], [s].[Milk_Manufacturer_Rating], [s].[Milk_Manufacturer_Tag_Text], [s].[Milk_Manufacturer_Tog_Text]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
    {
        base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal(state);

        AssertSql(
            """
@__p_0='707'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
    {
        base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(state);

        AssertSql(
            """
@__p_0='707'

SELECT TOP(1) [s].[Id], [s].[Culture_Rating], [s].[Culture_Species], [s].[Culture_Subspecies], [s].[Culture_Validation], [s].[Culture_License_Charge], [s].[Culture_License_Title], [s].[Culture_License_Tag_Text], [s].[Culture_License_Tog_Text], [s].[Culture_Manufacturer_Name], [s].[Culture_Manufacturer_Rating], [s].[Culture_Manufacturer_Tag_Text], [s].[Culture_Manufacturer_Tog_Text], [s].[Milk_Rating], [s].[Milk_Species], [s].[Milk_Subspecies], [s].[Milk_Validation], [s].[Milk_License_Charge], [s].[Milk_License_Title], [s].[Milk_License_Tag_Text], [s].[Milk_License_Tog_Text], [s].[Milk_Manufacturer_Name], [s].[Milk_Manufacturer_Rating], [s].[Milk_Manufacturer_Tag_Text], [s].[Milk_Manufacturer_Tog_Text]
FROM [SinglePkToPk] AS [s]
WHERE [s].[Id] = @__p_0
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_null_FK(state);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_null_FK(state);

        AssertSql(@"");
    }

    public override void Lazy_load_collection_not_found(EntityState state)
    {
        base.Lazy_load_collection_not_found(state);

        AssertSql(
            """
@__p_0='767' (Nullable = true)

SELECT [c].[Id], [c].[ParentId], [c].[Culture_Rating], [c].[Culture_Species], [c].[Culture_Subspecies], [c].[Culture_Validation], [c].[Culture_License_Charge], [c].[Culture_License_Title], [c].[Culture_License_Tag_Text], [c].[Culture_License_Tog_Text], [c].[Culture_Manufacturer_Name], [c].[Culture_Manufacturer_Rating], [c].[Culture_Manufacturer_Tag_Text], [c].[Culture_Manufacturer_Tog_Text], [c].[Milk_Rating], [c].[Milk_Species], [c].[Milk_Subspecies], [c].[Milk_Validation], [c].[Milk_License_Charge], [c].[Milk_License_Title], [c].[Milk_License_Tag_Text], [c].[Milk_License_Tog_Text], [c].[Milk_Manufacturer_Name], [c].[Milk_Manufacturer_Rating], [c].[Milk_Manufacturer_Tag_Text], [c].[Milk_Manufacturer_Tog_Text]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_not_found(state);

        AssertSql(
            """
@__p_0='787'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_not_found(state);

        AssertSql(
            """
@__p_0='787'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_dependent_not_found(state);

        AssertSql(
            """
@__p_0='767' (Nullable = true)

SELECT TOP(1) [s].[Id], [s].[ParentId], [s].[Culture_Rating], [s].[Culture_Species], [s].[Culture_Subspecies], [s].[Culture_Validation], [s].[Culture_License_Charge], [s].[Culture_License_Title], [s].[Culture_License_Tag_Text], [s].[Culture_License_Tog_Text], [s].[Culture_Manufacturer_Name], [s].[Culture_Manufacturer_Rating], [s].[Culture_Manufacturer_Tag_Text], [s].[Culture_Manufacturer_Tog_Text], [s].[Milk_Rating], [s].[Milk_Species], [s].[Milk_Subspecies], [s].[Milk_Validation], [s].[Milk_License_Charge], [s].[Milk_License_Title], [s].[Milk_License_Tag_Text], [s].[Milk_License_Tog_Text], [s].[Milk_Manufacturer_Name], [s].[Milk_Manufacturer_Rating], [s].[Milk_Manufacturer_Tag_Text], [s].[Milk_Manufacturer_Tog_Text]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0
""");
    }

    public override void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
    {
        base.Lazy_load_collection_already_loaded(state, cascadeDeleteTiming);

        AssertSql(@"");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_already_loaded(
        EntityState state,
        CascadeTiming cascadeDeleteTiming)
    {
        base.Lazy_load_many_to_one_reference_to_principal_already_loaded(state, cascadeDeleteTiming);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_already_loaded(state);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_reference_to_dependent_already_loaded(
        EntityState state,
        CascadeTiming cascadeDeleteTiming)
    {
        base.Lazy_load_one_to_one_reference_to_dependent_already_loaded(state, cascadeDeleteTiming);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
    {
        base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(state);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
    {
        base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(state);

        AssertSql(@"");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_alternate_key(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_alternate_key(state);

        AssertSql(
            """
@__p_0='Root' (Size = 450)

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_alternate_key(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_alternate_key(state);

        AssertSql(
            """
@__p_0='Root' (Size = 450)

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_dependent_alternate_key(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_dependent_alternate_key(state);

        AssertSql(
            """
@__p_0='Root' (Size = 450)

SELECT TOP(1) [s].[Id], [s].[ParentId], [s].[Culture_Rating], [s].[Culture_Species], [s].[Culture_Subspecies], [s].[Culture_Validation], [s].[Culture_License_Charge], [s].[Culture_License_Title], [s].[Culture_License_Tag_Text], [s].[Culture_License_Tog_Text], [s].[Culture_Manufacturer_Name], [s].[Culture_Manufacturer_Rating], [s].[Culture_Manufacturer_Tag_Text], [s].[Culture_Manufacturer_Tog_Text], [s].[Milk_Rating], [s].[Milk_Species], [s].[Milk_Subspecies], [s].[Milk_Validation], [s].[Milk_License_Charge], [s].[Milk_License_Title], [s].[Milk_License_Tag_Text], [s].[Milk_License_Tog_Text], [s].[Milk_Manufacturer_Name], [s].[Milk_Manufacturer_Rating], [s].[Milk_Manufacturer_Tag_Text], [s].[Milk_Manufacturer_Tog_Text]
FROM [SingleAk] AS [s]
WHERE [s].[ParentId] = @__p_0
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(state);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(state);

        AssertSql(@"");
    }

    public override void Lazy_load_collection_shadow_fk(EntityState state)
    {
        base.Lazy_load_collection_shadow_fk(state);

        AssertSql(
            """
@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId], [c].[Culture_Rating], [c].[Culture_Species], [c].[Culture_Subspecies], [c].[Culture_Validation], [c].[Culture_License_Charge], [c].[Culture_License_Title], [c].[Culture_License_Tag_Text], [c].[Culture_License_Tog_Text], [c].[Culture_Manufacturer_Name], [c].[Culture_Manufacturer_Rating], [c].[Culture_Manufacturer_Tag_Text], [c].[Culture_Manufacturer_Tog_Text], [c].[Milk_Rating], [c].[Milk_Species], [c].[Milk_Subspecies], [c].[Milk_Validation], [c].[Milk_License_Charge], [c].[Milk_License_Title], [c].[Milk_License_Tag_Text], [c].[Milk_License_Tog_Text], [c].[Milk_Manufacturer_Name], [c].[Milk_Manufacturer_Rating], [c].[Milk_Manufacturer_Tag_Text], [c].[Milk_Manufacturer_Tog_Text]
FROM [ChildShadowFk] AS [c]
WHERE [c].[ParentId] = @__p_0
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_shadow_fk(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_shadow_fk(state);

        AssertSql(
            state == EntityState.Detached
                ? ""
                : """
@__p_0='707'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_shadow_fk(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_shadow_fk(state);

        AssertSql(
            state == EntityState.Detached
                ? ""
                : """
@__p_0='707'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[Id] = @__p_0
""");
    }

    public override void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_dependent_shadow_fk(state);

        AssertSql(
            """
@__p_0='707' (Nullable = true)

SELECT TOP(1) [s].[Id], [s].[ParentId], [s].[Culture_Rating], [s].[Culture_Species], [s].[Culture_Subspecies], [s].[Culture_Validation], [s].[Culture_License_Charge], [s].[Culture_License_Title], [s].[Culture_License_Tag_Text], [s].[Culture_License_Tog_Text], [s].[Culture_Manufacturer_Name], [s].[Culture_Manufacturer_Rating], [s].[Culture_Manufacturer_Tag_Text], [s].[Culture_Manufacturer_Tog_Text], [s].[Milk_Rating], [s].[Milk_Species], [s].[Milk_Subspecies], [s].[Milk_Validation], [s].[Milk_License_Charge], [s].[Milk_License_Title], [s].[Milk_License_Tag_Text], [s].[Milk_License_Tog_Text], [s].[Milk_Manufacturer_Name], [s].[Milk_Manufacturer_Rating], [s].[Milk_Manufacturer_Tag_Text], [s].[Milk_Manufacturer_Tog_Text]
FROM [SingleShadowFk] AS [s]
WHERE [s].[ParentId] = @__p_0
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(state);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(state);

        AssertSql(@"");
    }

    public override void Lazy_load_collection_composite_key(EntityState state)
    {
        base.Lazy_load_collection_composite_key(state);

        AssertSql(
            """
@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentAlternateId], [c].[ParentId], [c].[Culture_Rating], [c].[Culture_Species], [c].[Culture_Subspecies], [c].[Culture_Validation], [c].[Culture_License_Charge], [c].[Culture_License_Title], [c].[Culture_License_Tag_Text], [c].[Culture_License_Tog_Text], [c].[Culture_Manufacturer_Name], [c].[Culture_Manufacturer_Rating], [c].[Culture_Manufacturer_Tag_Text], [c].[Culture_Manufacturer_Tog_Text], [c].[Milk_Rating], [c].[Milk_Species], [c].[Milk_Subspecies], [c].[Milk_Validation], [c].[Milk_License_Charge], [c].[Milk_License_Title], [c].[Milk_License_Tag_Text], [c].[Milk_License_Tog_Text], [c].[Milk_Manufacturer_Name], [c].[Milk_Manufacturer_Rating], [c].[Milk_Manufacturer_Tag_Text], [c].[Milk_Manufacturer_Tog_Text]
FROM [ChildCompositeKey] AS [c]
WHERE [c].[ParentAlternateId] = @__p_0 AND [c].[ParentId] = @__p_1
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_composite_key(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_composite_key(state);

        AssertSql(
            """
@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0 AND [p].[Id] = @__p_1
""");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_composite_key(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_composite_key(state);

        AssertSql(
            """
@__p_0='Root' (Size = 450)
@__p_1='707'

SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
WHERE [p].[AlternateId] = @__p_0 AND [p].[Id] = @__p_1
""");
    }

    public override void Lazy_load_one_to_one_reference_to_dependent_composite_key(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_dependent_composite_key(state);

        AssertSql(
            """
@__p_0='Root' (Size = 450)
@__p_1='707' (Nullable = true)

SELECT TOP(1) [s].[Id], [s].[ParentAlternateId], [s].[ParentId], [s].[Culture_Rating], [s].[Culture_Species], [s].[Culture_Subspecies], [s].[Culture_Validation], [s].[Culture_License_Charge], [s].[Culture_License_Title], [s].[Culture_License_Tag_Text], [s].[Culture_License_Tog_Text], [s].[Culture_Manufacturer_Name], [s].[Culture_Manufacturer_Rating], [s].[Culture_Manufacturer_Tag_Text], [s].[Culture_Manufacturer_Tog_Text], [s].[Milk_Rating], [s].[Milk_Species], [s].[Milk_Subspecies], [s].[Milk_Validation], [s].[Milk_License_Charge], [s].[Milk_License_Title], [s].[Milk_License_Tag_Text], [s].[Milk_License_Tog_Text], [s].[Milk_Manufacturer_Name], [s].[Milk_Manufacturer_Rating], [s].[Milk_Manufacturer_Tag_Text], [s].[Milk_Manufacturer_Tog_Text]
FROM [SingleCompositeKey] AS [s]
WHERE [s].[ParentAlternateId] = @__p_0 AND [s].[ParentId] = @__p_1
""");
    }

    public override void Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
    {
        base.Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(state);

        AssertSql(@"");
    }

    public override void Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
    {
        base.Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(state);

        AssertSql(@"");
    }

    public override async Task Load_collection(EntityState state, bool async)
    {
        await base.Load_collection(state, async);

        if (!async)
        {
            AssertSql(
                """
@__p_0='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId], [c].[Culture_Rating], [c].[Culture_Species], [c].[Culture_Subspecies], [c].[Culture_Validation], [c].[Culture_License_Charge], [c].[Culture_License_Title], [c].[Culture_License_Tag_Text], [c].[Culture_License_Tog_Text], [c].[Culture_Manufacturer_Name], [c].[Culture_Manufacturer_Rating], [c].[Culture_Manufacturer_Tag_Text], [c].[Culture_Manufacturer_Tog_Text], [c].[Milk_Rating], [c].[Milk_Species], [c].[Milk_Subspecies], [c].[Milk_Validation], [c].[Milk_License_Charge], [c].[Milk_License_Title], [c].[Milk_License_Tag_Text], [c].[Milk_License_Tog_Text], [c].[Milk_Manufacturer_Name], [c].[Milk_Manufacturer_Rating], [c].[Milk_Manufacturer_Tag_Text], [c].[Milk_Manufacturer_Tog_Text]
FROM [Child] AS [c]
WHERE [c].[ParentId] = @__p_0
""");
        }
    }

    [ConditionalFact]
    public override void Top_level_projection_track_entities_before_passing_to_client_method()
    {
        base.Top_level_projection_track_entities_before_passing_to_client_method();

        AssertSql(
            """
SELECT TOP(1) [p].[Id], [p].[AlternateId], [p].[Discriminator], [p].[Culture_Rating], [p].[Culture_Species], [p].[Culture_Subspecies], [p].[Culture_Validation], [p].[Culture_License_Charge], [p].[Culture_License_Title], [p].[Culture_License_Tag_Text], [p].[Culture_License_Tog_Text], [p].[Culture_Manufacturer_Name], [p].[Culture_Manufacturer_Rating], [p].[Culture_Manufacturer_Tag_Text], [p].[Culture_Manufacturer_Tog_Text], [p].[Milk_Rating], [p].[Milk_Species], [p].[Milk_Subspecies], [p].[Milk_Validation], [p].[Milk_License_Charge], [p].[Milk_License_Title], [p].[Milk_License_Tag_Text], [p].[Milk_License_Tog_Text], [p].[Milk_Manufacturer_Name], [p].[Milk_Manufacturer_Rating], [p].[Milk_Manufacturer_Tag_Text], [p].[Milk_Manufacturer_Tog_Text]
FROM [Parent] AS [p]
ORDER BY [p].[Id]

@__p_0='707' (Nullable = true)

SELECT TOP(1) [s].[Id], [s].[ParentId], [s].[Culture_Rating], [s].[Culture_Species], [s].[Culture_Subspecies], [s].[Culture_Validation], [s].[Culture_License_Charge], [s].[Culture_License_Title], [s].[Culture_License_Tag_Text], [s].[Culture_License_Tog_Text], [s].[Culture_Manufacturer_Name], [s].[Culture_Manufacturer_Rating], [s].[Culture_Manufacturer_Tag_Text], [s].[Culture_Manufacturer_Tog_Text], [s].[Milk_Rating], [s].[Milk_Species], [s].[Milk_Subspecies], [s].[Milk_Validation], [s].[Milk_License_Charge], [s].[Milk_License_Title], [s].[Milk_License_Tag_Text], [s].[Milk_License_Tog_Text], [s].[Milk_Manufacturer_Name], [s].[Milk_Manufacturer_Rating], [s].[Milk_Manufacturer_Tag_Text], [s].[Milk_Manufacturer_Tog_Text]
FROM [Single] AS [s]
WHERE [s].[ParentId] = @__p_0
""");
    }

    public override async Task Entity_equality_with_proxy_parameter(bool async)
    {
        await base.Entity_equality_with_proxy_parameter(async);

        AssertSql(
            """
@__entity_equality_called_0_Id='707' (Nullable = true)

SELECT [c].[Id], [c].[ParentId], [c].[Culture_Rating], [c].[Culture_Species], [c].[Culture_Subspecies], [c].[Culture_Validation], [c].[Culture_License_Charge], [c].[Culture_License_Title], [c].[Culture_License_Tag_Text], [c].[Culture_License_Tog_Text], [c].[Culture_Manufacturer_Name], [c].[Culture_Manufacturer_Rating], [c].[Culture_Manufacturer_Tag_Text], [c].[Culture_Manufacturer_Tog_Text], [c].[Milk_Rating], [c].[Milk_Species], [c].[Milk_Subspecies], [c].[Milk_Validation], [c].[Milk_License_Charge], [c].[Milk_License_Title], [c].[Milk_License_Tag_Text], [c].[Milk_License_Tog_Text], [c].[Milk_Manufacturer_Name], [c].[Milk_Manufacturer_Rating], [c].[Milk_Manufacturer_Tag_Text], [c].[Milk_Manufacturer_Tog_Text]
FROM [Child] AS [c]
LEFT JOIN [Parent] AS [p] ON [c].[ParentId] = [p].[Id]
WHERE [p].[Id] = @__entity_equality_called_0_Id
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected override void RecordLog()
        => Sql = Fixture.TestSqlLoggerFactory.Sql;

    private const string FileNewLine = @"
";

    private void AssertSql(string expected)
    {
        expected ??= "";
        var sql = Sql ?? "";
        try
        {
            Assert.Equal(
                expected, sql, ignoreLineEndingDifferences: true);
        }
        catch
        {
            var methodCallLine = Environment.StackTrace.Split(
                [Environment.NewLine],
                StringSplitOptions.RemoveEmptyEntries)[2][6..];

            var indexMethodEnding = methodCallLine.IndexOf(')') + 1;
            var testName = methodCallLine.Substring(0, indexMethodEnding);
            var parts = methodCallLine[indexMethodEnding..].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var fileName = parts[1][..^5];
            var lineNumber = int.Parse(parts[2]);

            var currentDirectory = Directory.GetCurrentDirectory();
            var logFile = currentDirectory.Substring(
                    0,
                    currentDirectory.LastIndexOf(
                        $"{Path.DirectorySeparatorChar}artifacts{Path.DirectorySeparatorChar}",
                        StringComparison.Ordinal)
                    + 1)
                + "QueryBaseline.txt";

            var testInfo = testName + " : " + lineNumber + FileNewLine;
            var newBaseLine = $@"            AssertSql(
                {"@\"" + sql.Replace("\"", "\"\"") + "\""});

";

            var contents = testInfo + newBaseLine + FileNewLine + "--------------------" + FileNewLine;

            File.AppendAllText(logFile, contents);

            throw;
        }
    }

    private string Sql { get; set; }

    public class LoadSqlServerFixture : LoadFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
