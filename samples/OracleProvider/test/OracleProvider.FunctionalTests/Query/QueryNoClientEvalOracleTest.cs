// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNoClientEvalOracleTest : QueryNoClientEvalTestBase<QueryNoClientEvalOracleFixture>
    {
        public QueryNoClientEvalOracleTest(QueryNoClientEvalOracleFixture fixture)
            : base(fixture)
        {
        }

        public override void Throws_when_from_sql_composed()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalStrings.LogClientEvalWarning.GenerateMessage("where [c].IsLondon"),
                        "RelationalEventId.QueryClientEvaluationWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => context
                            .Customers
                            .FromSql("select * from \"Customers\"")
                            .Where(c => c.IsLondon)
                            .ToList()).Message);
            }
        }

        public override void Doesnt_throw_when_from_sql_not_composed()
        {
            using (var context = CreateContext())
            {
                var customers = context
                    .Customers
                    .FromSql("select * from \"Customers\"")
                    .ToList();

                Assert.Equal(91, customers.Count);
            }
        }
    }
}
