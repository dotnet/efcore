// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class CompositeEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P1 = 7, P2 = "Ate", P3 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            var key = (CompositeKeyValue)new CompositeKeyValueFactory(
                type.FindPrimaryKey())
                .Create(type.FindPrimaryKey().Properties, entry);

            Assert.Equal(new object[] { 7, "Ate", random }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P5 = "Ate", P6 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            var key = (CompositeKeyValue)new CompositeKeyValueFactory(
                type.FindPrimaryKey())
                .Create(new[] { type.FindProperty("P6"), type.FindProperty("P5") }, entry);

            Assert.Equal(new object[] { random, "Ate" }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_values_from_a_sidecar()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P4 = 7, P5 = "Ate", P6 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            var sidecar = new RelationshipsSnapshot(entry);
            sidecar[type.FindProperty("P4")] = 77;

            var key = (CompositeKeyValue)new CompositeKeyValueFactory(
                type.FindPrimaryKey())
                .Create(new[] { type.FindProperty("P6"), type.FindProperty("P4"), type.FindProperty("P5") }, sidecar);

            Assert.Equal(new object[] { random, 77, "Ate" }, key.Value);
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_entry_properties_is_null()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P1 = 7, P2 = null, P3 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                KeyValue.InvalidKeyValue,
                new CompositeKeyValueFactory(
                    type.FindPrimaryKey())
                    .Create(type.FindPrimaryKey().Properties, entry));
        }

        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_value_buffer()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));

            var random = new Random();

            var key = (CompositeKeyValue)new CompositeKeyValueFactory(
                type.FindPrimaryKey())
                .Create(type.FindPrimaryKey().Properties, new ValueBuffer(new object[] { 7, "Ate", random }));

            Assert.Equal(new object[] { 7, "Ate", random }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));

            var random = new Random();

            var key = (CompositeKeyValue)new CompositeKeyValueFactory(
                type.FindPrimaryKey())
                .Create(
                    new[] { type.FindProperty("P6"), type.FindProperty("P5") },
                    new ValueBuffer(new object[] { null, null, null, null, "Ate", random }));

            Assert.Equal(new object[] { random, "Ate" }, key.Value);
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_given_buffer_is_null()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));

            var random = new Random();

            var key = new CompositeKeyValueFactory(
                type.FindPrimaryKey())
                .Create(
                    new[] { type.FindProperty("P6"), type.FindProperty("P5") },
                    new ValueBuffer(new object[] { 7, "Ate", random, 77, null, random }));

            Assert.Equal(KeyValue.InvalidKeyValue, key);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = model.AddEntityType(typeof(Banana));
            var property1 = entityType.AddProperty("P1", typeof(int));
            property1.IsShadowProperty = false;
            var property2 = entityType.AddProperty("P2", typeof(string));
            property2.IsShadowProperty = false;
            var property3 = entityType.AddProperty("P3", typeof(Random));
            property3.IsShadowProperty = false;
            var property4 = entityType.AddProperty("P4", typeof(int));
            property4.IsShadowProperty = false;
            var property5 = entityType.AddProperty("P5", typeof(string));
            property5.IsShadowProperty = false;
            var property6 = entityType.AddProperty("P6", typeof(Random));
            property6.IsShadowProperty = false;

            entityType.GetOrSetPrimaryKey(new[] { property1, property2, property3 });
            entityType.GetOrAddForeignKey(new[] { property4, property5, property6 }, entityType.FindPrimaryKey(), entityType);

            return model;
        }

        private class Banana
        {
            public int P1 { get; set; }
            public string P2 { get; set; }
            public Random P3 { get; set; }
            public int P4 { get; set; }
            public string P5 { get; set; }
            public Random P6 { get; set; }
        }
    }
}
