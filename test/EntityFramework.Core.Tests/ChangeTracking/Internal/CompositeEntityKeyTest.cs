// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class CompositeEntityKeyTest
    {
        [Fact]
        public void Value_property_is_strongly_typed()
        {
            Assert.IsType<object[]>(new CompositeKeyValue(new Mock<IKey>().Object, new object[] { 77, "Kake" }).Value);
        }

        [Fact]
        public void Base_class_value_property_returns_same_as_strongly_typed_value_property()
        {
            var key = new CompositeKeyValue(new Mock<IKey>().Object, new object[] { 77, "Kake" });

            Assert.Equal(new object[] { 77, "Kake" }, key.Value);
            Assert.Equal(new object[] { 77, "Kake" }, (object[])((IKeyValue)key).Value);
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_entity_type_test_as_equal()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.True(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(new CompositeKeyValue(key1, new object[] { 77, "Kake" })));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(null));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77 }).Equals(new SimpleKeyValue<int>(key1, 77)));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(new CompositeKeyValue(key1, new object[] { 77, "Lie" })));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(new CompositeKeyValue(key1, new object[] { 77L, "Kake" })));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(new CompositeKeyValue(key1, new object[] { null, "Kake" })));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(new CompositeKeyValue(key1, new object[] { 77, "Kake", 42 })));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(new CompositeKeyValue(key1, new object[] { 88, "Kake" })));
            Assert.False(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).Equals(new CompositeKeyValue(key2, new object[] { 77, "Kake" })));
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_type_return_same_hashcode()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.Equal(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).GetHashCode(), new CompositeKeyValue(key1, new object[] { 77, "Kake" }).GetHashCode());
            Assert.NotEqual(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).GetHashCode(), new CompositeKeyValue(key1, new object[] { 77, "Lie" }).GetHashCode());
            Assert.NotEqual(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).GetHashCode(), new CompositeKeyValue(key1, new object[] { null, "Kake" }).GetHashCode());
            Assert.NotEqual(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).GetHashCode(), new CompositeKeyValue(key1, new object[] { 77, "Kake", 42 }).GetHashCode());
            Assert.NotEqual(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).GetHashCode(), new CompositeKeyValue(key1, new object[] { 88, "Kake" }).GetHashCode());
            Assert.NotEqual(new CompositeKeyValue(key1, new object[] { 77, "Kake" }).GetHashCode(), new CompositeKeyValue(key2, new object[] { 77, "Kake" }).GetHashCode());
        }

        [Fact]
        public void Uses_structural_comparisons_for_array_matching()
        {
            var key = new Mock<IKey>().Object;

            Assert.True(new CompositeKeyValue(key, new object[] { new Byte[] { 1, 2, 3 } }).Equals(new CompositeKeyValue(key, new object[] { new Byte[] { 1, 2, 3 } })));
            Assert.False(new CompositeKeyValue(key, new object[] { new Byte[] { 1, 2, 3 } }).Equals(new CompositeKeyValue(key, new object[] { new Byte[] { 3, 2, 3 } })));
        }

        [Fact]
        public void Uses_structural_comparisons_for_array_hashcode_generation()
        {
            var key = new Mock<IKey>().Object;

            Assert.Equal(new CompositeKeyValue(key, new object[] { new Byte[] { 1, 2, 3 } }).GetHashCode(), new CompositeKeyValue(key, new object[] { new Byte[] { 1, 2, 3 } }).GetHashCode());
            Assert.NotEqual(new CompositeKeyValue(key, new object[] { new Byte[] { 1, 2, 3 } }).GetHashCode(), new CompositeKeyValue(key, new object[] { new Byte[] { 3, 2, 3 } }).GetHashCode());
        }
    }
}
