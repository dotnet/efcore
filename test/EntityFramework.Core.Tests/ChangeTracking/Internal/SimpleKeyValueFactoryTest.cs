// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class SimpleKeyValueFactoryTest
    {
        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 8 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var property = type.FindPrimaryKey().Properties.Single();

            var key = (KeyValue<int>)new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(entry[property]);

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 8 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var property = type.FindProperty("P2");

            var key = (KeyValue<int>)new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(entry[property]);

            Assert.Equal(8, key.Value);
        }

        [Fact]
        public void Returns_null_if_key_value_is_null()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = null };
            var entry = stateManager.GetOrCreateEntry(entity);

            var property = type.FindProperty("P2");

            Assert.True(new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(entry[property]).IsInvalid);
        }

        [Fact]
        public void Creates_a_new_key_for_CLR_defaults_of_nullable_types()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7, P2 = 0 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var property = type.FindProperty("P2");

            var key = (KeyValue<int?>)new SimpleKeyValueFactory<int?>(type.FindPrimaryKey())
                .Create(entry[property]);

            Assert.Equal(0, key.Value);
        }

        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().FindEntityType(typeof(Banana));

            var key = (KeyValue<int>)new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(type.FindPrimaryKey().Properties, new ValueBuffer(new object[] { 7, "Ate" }));

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().FindEntityType(typeof(Banana));

            var key = (KeyValue<int>)new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(new[] { type.FindProperty("P2") }, new ValueBuffer(new object[] { 7, 8 }));

            Assert.Equal(8, key.Value);
        }

        [Fact]
        public void Creates_a_new_primary_key_for_reference_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().FindEntityType(typeof(Kiwi));

            var key = (KeyValue<string>)new SimpleKeyValueFactory<string>(type.FindPrimaryKey())
                .Create(type.FindPrimaryKey().Properties, new ValueBuffer(new object[] { "7", "Ate" }));

            Assert.Equal("7", key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_reference_key_values_in_the_given_value_buffer()
        {
            var type = BuildModel().FindEntityType(typeof(Kiwi));

            var key = (KeyValue<string>)new SimpleKeyValueFactory<string>(type.FindPrimaryKey())
                .Create(new[] { type.FindProperty("P2") }, new ValueBuffer(new object[] { "7", "Ate" }));

            Assert.Equal("Ate", key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_CLR_defaults_of_nullable_types_using_value_reader()
        {
            var type = BuildModel().FindEntityType(typeof(Banana));

            var key = (KeyValue<int?>)new SimpleKeyValueFactory<int?>(type.FindPrimaryKey())
                .Create(new[] { type.FindProperty("P2") }, new ValueBuffer(new object[] { 7, 0 }));

            Assert.Equal(0, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_CLR_defaults_using_value_reader()
        {
            var type = BuildModel().FindEntityType(typeof(Banana));

            var key = (KeyValue<int>)new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(new[] { type.FindProperty("P1") }, new ValueBuffer(new object[] { 0, 8 }));

            Assert.Equal(0, key.Value);
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
            entityType.GetOrAddForeignKey(property2, entityType.FindPrimaryKey(), entityType);

            entityType = model.AddEntityType(typeof(Kiwi));
            var property3 = entityType.AddProperty("P1", typeof(string));
            property3.IsShadowProperty = false;
            var property4 = entityType.AddProperty("P2", typeof(string));
            property4.IsShadowProperty = false;

            entityType.GetOrSetPrimaryKey(property3);
            entityType.GetOrAddForeignKey(property4, entityType.FindPrimaryKey(), entityType);

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
