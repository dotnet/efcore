// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class SqlServerModelBuilderGenericTest : ModelBuilderGenericTest
    {
        public class SqlServerGenericNonRelationship : GenericNonRelationship
        {
            [Fact]
            public virtual void Index_has_a_filter_if_nonclustered_unique_with_nullable_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var entityTypeBuilder = modelBuilder
                    .Entity<Customer>();
                var indexBuilder = entityTypeBuilder
                    .HasIndex(ix => ix.Name)
                    .IsUnique();

                var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
                var index = entityType.GetIndexes().Single();
                Assert.Equal("[Name] IS NOT NULL", index.SqlServer().Filter);

                indexBuilder.IsUnique(false);

                Assert.Null(index.SqlServer().Filter);

                indexBuilder.IsUnique();

                Assert.Equal("[Name] IS NOT NULL", index.SqlServer().Filter);

                indexBuilder.ForSqlServerIsClustered();

                Assert.Null(index.SqlServer().Filter);

                indexBuilder.ForSqlServerIsClustered(false);

                Assert.Equal("[Name] IS NOT NULL", index.SqlServer().Filter);

                entityTypeBuilder.Property(e => e.Name).IsRequired();

                Assert.Null(index.SqlServer().Filter);

                entityTypeBuilder.Property(e => e.Name).IsRequired(false);

                Assert.Equal("[Name] IS NOT NULL", index.SqlServer().Filter);

                entityTypeBuilder.Property(e => e.Name).HasColumnName("RelationalName");

                Assert.Equal("[RelationalName] IS NOT NULL", index.SqlServer().Filter);

                entityTypeBuilder.Property(e => e.Name).HasColumnName("SqlServerName");

                Assert.Equal("[SqlServerName] IS NOT NULL", index.SqlServer().Filter);

                entityTypeBuilder.Property(e => e.Name).HasColumnName(null);

                Assert.Equal("[Name] IS NOT NULL", index.SqlServer().Filter);

                indexBuilder.HasFilter("Foo");

                Assert.Equal("Foo", index.SqlServer().Filter);

                indexBuilder.HasFilter(null);

                Assert.Null(index.SqlServer().Filter);
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericInheritance : GenericInheritance
        {
            [Fact] // #7240
            public void Can_use_shadow_FK_that_collides_with_convention_shadow_FK_on_other_derived_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Child>();
                modelBuilder.Entity<Parent>()
                    .HasOne(p => p.A)
                    .WithOne()
                    .HasForeignKey<DisjointChildSubclass1>("ParentId");

                modelBuilder.Validate();

                var property1 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass1)).FindProperty("ParentId");
                Assert.True(property1.IsForeignKey());
                Assert.Equal("ParentId", property1.SqlServer().ColumnName);
                var property2 = modelBuilder.Model.FindEntityType(typeof(DisjointChildSubclass2)).FindProperty("ParentId");
                Assert.True(property2.IsForeignKey());
                Assert.Equal("DisjointChildSubclass2_ParentId", property2.SqlServer().ColumnName);
            }

            [Fact] //Issue#10659
            public void Index_convention_run_for_fk_when_derived_type_discovered_before_base_type()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Order>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Entity<DetailsBase>();

                var index = modelBuilder.Model.FindEntityType(typeof(CustomerDetails)).GetIndexes().Single();

                Assert.Equal("[CustomerId] IS NOT NULL", index.SqlServer().Filter);
            }

            [Fact]
            public void Index_convention_sets_filter_for_unique_index_when_base_type_changed()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<CustomerDetails>()
                    .HasIndex(e => e.CustomerId)
                    .IsUnique();

                modelBuilder.Entity<DetailsBase>();

                var index = modelBuilder.Model.FindEntityType(typeof(CustomerDetails)).GetIndexes().Single();

                Assert.Equal("[CustomerId] IS NOT NULL", index.SqlServer().Filter);

                modelBuilder.Ignore<DetailsBase>();

                Assert.Null(index.SqlServer().Filter);
            }

            public class Parent
            {
                public int Id { get; set; }
                public DisjointChildSubclass1 A { get; set; }
                public IList<DisjointChildSubclass2> B { get; set; }
            }

            public abstract class Child
            {
                public int Id { get; set; }
            }

            public class DisjointChildSubclass1 : Child
            {
            }

            public class DisjointChildSubclass2 : Child
            {
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericOneToMany : GenericOneToMany
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericOneToOne : GenericOneToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericOwnedTypes : GenericOwnedTypes
        {
            [Fact]
            public virtual void Owned_types_use_table_splitting_by_default()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).OwnsOne(l => l.AnotherBookLabel).OwnsOne(s => s.SpecialBookLabel);
                modelBuilder.Entity<Book>().OwnsOne(b => b.Label).OwnsOne(l => l.SpecialBookLabel).OwnsOne(a => a.AnotherBookLabel);

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label).OwnsOne(l => l.AnotherBookLabel).OwnsOne(a => a.SpecialBookLabel);
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).OwnsOne(l => l.SpecialBookLabel).OwnsOne(s => s.AnotherBookLabel);

                modelBuilder.Validate();

                var book = model.FindEntityType(typeof(Book));
                var bookOwnership1 = book.FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;

                Assert.Equal(book.SqlServer().TableName, bookOwnership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookOwnership2.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookLabel1Ownership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookLabel1Ownership2.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookLabel2Ownership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookLabel2Ownership2.DeclaringEntityType.SqlServer().TableName);

                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());

                Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
                Assert.Equal(1, bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys().Count());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));

                Assert.Equal(
                    nameof(Book.Label) + "_" + nameof(BookLabel.Id),
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);
                Assert.Equal(
                    nameof(Book.AlternateLabel) + "_" + nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label).ToTable("Label");
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).ToTable("AlternateLabel");

                Assert.Equal(
                    nameof(BookLabel.Id),
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);
                Assert.Equal(
                    nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);
            }

            [Fact]
            public virtual void Owned_types_can_be_mapped_to_different_tables()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Book>(bb =>
                {
                    bb.ToTable("BT", "BS");
                    bb.OwnsOne(b => b.AlternateLabel, tb =>
                    {
                        tb.ToTable("TT", "TS");
                        tb.OwnsOne(l => l.AnotherBookLabel, ab =>
                        {
                            ab.ToTable("AT1", "AS1");
                            ab.OwnsOne(s => s.SpecialBookLabel)
                                .ToTable("ST11", "SS11");
                            ((Navigation)ab.OwnedEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel))).AddAnnotation("Foo", "Bar");
                        });
                    });
                    bb.OwnsOne(b => b.Label, lb =>
                    {
                        lb.ToTable("LT", "LS");
                        lb.OwnsOne(l => l.SpecialBookLabel, sb =>
                        {
                            sb.ToTable("ST1", "SS1");
                            sb.OwnsOne(a => a.AnotherBookLabel)
                                .ToTable("AT11", "AS11");
                        });
                    });
                    bb.OwnsOne(b => b.Label).OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.ToTable("AT2", "AS2");
                        ab.OwnsOne(a => a.SpecialBookLabel)
                            .ToTable("ST21", "SS21");
                    });
                    bb.OwnsOne(b => b.AlternateLabel).OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.ToTable("ST2", "SS2");
                        sb.OwnsOne(s => s.AnotherBookLabel)
                            .ToTable("AT21", "AS21");
                    });
                });

                modelBuilder.Validate();

                var book = model.FindEntityType(typeof(Book));
                var bookOwnership1 = book.FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel1Ownership11 = bookLabel1Ownership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel1Ownership21 = bookLabel1Ownership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership11 = bookLabel2Ownership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership21 = bookLabel2Ownership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;

                Assert.Equal("BS", book.SqlServer().Schema);
                Assert.Equal("BT", book.SqlServer().TableName);
                Assert.Equal("LS", bookOwnership1.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("LT", bookOwnership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("TS", bookOwnership2.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("TT", bookOwnership2.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("AS2", bookLabel1Ownership1.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("AT2", bookLabel1Ownership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("SS1", bookLabel1Ownership2.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("ST1", bookLabel1Ownership2.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("AS1", bookLabel2Ownership1.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("AT1", bookLabel2Ownership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("SS2", bookLabel2Ownership2.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("ST2", bookLabel2Ownership2.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("SS21", bookLabel1Ownership11.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("ST21", bookLabel1Ownership11.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("AS11", bookLabel1Ownership21.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("AT11", bookLabel1Ownership21.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("SS11", bookLabel2Ownership11.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("ST11", bookLabel2Ownership11.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal("AS21", bookLabel2Ownership21.DeclaringEntityType.SqlServer().Schema);
                Assert.Equal("AT21", bookLabel2Ownership21.DeclaringEntityType.SqlServer().TableName);

                Assert.Equal("Bar",  bookLabel2Ownership11.PrincipalToDependent["Foo"]);

                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());

                Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
                Assert.Equal(1, bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys().Count());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }

        public class SqlServerGenericQueryTypes : GenericQueryTypes
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance);
        }
    }
}
