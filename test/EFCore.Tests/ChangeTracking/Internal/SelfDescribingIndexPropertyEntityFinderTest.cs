// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class SelfDescribingIndexPropertyEntityFinderTest
    {
        [Fact]
        public void Can_find_EntityType_using_default_EntityTypeNamePropertyName()
        {
            var model = new Model();
            var entityType = model.AddSharedTypeEntityType("TestDictionaryEntityType", typeof(Dictionary<string, object>));
            var idProperty = entityType.AddIndexedProperty("Id", typeof(int));
            idProperty.IsNullable = false;
            var propA = entityType.AddIndexedProperty("PropA", typeof(string));

            var instance = new Dictionary<string, object>()
            {
                { "__EntityTypeName__", "TestDictionaryEntityType" },
                { "Id", 0 },
                { "PropA", "PropAValue"},
            };

            var finder = new SelfDescribingIndexPropertyEntityFinder(model);

            Assert.Equal(entityType, finder.FindSharedTypeEntityType(instance));
        }

        [Fact]
        public void Can_find_EntityType_using_user_defined_EntityTypeNamePropertyName()
        {
            var model = new Model();
            var entityType = model.AddSharedTypeEntityType("TestDictionaryEntityType", typeof(Dictionary<string, object>));
            var idProperty = entityType.AddIndexedProperty("Id", typeof(int));
            idProperty.IsNullable = false;
            var propA = entityType.AddIndexedProperty("PropA", typeof(string));

            var instance = new Dictionary<string, object>()
            {
                { "SelfDescribingProperty", "TestDictionaryEntityType" },
                { "Id", 0 },
                { "PropA", "PropAValue"},
            };

            var finder = new SelfDescribingIndexPropertyEntityFinder(model);
            finder.EntityTypeNamePropertyName = "SelfDescribingProperty";

            Assert.Equal(entityType, finder.FindSharedTypeEntityType(instance));
        }

        [Fact]
        public void Return_null_if_no_matching_EntityType_found()
        {
            var model = new Model();
            var entityType = model.AddSharedTypeEntityType("DifferentlyNamedEntityType", typeof(Dictionary<string, object>));
            var idProperty = entityType.AddIndexedProperty("Id", typeof(int));
            idProperty.IsNullable = false;
            var propA = entityType.AddIndexedProperty("PropA", typeof(string));

            var instance = new Dictionary<string, object>()
            {
                { "__EntityTypeName__", "TestDictionaryEntityType" },
                { "Id", 0 },
                { "PropA", "PropAValue"},
            };

            var finder = new SelfDescribingIndexPropertyEntityFinder(model);

            Assert.Null(finder.FindSharedTypeEntityType(instance));
        }
        [Fact]
        public void Return_null_if_find_non_shared_type_EntityType_with_same_name()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(NonSharedTypeEntity));
            var idProperty = entityType.AddProperty("Id", typeof(int));
            idProperty.IsNullable = false;
            var propA = entityType.AddProperty("PropA", typeof(string));

            var instance = new Dictionary<string, object>()
            {
                { "__EntityTypeName__", entityType.Name },
                { "Id", 0 },
                { "PropA", "PropAValue"},
            };

            var finder = new SelfDescribingIndexPropertyEntityFinder(model);

            Assert.Null(finder.FindSharedTypeEntityType(instance));
        }

        private class NonSharedTypeEntity
        {
            public int Id { get; set; }
            public string PropA { get; set; }
        }
    }
}
