// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class IndexedPropertySetterFactoryTest
    {
        [Fact]
        public void Delegate_setter_is_returned_for_indexed_property()
        {
            var entityType = new Model().AddEntityType(typeof(IndexedClass));
            var idProperty = entityType.AddProperty("Id", typeof(int));
            var propertyA = entityType.AddIndexedProperty("PropertyA", typeof(string));
            var propertyB = entityType.AddIndexedProperty("PropertyB", typeof(int));

            var indexedClass = new IndexedClass
            {
                Id = 1
            };

            new IndexedPropertySetterFactory().Create(propertyA).SetClrValue(indexedClass, "UpdatedValueA");
            new IndexedPropertySetterFactory().Create(propertyB).SetClrValue(indexedClass, 456);

            Assert.Equal("UpdatedValueA", indexedClass["PropertyA"]);
            Assert.Equal(456, indexedClass["PropertyB"]);
        }

        [Fact]
        public void Exception_is_returned_when_setting_indexed_property_without_indexer()
        {
            var entityType = new Model().AddEntityType(typeof(NonIndexedClass));
            var idProperty = entityType.AddProperty("Id", typeof(int));
            var propertyA = entityType.AddIndexedProperty("PropertyA", typeof(string));
            var propertyB = entityType.AddIndexedProperty("PropertyB", typeof(int));

            var indexedClass = new NonIndexedClass
            {
                Id = 1,
                PropA = "PropAValue",
                PropB = 123
            };

            Assert.Throws<InvalidOperationException>(
                () => new IndexedPropertySetterFactory().Create(propertyA).SetClrValue(indexedClass, "UpdatedValueA"));
            Assert.Throws<InvalidOperationException>(
                () => new IndexedPropertySetterFactory().Create(propertyB).SetClrValue(indexedClass, 456));
        }

        private class IndexedClass
        {
            private Dictionary<string, object> _internalValues = new Dictionary<string, object>()
                {
                    { "PropertyA", "ValueA" },
                    { "PropertyB", 123 }
                };

            internal int Id { get; set; }

            public object this[string name]
            {
                get => _internalValues[name];
                set => _internalValues[name] = value;
            }
        }

        private class NonIndexedClass
        {
            internal int Id { get; set; }
            public string PropA { get; set; }
            public int PropB { get; set; }
        }
    }
}
