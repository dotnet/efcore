// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class AtsQueryTest : QueryTestBase<AtsNorthwindQueryFixture>
    {
        public override void Where_bool_member()
        {
            // TODO: #965
            //base.Where_bool_member();
        }

        public override void Where_bool_member_false()
        {
            // TODO: #965
            //base.Where_bool_member_false();
        }

        public override void Where_bool_member_shadow()
        {
            // TODO: #965
            //base.Where_bool_member_shadow();
        }

        public override void Where_bool_member_false_shadow()
        {
            // TODO: #965
            //base.Where_bool_member_false_shadow();
        }

        public AtsQueryTest(AtsNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
