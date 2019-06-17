// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesSqlServerTestClientNoAction : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerTestClientNoAction.GraphUpdatesWithClientNoActionSqlServerFixture>
    {
        public GraphUpdatesSqlServerTestClientNoAction(GraphUpdatesWithClientNoActionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithClientNoActionSqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string StoreName { get; } = "GraphClientNoActionUpdatesTest";
            public override bool ForceClientNoAction => true;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                foreach (var foreignKey in modelBuilder.Model
                    .GetEntityTypes()
                    .SelectMany(e => e.GetDeclaredForeignKeys()))
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.ClientNoAction;
                }
            }
        }
    }
}
