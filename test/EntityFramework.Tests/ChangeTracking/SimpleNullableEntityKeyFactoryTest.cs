// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class SimpleNullableEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_key_for_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var entity = new Banana { P1 = 7 };
            var entry = new ClrStateEntry(TestHelpers.CreateContextConfiguration(model), type, entity);

            var key = (SimpleEntityKey<int>)new SimpleNullableEntityKeyFactory<int, int?>().Create(type, type.GetKey().Properties, entry);

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Returns_null_if_key_value_is_null()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var entity = new Banana { P1 = 7, P2 = null };
            var entry = new ClrStateEntry(TestHelpers.CreateContextConfiguration(model), type, entity);

            Assert.Equal(EntityKey.NullEntityKey, new SimpleNullableEntityKeyFactory<int, int?>().Create(type, new[] { type.GetProperty("P2") }, entry));
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var key = (SimpleEntityKey<int>)new SimpleNullableEntityKeyFactory<int, int?>().Create(
                type, new[] { type.GetProperty("P2") }, new ObjectArrayValueReader(new object[] { 7, 77 }));

            Assert.Equal(77, key.Value);
        }

        [Fact]
        public void Returns_null_if_value_in_buffer_is_null()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            Assert.Null(new SimpleNullableEntityKeyFactory<int, int?>().Create(
                type, new[] { type.GetProperty("P2") }, new ObjectArrayValueReader(new object[] { 7, null })));
        }

        [Fact]
        public void Creates_a_new_key_from_a_sidecar_value()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var entity = new Banana { P1 = 7, P2 = 77 };
            var entry = new ClrStateEntry(TestHelpers.CreateContextConfiguration(model), type, entity);

            var sidecar = new RelationshipsSnapshot(entry);
            sidecar[type.GetProperty("P2")] = 78;

            var key = (SimpleEntityKey<int>)new SimpleNullableEntityKeyFactory<int, int?>().Create(
                type, new[] { type.GetProperty("P2") }, sidecar);

            Assert.Equal(78, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_from_current_value_when_value_not_yet_set_in_sidecar()
        {
            var model = BuildModel();
            var type = model.GetEntityType(typeof(Banana));

            var entity = new Banana { P1 = 7, P2 = 77 };
            var entry = new ClrStateEntry(TestHelpers.CreateContextConfiguration(model), type, entity);

            var sidecar = new RelationshipsSnapshot(entry);

            var key = (SimpleEntityKey<int>)new SimpleNullableEntityKeyFactory<int, int?>().Create(
                type, new[] { type.GetProperty("P2") }, sidecar);

            Assert.Equal(77, key.Value);
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var entityType = new EntityType(typeof(Banana));
            var property1 = entityType.AddProperty("P1", typeof(int));
            var property2 = entityType.AddProperty("P2", typeof(string));

            entityType.SetKey(property1);
            entityType.AddForeignKey(entityType.GetKey(), property2);

            model.AddEntityType(entityType);

            return model;
        }

        private class Banana
        {
            public int P1 { get; set; }
            public int? P2 { get; set; }
        }
    }
}
