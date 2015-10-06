// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class EntityKeyFactorySourceTest
    {
        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_property()
        {
            var key = GetEntityType().GetPrimaryKey();

            Assert.IsType<SimpleEntityKeyFactory<int>>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_nullable_property()
        {
            var entityType = GetEntityType();
            var key = entityType.GetKey(entityType.GetProperty("NullableInt"));

            Assert.IsType<SimpleEntityKeyFactory<int>>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_different_simple_entity_key_factory_for_different_properties()
        {
            var entityType = GetEntityType();
            var key1 = entityType.GetKey(entityType.GetProperty("Guid1"));
            var key2 = entityType.GetKey(entityType.GetProperty("Guid2"));

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(key1), factorySource.GetKeyFactory(key2));
        }

        [Fact]
        public void Returns_different_simple_nullable_entity_key_factory_for_different_properties()
        {
            var entityType = GetEntityType();
            var key1 = entityType.GetKey(entityType.GetProperty("NullableGuid1"));
            var key2 = entityType.GetKey(entityType.GetProperty("NullableGuid2"));

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(key1), factorySource.GetKeyFactory(key2));
        }

        [Fact]
        public void Returns_same_simple_entity_key_factory_for_same_property()
        {
            var entityType = GetEntityType();
            var key = entityType.GetKey(entityType.GetProperty("Guid1"));

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(key), factorySource.GetKeyFactory(key));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_composite_property_key()
        {
            var entityType = GetEntityType();
            var key = entityType.GetKey(new[] { entityType.GetProperty("Id"), entityType.GetProperty("String") });

            Assert.IsType<CompositeEntityKeyFactory>(
                CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_same_composite_entity_key_factory_for_same_properties()
        {
            var entityType = GetEntityType();
            var key1 = entityType.GetKey(new[] { entityType.GetProperty("Id"), entityType.GetProperty("String") });
            var key2 = entityType.GetKey(new[] { entityType.GetProperty("Id"), entityType.GetProperty("String") });

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(key1), factorySource.GetKeyFactory(key2));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_reference_property()
        {
            var entityType = GetEntityType();
            var key = entityType.GetKey(entityType.GetProperty("String"));

            Assert.IsType<SimpleEntityKeyFactory<string>>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_single_structural_property()
        {
            var entityType = GetEntityType();
            var key = entityType.GetKey(entityType.GetProperty("ByteArray"));

            Assert.IsType<CompositeEntityKeyFactory>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_same_composite_entity_key_factory_for_same_structural_property()
        {
            var entityType = GetEntityType();
            var key = entityType.GetKey(entityType.GetProperty("ByteArray"));

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(key), factorySource.GetKeyFactory(key));
        }

        private static IEntityKeyFactorySource CreateKeyFactorySource() => new EntityKeyFactorySource();

        private static IEntityType GetEntityType()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<ScissorSister>(b =>
                {
                    b.HasAlternateKey(e => e.NullableInt);
                    b.HasAlternateKey(e => e.Guid1);
                    b.HasAlternateKey(e => e.Guid2);
                    b.HasAlternateKey(e => e.NullableGuid1);
                    b.HasAlternateKey(e => e.NullableGuid2);
                    b.HasAlternateKey(e => e.String);
                    b.HasAlternateKey(e => e.ByteArray);
                    b.HasAlternateKey(e => new { e.Id, e.String });
                });

            return builder.Model.GetEntityType(typeof(ScissorSister));
        }

        private class ScissorSister
        {
            public int Id { get; set; }
            public int? NullableInt { get; set; }
            public string String { get; set; }
            public Guid Guid1 { get; set; }
            public Guid Guid2 { get; set; }
            public Guid? NullableGuid1 { get; set; }
            public Guid? NullableGuid2 { get; set; }
            public byte[] ByteArray { get; set; }
        }
    }
}
