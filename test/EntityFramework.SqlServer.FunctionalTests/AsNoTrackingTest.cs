// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class AsNoTrackingTest : AsNoTrackingTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override void Applied_to_body_clause()
        {
            base.Applied_to_body_clause();
        }

        public override void Entity_not_added_to_state_manager()
        {
            base.Entity_not_added_to_state_manager();
        }

        public override void Applied_to_body_clause_with_projection()
        {
            base.Applied_to_body_clause_with_projection();
        }

        private readonly NorthwindQueryFixture _fixture;

        public AsNoTrackingTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
