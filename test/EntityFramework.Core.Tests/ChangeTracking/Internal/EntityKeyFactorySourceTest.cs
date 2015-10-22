// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            var key = GetEntityType().FindPrimaryKey();

            Assert.IsType<SimpleKeyValueFactory<int>>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_nullable_property()
        {
            var entityType = GetEntityType();
            var key = entityType.FindKey( entityType.FindProperty("NullableInt") );

            Assert.IsType<SimpleKeyValueFactory<int>>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_different_simple_entity_key_factory_for_different_properties()
        {
            var entityType = GetEntityType();
            var key1 = entityType.FindKey( entityType.FindProperty("Guid1") );
            var key2 = entityType.FindKey( entityType.FindProperty("Guid2") );

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(key1), factorySource.GetKeyFactory(key2));
        }

        [Fact]
        public void Returns_different_simple_nullable_entity_key_factory_for_different_properties()
        {
            var entityType = GetEntityType();
            var key1 = entityType.FindKey( entityType.FindProperty("NullableGuid1") );
            var key2 = entityType.FindKey( entityType.FindProperty("NullableGuid2") );

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(key1), factorySource.GetKeyFactory(key2));
        }

        [Fact]
        public void Returns_same_simple_entity_key_factory_for_same_property()
        {
            var entityType = GetEntityType();
            var key = entityType.FindKey( entityType.FindProperty("Guid1") );

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(key), factorySource.GetKeyFactory(key));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_composite_property_key()
        {
            var entityType = GetEntityType();
            var key = entityType.FindKey(new[] { entityType.FindProperty("Id"), entityType.FindProperty("String") });

            Assert.IsType<CompositeKeyValueFactory>(
                CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_same_composite_entity_key_factory_for_same_properties()
        {
            var entityType = GetEntityType();
            var key1 = entityType.FindKey(new[] { entityType.FindProperty("Id"), entityType.FindProperty("String") });
            var key2 = entityType.FindKey(new[] { entityType.FindProperty("Id"), entityType.FindProperty("String") });

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(key1), factorySource.GetKeyFactory(key2));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_reference_property()
        {
            var entityType = GetEntityType();
            var key = entityType.FindKey( entityType.FindProperty("String") );

            Assert.IsType<SimpleKeyValueFactory<string>>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_single_structural_property()
        {
            var entityType = GetEntityType();
            var key = entityType.FindKey( entityType.FindProperty("ByteArray") );

            Assert.IsType<CompositeKeyValueFactory>(CreateKeyFactorySource().GetKeyFactory(key));
        }

        [Fact]
        public void Returns_same_composite_entity_key_factory_for_same_structural_property()
        {
            var entityType = GetEntityType();
            var key = entityType.FindKey( entityType.FindProperty("ByteArray") );

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(key), factorySource.GetKeyFactory(key));
        }

        private static IKeyValueFactorySource CreateKeyFactorySource() => new KeyValueFactorySource();

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

            return builder.Model.FindEntityType(typeof(ScissorSister));
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
