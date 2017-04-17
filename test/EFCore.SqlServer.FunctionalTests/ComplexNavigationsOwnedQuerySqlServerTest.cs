// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class ComplexNavigationsOwnedQuerySqlServerTest
        : ComplexNavigationsOwnedQueryTestBase<SqlServerTestStore, ComplexNavigationsOwnedQuerySqlServerFixture>
    {
        public ComplexNavigationsOwnedQuerySqlServerTest(
            ComplexNavigationsOwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        // TODO: Assert SQL

        [ConditionalFact(Skip = "issue #4311")]
        public override void Nested_group_join_with_take()
        {
            base.Nested_group_join_with_take();
        }

        [ConditionalFact(Skip = "issue #8254")]
        public override void Where_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            base.Where_nav_prop_reference_optional2_via_DefaultIfEmpty();
        }

        [ConditionalFact(Skip = "issue #8254")]
        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection();
        }

        [ConditionalFact(Skip = "issue #8254")]
        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection2()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2();
        }

        [ConditionalFact(Skip = "issue #8254")]
        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection3()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection3();
        }

        [ConditionalFact(Skip = "issue #8254")]
        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection4()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection4();
        }

        [ConditionalFact(Skip = "issue #8255")]
        public override void Multiple_required_navigation_using_multiple_selects_with_string_based_Include()
        {
            base.Multiple_required_navigation_using_multiple_selects_with_string_based_Include();
        }

        [ConditionalFact(Skip = "issue #8255")]
        public override void Multiple_required_navigations_with_Include()
        {
            base.Multiple_required_navigations_with_Include();
        }

        [ConditionalFact(Skip = "issue #8255")]
        public override void Multiple_required_navigation_with_string_based_Include()
        {
            base.Multiple_required_navigation_with_string_based_Include();
        }

        [ConditionalFact(Skip = "issue #8255")]
        public override void Multiple_required_navigation_using_multiple_selects_with_Include()
        {
            base.Multiple_required_navigation_using_multiple_selects_with_Include();
        }
    }
}
