// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class SimpleNullableEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_key_for_values_in_the_given_entry()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));
            var stateManager = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

            var entity = new Banana { P1 = 7 };
            var entry = stateManager.GetOrCreateEntry(entity);

            var property = type.FindPrimaryKey().Properties.Single();

            var key = (KeyValue<int>)new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(entry[property]);

            Assert.Equal(7, key.Value);
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
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));

            var key = (KeyValue<int>)new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                .Create(new[] { type.FindProperty("P2") }, new ValueBuffer(new object[] { 7, 77 }));

            Assert.Equal(77, key.Value);
        }

        [Fact]
        public void Returns_null_if_value_in_buffer_is_null()
        {
            var model = BuildModel();
            var type = model.FindEntityType(typeof(Banana));

            Assert.True(
                new SimpleKeyValueFactory<int>(type.FindPrimaryKey())
                    .Create(new[] { type.FindProperty("P2") }, new ValueBuffer(new object[] { 7, null })).IsInvalid);
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

            return model;
        }

        private class Banana
        {
            public int P1 { get; set; }
            public int? P2 { get; set; }
        }
    }
}
