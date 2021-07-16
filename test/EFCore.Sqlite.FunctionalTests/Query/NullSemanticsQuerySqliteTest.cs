// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullSemanticsQuerySqliteTest : NullSemanticsQueryTestBase<NullSemanticsQuerySqliteFixture>
    {
        public NullSemanticsQuerySqliteTest(NullSemanticsQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
        {
            var options = new DbContextOptionsBuilder(Fixture.CreateOptions());
            if (useRelationalNulls)
            {
                new SqliteDbContextOptionsBuilder(options).UseRelationalNulls();
            }

            var context = new NullSemanticsContext(options.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }
}
