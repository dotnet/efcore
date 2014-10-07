// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalModelBuilderTest
    {
        [Fact]
        public void Entity_returns_same_instance_for_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);

            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.TryGetEntityType(typeof(Customer)));
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Entity_returns_same_instance_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);

            var entityBuilder = modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.TryGetEntityType(typeof(Customer).FullName));
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));
        }

        [Fact]
        public void Can_ignore_lower_source_entity_type_using_entity_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);
            modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);

            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Null(model.TryGetEntityType(typeof(Customer)));
            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.Null(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Can_ignore_lower_source_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);
            modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation);

            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer).FullName, ConfigurationSource.Explicit));

            Assert.Null(model.TryGetEntityType(typeof(Customer).FullName));
            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer).FullName, ConfigurationSource.Explicit));
            Assert.Null(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Explicit));
        }

        [Fact]
        public void Cannot_ignore_higher_source_entity_type_using_entity_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);

            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

            Assert.False(modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.NotNull(model.TryGetEntityType(typeof(Customer)));
        }

        [Fact]
        public void Cannot_ignore_higher_source_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);

            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer).FullName, ConfigurationSource.Convention));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.False(modelBuilder.IgnoreEntity(typeof(Customer).FullName, ConfigurationSource.Convention));

            Assert.NotNull(model.TryGetEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Can_only_ignore_existing_entity_type_explicitly_using_entity_entity_clr_type()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Customer));
            model.AddEntityType(entityType);
            var modelBuilder = new InternalModelBuilder(model, null);
            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention).Metadata);

            Assert.False(modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention).Metadata);
            Assert.False(modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.NotNull(model.TryGetEntityType(typeof(Customer)));

            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer), ConfigurationSource.Explicit));
        }

        [Fact]
        public void Can_only_ignore_existing_entity_type_explicitly_using_entity_entity_type_name()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Customer).FullName);
            model.AddEntityType(entityType);
            var modelBuilder = new InternalModelBuilder(model, null);
            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention).Metadata);

            Assert.False(modelBuilder.IgnoreEntity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention).Metadata);
            Assert.False(modelBuilder.IgnoreEntity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));
            Assert.NotNull(model.TryGetEntityType(typeof(Customer).FullName));

            Assert.True(modelBuilder.IgnoreEntity(typeof(Customer).FullName, ConfigurationSource.Explicit));
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
