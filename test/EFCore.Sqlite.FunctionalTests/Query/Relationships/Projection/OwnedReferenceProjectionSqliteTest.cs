// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedReferenceProjectionSqliteTest
    : OwnedReferenceProjectionRelationalTestBase<OwnedRelationshipsSqliteFixture>
{
    public OwnedReferenceProjectionSqliteTest(OwnedRelationshipsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_root(bool async)
        => base.Select_root(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_trunk_optional(bool async)
        => base.Select_trunk_optional(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_trunk_required(bool async)
        => base.Select_trunk_required(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_branch_required_required(bool async)
        => base.Select_branch_required_required(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_branch_required_optional(bool async)
        => base.Select_branch_required_optional(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_branch_optional_required(bool async)
        => base.Select_branch_optional_required(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_branch_optional_optional(bool async)
        => base.Select_branch_optional_optional(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_root_duplicated(bool async)
        => base.Select_root_duplicated(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_trunk_and_branch_duplicated(bool async)
        => base.Select_trunk_and_branch_duplicated(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_trunk_and_trunk_duplicated(bool async)
        => base.Select_trunk_and_trunk_duplicated(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_leaf_trunk_root(bool async)
        => base.Select_leaf_trunk_root(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);
}
