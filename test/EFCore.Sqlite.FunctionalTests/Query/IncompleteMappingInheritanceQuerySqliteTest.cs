// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncompleteMappingInheritanceQuerySqliteTest : InheritanceRelationalQueryTestBase<
        IncompleteMappingInheritanceQuerySqliteFixture>
    {
        public IncompleteMappingInheritanceQuerySqliteTest(IncompleteMappingInheritanceQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_insert_update_delete()
        {
            // Test from InheritanceSqliteTest causes transaction failure. We only need to test it once.
        }
    }
}
