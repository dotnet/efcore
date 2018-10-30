// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.SupportsGraphing)]
    public class GraphTablesTest : IClassFixture<GraphTablesTest.GraphTablesFixture>
    {
        protected GraphTablesFixture Fixture { get; }

        public GraphTablesTest(GraphTablesFixture fixture) => Fixture = fixture;

        [ConditionalFact]
        public void Can_create_graph_node_table()
        {
            using (CreateTestStore())
            {
                var nodeUns = new[]
                {
                    new NodeUn { Name = "Some un" },
                    new NodeUn { Name = "Another un" },
                };

                using (var context = CreateContext())
                {
                    context.Database.EnsureCreatedResiliently();

                    // ReSharper disable once CoVariantArrayConversion
                    context.AddRange(nodeUns);

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var local = nodeUns
                        .OrderBy(f => f.Name)
                        .Select(f => f.Name);

                    var fromContext = context.Nodes
                        .OrderBy(f => f.Name)
                        .Select(f => f.Name);

                    Assert.True(local.SequenceEqual(fromContext));
                }
            }
        }

        [ConditionalFact]
        public void Can_create_graph_edge_table()
        {
            using (CreateTestStore())
            {
                var nodeUns = new[]
                {
                    new NodeUn
                    {
                        Name = "Node1 un"
                    },
                    new NodeUn
                    {
                        Name = "Node2 un"
                    }
                };

                using (var context = CreateContext())
                {
                    context.Database.EnsureCreatedResiliently();

                    // ReSharper disable once CoVariantArrayConversion
                    context.AddRange(nodeUns);

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    context.Database.EnsureCreatedResiliently();

                    var nodes = context.Nodes.ToList();

                    Assert.True(nodes.All(e => !string.IsNullOrEmpty(e.NodeId)));

                    var edge = new EdgeUn
                    {
                        FromId = nodes.First().NodeId,
                        ToId = nodes.Last().NodeId
                    };

                    context.Add(edge);

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var edges = context.Edges
                        .OrderBy(f => f.Name)
                        .ToList();

                    Assert.True(edges.All(e => !string.IsNullOrEmpty(e.EdgeId)));
                }
            }
        }

        protected TestStore TestStore { get; set; }

        protected TestStore CreateTestStore()
        {
            TestStore = SqlServerTestStore.GetOrCreate(nameof(GraphTablesTest));
            TestStore.Initialize(null, CreateContext, c => { });
            return TestStore;
        }

        private GraphContext CreateContext() => new GraphContext(Fixture.CreateOptions(TestStore));

        public class GraphTablesFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }

        private class GraphContext : DbContext
        {
            public GraphContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<NodeUn> Nodes { get; set; }
            public DbSet<EdgeUn> Edges { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<NodeUn>().ForSqlServerIsGraphNode(e => e.NodeId);

                modelBuilder.Entity<EdgeUn>().ForSqlServerIsGraphEdge(
                    e => e.EdgeId,
                    e => e.FromId,
                    e => e.ToId);
            }
        }

        private class NodeUn
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string NodeId { get; set; }
        }

        private class EdgeUn
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string EdgeId { get; set; }
            public string FromId { get; set; }
            public string ToId { get; set; }
        }
    }
}
