// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQuerySqliteTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQuerySqliteFixture>
    {
        public ComplexNavigationsQuerySqliteTest(ComplexNavigationsQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        // Skip for SQLite. Issue #14935. Cannot eval 'from <>f__AnonymousType100`1 <generated>_1 in {from Level2 l2 in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) where  ?= (Convert(Property([l1], \"Id\"), Nullable`1) == Property([l2], \"OneToMany_Optional_Inverse2Id\")) =? select new <>f__AnonymousType100`1(Name = [l2].Name)}'
        public override Task SelectMany_subquery_with_custom_projection(bool isAsync) => null;
    }
}
