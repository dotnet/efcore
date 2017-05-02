// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Tests;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
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

                entityTypeBuilder.Property(e => e.Name).ForSqlServerHasColumnName("SqlServerName");

                Assert.Equal("[SqlServerName] IS NOT NULL", index.SqlServer().Filter);

                entityTypeBuilder.Property(e => e.Name).ForSqlServerHasColumnName(null);

                Assert.Equal("[RelationalName] IS NOT NULL", index.SqlServer().Filter);
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance.CreateConventionBuilder());
        }

        public class SqlServerGenericInheritance : GenericInheritance
        {
            [Fact] // #7049
            public void Base_type_can_be_discovered_after_creating_foreign_keys_on_derived()
            {
                var mb = CreateModelBuilder();
                mb.Entity<AL>();
                mb.Entity<L>();

                Assert.Equal(ValueGenerated.OnAdd, mb.Model.FindEntityType(typeof(Q)).FindProperty(nameof(Q.ID)).ValueGenerated);
            }

            public class L
            {
                public int Id { get; set; }
                public IList<T> Ts { get; set; }
            }

            public class T : P
            {
                public Q D { get; set; }
                public P P { get; set; }
                public Q F { get; set; }
            }

            public class P : PBase { }

            public class Q : PBase { }

            public abstract class PBase
            {
                public int ID { get; set; }
                public string Stuff { get; set; }
            }

            public class AL
            {
                public int Id { get; set; }
                public PBase L { get; set; }
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance.CreateConventionBuilder());
        }

        public class SqlServerGenericOneToMany : GenericOneToMany
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance.CreateConventionBuilder());
        }

        public class SqlServerGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance.CreateConventionBuilder());
        }

        public class SqlServerGenericOneToOne : GenericOneToOne
        {
            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance.CreateConventionBuilder());
        }

        public class SqlServerGenericOwnedTypes : GenericOwnedTypes
        {
            [Fact]
            public virtual void Owned_types_use_table_splitting()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var bookOwnershipBuilder2 = modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel);
                var bookLabel2OwnershipBuilder1 = bookOwnershipBuilder2.OwnsOne(l => l.AnotherBookLabel);
                var bookOwnershipBuilder1 = modelBuilder.Entity<Book>().OwnsOne(b => b.Label);
                var bookLabel1OwnershipBuilder2 = bookOwnershipBuilder1.OwnsOne(l => l.SpecialBookLabel);

                var book = model.FindEntityType(typeof(Book));
                var bookOwnership1 = book.FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = book.FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;

                // Only owned types have the table name set
                Assert.Equal(book.SqlServer().TableName, bookOwnership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookOwnership2.DeclaringEntityType.SqlServer().TableName);
                Assert.NotEqual(book.SqlServer().TableName, bookLabel1Ownership1.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookLabel1Ownership2.DeclaringEntityType.SqlServer().TableName);
                Assert.Equal(book.SqlServer().TableName, bookLabel2Ownership1.DeclaringEntityType.SqlServer().TableName);
                Assert.NotEqual(book.SqlServer().TableName, bookLabel2Ownership2.DeclaringEntityType.SqlServer().TableName);

                var bookLabel1OwnershipBuilder1 = bookOwnershipBuilder1.OwnsOne(l => l.AnotherBookLabel);
                var bookLabel2OwnershipBuilder2 = bookOwnershipBuilder2.OwnsOne(l => l.SpecialBookLabel);
                bookLabel1OwnershipBuilder1.OwnsOne(l => l.SpecialBookLabel);
                bookLabel1OwnershipBuilder2.OwnsOne(l => l.AnotherBookLabel);
                bookLabel2OwnershipBuilder1.OwnsOne(l => l.SpecialBookLabel);
                bookLabel2OwnershipBuilder2.OwnsOne(l => l.AnotherBookLabel);

                modelBuilder.Validate();

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

                Assert.Equal(nameof(Book.Label) + "_" + nameof(BookLabel.Id),
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);
                Assert.Equal(nameof(Book.AlternateLabel) + "_" + nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);

                bookOwnershipBuilder1.ForSqlServerToTable("Label");
                bookOwnershipBuilder2.ForSqlServerToTable("AlternateLabel");

                Assert.Equal(nameof(BookLabel.Id),
                    bookOwnership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);
                Assert.Equal(nameof(BookLabel.AnotherBookLabel) + "_" + nameof(BookLabel.Id),
                    bookLabel2Ownership1.DeclaringEntityType.FindProperty(nameof(BookLabel.Id)).SqlServer().ColumnName);
            }

            protected override TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(SqlServerTestHelpers.Instance.CreateConventionBuilder());
        }
    }
}
