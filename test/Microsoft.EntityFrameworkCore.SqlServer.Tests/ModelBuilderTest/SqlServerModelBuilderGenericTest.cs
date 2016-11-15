// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
    }
}
