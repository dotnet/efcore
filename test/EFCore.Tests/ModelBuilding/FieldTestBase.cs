// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        // TODO is this the correct place for these tests?
        public abstract class FieldTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Entity_field_expression_key_test()
            {
                var modelBuilder = CreateRealModelBuilder();
                modelBuilder.Entity<EntityWithFieldKey>().HasKey(e => e.Id);
                var properties = modelBuilder.Model.FindEntityType(typeof(EntityWithFieldKey)).GetProperties();

                Assert.Equal(1, properties.Count());
                var property = properties.Single();
                Assert.Equal(nameof(EntityWithFieldKey.Id), property.Name);
                Assert.Null(property.PropertyInfo);
                Assert.NotNull(property.FieldInfo);
            }

            [Fact]
            public virtual void Entity_field_expression_property_test()
            {
                var modelBuilder = CreateRealModelBuilder();
                modelBuilder.Entity<EntityWithFieldKey>().Property(e => e.Id);
                var properties = modelBuilder.Model.FindEntityType(typeof(EntityWithFieldKey)).GetProperties();

                Assert.Equal(1, properties.Count());
                var property = properties.Single();
                Assert.Equal(nameof(EntityWithFieldKey.Id), property.Name);
                Assert.Null(property.PropertyInfo);
                Assert.NotNull(property.FieldInfo);
            }

            [Fact]
            public virtual void Entity_field_expression_ignore_test()
            {
                var modelBuilder = CreateRealModelBuilder();
                modelBuilder.Entity<EntityWithFieldKey>().Ignore(e => e.Year);

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(EntityWithFieldKey)).GetProperties());
            }

            [Fact]
            public virtual void Query_field_expression_property_test()
            {
                var modelBuilder = CreateRealModelBuilder();
                modelBuilder.Query<QueryWithField>().Property(e => e.Year);
                var properties = modelBuilder.Model.FindEntityType(typeof(QueryWithField)).GetProperties();

                Assert.Equal(1, properties.Count());
                var property = properties.Single();
                Assert.Equal(nameof(EntityWithFieldKey.Year), property.Name);
                Assert.Null(property.PropertyInfo);
                Assert.NotNull(property.FieldInfo);
            }

            public ModelBuilder CreateRealModelBuilder() =>
                InMemoryTestHelpers.Instance.CreateConventionBuilder();
        }
    }
}
