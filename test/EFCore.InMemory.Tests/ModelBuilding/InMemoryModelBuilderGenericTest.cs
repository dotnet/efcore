// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class InMemoryModelBuilderGenericTest : ModelBuilderGenericTest
    {
        public class InMemoryGenericNonRelationship : GenericNonRelationship
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);
        }

        public class InMemoryGenericInheritance : GenericInheritance
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);
        }

        public class InMemoryGenericOneToMany : GenericOneToMany
        {
            [ConditionalFact] // Issue #3376
            public virtual void Can_use_self_referencing_overlapping_FK_PK()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<ModifierGroupHeader>()
                    .HasKey(
                        x => new { x.GroupHeaderId, x.AccountId });

                modelBuilder.Entity<ModifierGroupHeader>()
                    .HasOne(x => x.ModifierGroupHeader2)
                    .WithMany(x => x.ModifierGroupHeader1)
                    .HasForeignKey(
                        x => new { x.LinkedGroupHeaderId, x.AccountId });

                var contextOptions = new DbContextOptionsBuilder()
                    .UseModel(modelBuilder.Model.FinalizeModel())
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("Can_use_self_referencing_overlapping_FK_PK")
                    .Options;

                using (var context = new DbContext(contextOptions))
                {
                    var parent = context.Add(
                        new ModifierGroupHeader { GroupHeaderId = 77, AccountId = 90 }).Entity;
                    var child1 = context.Add(
                        new ModifierGroupHeader { GroupHeaderId = 78, AccountId = 90 }).Entity;
                    var child2 = context.Add(
                        new ModifierGroupHeader { GroupHeaderId = 79, AccountId = 90 }).Entity;

                    child1.ModifierGroupHeader2 = parent;
                    child2.ModifierGroupHeader2 = parent;

                    context.SaveChanges();

                    AssertGraph(parent, child1, child2);
                }

                using (var context = new DbContext(contextOptions))
                {
                    var parent = context.Set<ModifierGroupHeader>().Single(e => e.GroupHeaderId == 77);
                    var child1 = context.Set<ModifierGroupHeader>().Single(e => e.GroupHeaderId == 78);
                    var child2 = context.Set<ModifierGroupHeader>().Single(e => e.GroupHeaderId == 79);

                    AssertGraph(parent, child1, child2);
                }
            }

            private static void AssertGraph(
                ModifierGroupHeader parent,
                ModifierGroupHeader child1,
                ModifierGroupHeader child2)
            {
                Assert.Equal(new[] { child1, child2 }, parent.ModifierGroupHeader1.ToArray());
                Assert.Same(parent, child1.ModifierGroupHeader2);
                Assert.Same(parent, child2.ModifierGroupHeader2);

                Assert.Equal(77, parent.GroupHeaderId);
                Assert.Equal(78, child1.GroupHeaderId);
                Assert.Equal(79, child2.GroupHeaderId);
                Assert.Equal(90, parent.AccountId);
                Assert.Equal(90, child1.AccountId);
                Assert.Equal(90, child2.AccountId);
                Assert.Null(parent.LinkedGroupHeaderId);
                Assert.Equal(77, child1.LinkedGroupHeaderId);
                Assert.Equal(77, child2.LinkedGroupHeaderId);
            }

            private class ModifierGroupHeader
            {
                [Key]
                [Column(Order = 0)]
                public int GroupHeaderId { get; set; }

                [Key]
                [Column(Order = 1)]
                [DatabaseGenerated(DatabaseGeneratedOption.None)]
                public int AccountId { get; set; }

                [Required]
                [StringLength(50)]
                public string GroupBatchName { get; set; }

                [StringLength(200)]
                public string GroupBatchNameAlt { get; set; }

                public int MaxModifierSelectCount { get; set; }

                public int? LinkedGroupHeaderId { get; set; }

                public bool Enabled { get; set; }

                public DateTime CreatedDate { get; set; }

                [Required]
                [StringLength(50)]
                public string CreatedBy { get; set; }

                public DateTime ModifiedDate { get; set; }

                [Required]
                [StringLength(50)]
                public string ModifiedBy { get; set; }

                public bool? IsFollowSet { get; set; }

                public virtual ICollection<ModifierGroupHeader> ModifierGroupHeader1 { get; set; }
                    = new HashSet<ModifierGroupHeader>();

                public virtual ModifierGroupHeader ModifierGroupHeader2 { get; set; }
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);
        }

        public class InMemoryGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);
        }

        public class InMemoryGenericOneToOne : GenericOneToOne
        {
            [ConditionalFact]
            public virtual void Can_use_self_referencing_overlapping_FK_PK()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Node>(
                    b =>
                    {
                        b.HasKey(
                            e => new { e.ListId, e.PreviousNodeId });
                        b.HasOne(e => e.NextNode)
                            .WithOne(e => e.PreviousNode)
                            .HasForeignKey<Node>(
                                e => new { e.ListId, e.NextNodeId });
                    });

                var contextOptions = new DbContextOptionsBuilder()
                    .UseModel(modelBuilder.Model.FinalizeModel())
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("Can_use_self_referencing_overlapping_FK_PK_one_to_one")
                    .Options;

                using (var context = new DbContext(contextOptions))
                {
                    var node1 = context.Add(
                        new Node { ListId = 90, PreviousNodeId = 77 }).Entity;
                    var node2 = context.Add(
                        new Node { ListId = 90, PreviousNodeId = 78 }).Entity;
                    var node3 = context.Add(
                        new Node { ListId = 90, PreviousNodeId = 79 }).Entity;

                    node1.NextNode = node2;
                    node3.PreviousNode = node2;

                    context.SaveChanges();

                    AssertGraph(node1, node2, node3);
                }

                using (var context = new DbContext(contextOptions))
                {
                    var node1 = context.Set<Node>().Single(e => e.PreviousNodeId == 77);
                    var node2 = context.Set<Node>().Single(e => e.PreviousNodeId == 78);
                    var node3 = context.Set<Node>().Single(e => e.PreviousNodeId == 79);

                    AssertGraph(node1, node2, node3);
                }
            }

            private static void AssertGraph(Node node1, Node node2, Node node3)
            {
                Assert.Null(node1.PreviousNode);
                Assert.Same(node1, node2.PreviousNode);
                Assert.Same(node2, node1.NextNode);
                Assert.Same(node2, node3.PreviousNode);
                Assert.Same(node3, node2.NextNode);
                Assert.Null(node3.NextNode);

                Assert.Equal(77, node1.PreviousNodeId);
                Assert.Equal(78, node2.PreviousNodeId);
                Assert.Equal(79, node3.PreviousNodeId);
                Assert.Equal(90, node1.ListId);
                Assert.Equal(90, node2.ListId);
                Assert.Equal(90, node3.ListId);
                Assert.Equal(78, node1.NextNodeId);
                Assert.Equal(79, node2.NextNodeId);
                Assert.Equal(0, node3.NextNodeId);
            }

            private class Node
            {
                public int ListId { get; set; }
                public int PreviousNodeId { get; set; }
                public int NextNodeId { get; set; }

                public Node PreviousNode { get; set; }
                public Node NextNode { get; set; }
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);
        }

        public class InMemoryGenericOwnedTypes : GenericOwnedTypes
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);
        }

        public class InMemoryGenericKeylessEntities : GenericKeylessEntities
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);
        }
    }
}
