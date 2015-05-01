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
            var property = GetEntityType().GetProperty("Id");

            Assert.IsType<SimpleEntityKeyFactory<int>>(CreateKeyFactorySource().GetKeyFactory(new[] { property }));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_nullable_property()
        {
            var property = GetEntityType().GetProperty("NullableInt");

            Assert.IsType<SimpleEntityKeyFactory<int>>(CreateKeyFactorySource().GetKeyFactory(new[] { property }));
        }

        [Fact]
        public void Returns_different_simple_entity_key_factory_for_different_properties()
        {
            var entityType = GetEntityType();
            var property1 = entityType.GetProperty("Guid1");
            var property2 = entityType.GetProperty("Guid2");

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(new[] { property1 }), factorySource.GetKeyFactory(new[] { property2 }));
        }

        [Fact]
        public void Returns_different_simple_nullable_entity_key_factory_for_different_properties()
        {
            var entityType = GetEntityType();
            var property1 = entityType.GetProperty("NullableGuid1");
            var property2 = entityType.GetProperty("NullableGuid2");

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(new[] { property1 }), factorySource.GetKeyFactory(new[] { property2 }));
        }

        [Fact]
        public void Returns_same_simple_entity_key_factory_for_same_property()
        {
            var property = GetEntityType().GetProperty("Guid1");

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(new[] { property }), factorySource.GetKeyFactory(new[] { property }));
        }

        [Fact]
        public void Returns_same_nullable_simple_entity_key_factory_for_same_property()
        {
            var property = GetEntityType().GetProperty("NullableGuid1");

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(new[] { property }), factorySource.GetKeyFactory(new[] { property }));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_composite_property_key()
        {
            var entityType = GetEntityType();
            var property1 = entityType.GetProperty("Id");
            var property2 = entityType.GetProperty("String");

            Assert.IsType<CompositeEntityKeyFactory>(
                CreateKeyFactorySource().GetKeyFactory(new[] { property1, property2 }));
        }

        [Fact]
        public void Returns_same_composite_entity_key_factory_for_same_properties()
        {
            var entityType = GetEntityType();
            var property1 = entityType.GetProperty("Id");
            var property2 = entityType.GetProperty("String");

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(new[] { property1, property2 }), factorySource.GetKeyFactory(new[] { property1, property2 }));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_reference_property()
        {
            var property = GetEntityType().GetProperty("String");

            Assert.IsType<SimpleEntityKeyFactory<string>>(CreateKeyFactorySource().GetKeyFactory(new[] { property }));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_single_structural_property()
        {
            var property = GetEntityType().GetProperty("ByteArray");

            Assert.IsType<CompositeEntityKeyFactory>(CreateKeyFactorySource().GetKeyFactory(new[] { property }));
        }

        [Fact]
        public void Returns_same_composite_entity_key_factory_for_same_structural_property()
        {
            var property = GetEntityType().GetProperty("ByteArray");

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(new[] { property }), factorySource.GetKeyFactory(new[] { property }));
        }

        private static IEntityKeyFactorySource CreateKeyFactorySource()
        {
            return new EntityKeyFactorySource();
        }

        private static IEntityType GetEntityType()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<ScissorSister>();

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
