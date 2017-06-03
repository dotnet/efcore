// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesWithRestrictSqlServerTest : GraphUpdatesSqlServerTestBase<GraphUpdatesWithRestrictSqlServerTest.GraphUpdatesWithRestrictSqlServerFixture>
    {
        public GraphUpdatesWithRestrictSqlServerTest(GraphUpdatesWithRestrictSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithRestrictSqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string DatabaseName => "GraphRestrictUpdatesTest";

            public override bool ForceRestrict => true;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                foreach (var foreignKey in modelBuilder.Model
                    .GetEntityTypes()
                    .SelectMany(e => e.GetDeclaredForeignKeys()))
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}
