// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class DatabindingSqlServerTest : DatabindingTestBase<SqlServerTestStore, F1SqlServerFixture>
    {
        public DatabindingSqlServerTest(F1SqlServerFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip = "Test is flaky on CI")]
        public override void Entities_deleted_from_context_are_removed_from_local_view()
        {
            base.Entities_deleted_from_context_are_removed_from_local_view();
        }

        [Fact(Skip = "Test is flaky on CI")]
        public override void LocalView_is_initialized_with_entities_from_the_context()
        {
            base.LocalView_is_initialized_with_entities_from_the_context();
        }

        [Fact(Skip = "Test is flaky on CI")]
        public override void Adding_entity_to_state_manager_of_different_type_than_local_view_type_has_no_effect_on_local_view()
        {
            base.Adding_entity_to_state_manager_of_different_type_than_local_view_type_has_no_effect_on_local_view();
        }

        [Fact(Skip = "Test is flaky on CI")]
        public override void DbSet_Local_does_not_call_DetectChanges()
        {
            base.DbSet_Local_does_not_call_DetectChanges();
        }

        [Fact(Skip = "Test is flaky on CI")]
        public override void Entities_removed_from_the_local_view_are_marked_deleted_in_the_state_manager()
        {
            base.Entities_removed_from_the_local_view_are_marked_deleted_in_the_state_manager();
        }
    }
}
