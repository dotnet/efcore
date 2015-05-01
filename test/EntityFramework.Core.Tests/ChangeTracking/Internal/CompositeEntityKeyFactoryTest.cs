// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class CompositeEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P1 = 7, P2 = "Ate", P3 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory(
                new object[] { 0, null, null })
                .Create(type, type.GetPrimaryKey().Properties, entry);

            Assert.Equal(new object[] { 7, "Ate", random }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P5 = "Ate", P6 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory(
                new object[] { null, null })
                .Create(type, new[] { type.GetProperty("P6"), type.GetProperty("P5") }, entry);

            Assert.Equal(new object[] { random, "Ate" }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_values_from_a_sidecar()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P4 = 7, P5 = "Ate", P6 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            var sidecar = new RelationshipsSnapshot(entry);
            sidecar[type.GetProperty("P4")] = 77;

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory(
                new object[] { null, 0, null })
                .Create(type, new[] { type.GetProperty("P6"), type.GetProperty("P4"), type.GetProperty("P5") }, sidecar);

            Assert.Equal(new object[] { random, 77, "Ate" }, key.Value);
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_entry_properties_is_null()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var random = new Random();
            var entity = new Banana { P1 = 7, P2 = null, P3 = random };

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                EntityKey.InvalidEntityKey,
                new CompositeEntityKeyFactory(
                    new object[] { 0, null, null })
                    .Create(type, type.GetPrimaryKey().Properties, entry));
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_entry_properties_are_the_default_sentinel()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 0, P2 = "Ate", P3 = new Random() };

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                EntityKey.InvalidEntityKey,
                new CompositeEntityKeyFactory(
                    new object[] { 0, null, null })
                    .Create(type, type.GetPrimaryKey().Properties, entry));
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_entry_properties_are_the_set_sentinel()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = "Ate", P3 = new Random() };

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                EntityKey.InvalidEntityKey,
                new CompositeEntityKeyFactory(
                    new object[] { 7, null, null })
                    .Create(type, type.GetPrimaryKey().Properties, entry));
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_entry_properties_are_the_default_sentinel_using_value_reader()
        {
            var type = BuildModel().GetEntityType(typeof(Banana));

            Assert.Equal(
                EntityKey.InvalidEntityKey,
                new CompositeEntityKeyFactory(
                    new object[] { 0, null, null })
                    .Create(type, type.GetPrimaryKey().Properties, new ValueBuffer(new object[] { 0, "Ate", new Random() })));
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_entry_properties_are_the_set_sentinel_using_value_reader()
        {
            var type = BuildModel().GetEntityType(typeof(Banana));

            Assert.Equal(
                EntityKey.InvalidEntityKey,
                new CompositeEntityKeyFactory(
                    new object[] { 7, null, null })
                    .Create(type, type.GetPrimaryKey().Properties, new ValueBuffer(new object[] { 7, "Ate", new Random() })));
        }

        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_value_buffer()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var random = new Random();

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory(
                new object[] { 0, null, null })
                .Create(type, type.GetPrimaryKey().Properties, new ValueBuffer(new object[] { 7, "Ate", random }));

            Assert.Equal(new object[] { 7, "Ate", random }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var random = new Random();

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory(
                new object[] { null, null })
                .Create(
                    type,
                    new[] { type.GetProperty("P6"), type.GetProperty("P5") },
                    new ValueBuffer(new object[] { null, null, null, null, "Ate", random }));

            Assert.Equal(new object[] { random, "Ate" }, key.Value);
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_given_buffer_is_null()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var random = new Random();

            var key = new CompositeEntityKeyFactory(
                new object[] { null, null })
                .Create(
                    type,
                    new[] { type.GetProperty("P6"), type.GetProperty("P5") },
                    new ValueBuffer(new object[] { 7, "Ate", random, 77, null, random }));

            Assert.Equal(EntityKey.InvalidEntityKey, key);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = model.AddEntityType(typeof(Banana));
            var property1 = entityType.GetOrAddProperty("P1", typeof(int));
            var property2 = entityType.GetOrAddProperty("P2", typeof(string));
            var property3 = entityType.GetOrAddProperty("P3", typeof(Random));
            var property4 = entityType.GetOrAddProperty("P4", typeof(int));
            var property5 = entityType.GetOrAddProperty("P5", typeof(string));
            var property6 = entityType.GetOrAddProperty("P6", typeof(Random));

            entityType.GetOrSetPrimaryKey(new[] { property1, property2, property3 });
            entityType.GetOrAddForeignKey(new[] { property4, property5, property6 }, entityType.GetPrimaryKey());

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
