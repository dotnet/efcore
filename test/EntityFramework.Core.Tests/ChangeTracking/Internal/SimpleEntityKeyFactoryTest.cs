// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class SimpleEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 8 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>(type.GetPrimaryKey())
                .Create(type.GetPrimaryKey().Properties, entry);

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 8 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, entry);

            Assert.Equal(8, key.Value);
        }

        [Fact]
        public void Returns_null_if_key_value_is_null()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = null };
            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(EntityKey.InvalidEntityKey, new SimpleEntityKeyFactory<int>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, entry));
        }

        [Fact]
        public void Creates_a_new_key_for_CLR_defaults_of_nullable_types()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 0 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var key = (SimpleEntityKey<int?>)new SimpleEntityKeyFactory<int?>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, entry);

            Assert.Equal(0, key.Value);
        }

        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().GetEntityType(typeof(Banana));

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>(type.GetPrimaryKey())
                .Create(type.GetPrimaryKey().Properties, new ValueBuffer(new object[] { 7, "Ate" }));

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().GetEntityType(typeof(Banana));

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, new ValueBuffer(new object[] { 7, 8 }));

            Assert.Equal(8, key.Value);
        }

        [Fact]
        public void Creates_a_new_primary_key_for_reference_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().GetEntityType(typeof(Kiwi));

            var key = (SimpleEntityKey<string>)new SimpleEntityKeyFactory<string>(type.GetPrimaryKey())
                .Create(type.GetPrimaryKey().Properties, new ValueBuffer(new object[] { "7", "Ate" }));

            Assert.Equal("7", key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_reference_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().GetEntityType(typeof(Kiwi));

            var key = (SimpleEntityKey<string>)new SimpleEntityKeyFactory<string>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, new ValueBuffer(new object[] { "7", "Ate" }));

            Assert.Equal("Ate", key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_CLR_defaults_of_nullable_types_using_value_reader()
        {
            var type = BuildModel().GetEntityType(typeof(Banana));

            var key = (SimpleEntityKey<int?>)new SimpleEntityKeyFactory<int?>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, new ValueBuffer(new object[] { 7, 0 }));

            Assert.Equal(0, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_CLR_defaults_using_value_reader()
        {
            var type = BuildModel().GetEntityType(typeof(Banana));

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P1") }, new ValueBuffer(new object[] { 0, 8 }));

            Assert.Equal(0, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_from_a_sidecar_value()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 8 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var sidecar = new RelationshipsSnapshot(entry);
            sidecar[type.GetProperty("P2")] = "Eaten";

            var key = (SimpleEntityKey<string>)new SimpleEntityKeyFactory<string>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, sidecar);

            Assert.Equal("Eaten", key.Value);
        }

        [Fact]
        public void Creates_a_new_key_from_current_value_when_value_not_yet_set_in_sidecar()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 8 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var sidecar = new RelationshipsSnapshot(entry);

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>(type.GetPrimaryKey())
                .Create(new[] { type.GetProperty("P2") }, sidecar);

            Assert.Equal(8, key.Value);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = model.AddEntityType(typeof(Banana));
            var property1 = entityType.AddProperty("P1", typeof(int));
            property1.IsShadowProperty = false;
            var property2 = entityType.AddProperty("P2", typeof(int?));
            property2.IsShadowProperty = false;

            entityType.GetOrSetPrimaryKey(property1);
            entityType.GetOrAddForeignKey(property2, entityType.GetPrimaryKey(), entityType);

            entityType = model.AddEntityType(typeof(Kiwi));
            var property3 = entityType.AddProperty("P1", typeof(string));
            property3.IsShadowProperty = false;
            var property4 = entityType.AddProperty("P2", typeof(string));
            property4.IsShadowProperty = false;

            entityType.GetOrSetPrimaryKey(property3);
            entityType.GetOrAddForeignKey(property4, entityType.GetPrimaryKey(), entityType);

            return model;
        }

        private class Banana
        {
            public int P1 { get; set; }
            public int? P2 { get; set; }
        }

        private class Kiwi
        {
            public string P1 { get; set; }
            public string P2 { get; set; }
        }
    }
}
