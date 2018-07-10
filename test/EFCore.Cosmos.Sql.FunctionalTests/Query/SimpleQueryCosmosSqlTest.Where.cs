// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query
{
    public partial class SimpleQueryCosmosSqlTest : QueryTestBase<NorthwindQueryCosmosSqlFixture<NoopModelCustomizer>>
    {
        [ConditionalFact]
        public virtual void Where_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE c[""Discriminator""] = ""Customer"" AND c[""City""] = ""London""");
        }
    }
}
