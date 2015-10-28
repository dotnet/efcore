// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

// ReSharper disable ConvertMethodToExpressionBody

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class EntityKeyTest
    {
        [Fact]
        public void Value_property_is_strongly_typed()
        {
            Assert.IsType<int>(new KeyValue<int>(new Mock<IKey>().Object, 77).Value);
        }

        [Fact]
        public void Base_class_value_property_returns_same_as_strongly_typed_value_property()
        {
            var key = new KeyValue<int>(new Mock<IKey>().Object, 77);

            Assert.Equal(77, key.Value);
            Assert.Equal(77, ((IKeyValue)key).Value);
        }

        [Fact]
        public void Only_keys_with_the_same_value_type_and_entity_type_test_as_equal()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.Equal(new KeyValue<int>(key1, 77), new KeyValue<int>(key1, 77));
            Assert.NotEqual<IKeyValue>(new KeyValue<int>(key1, 77), new KeyValue<object[]>(key1, new object[] { 77 }));
            Assert.NotEqual(new KeyValue<int>(key1, 77), new KeyValue<int>(key1, 88));
            Assert.NotEqual<IKeyValue>(new KeyValue<int>(key1, 77), new KeyValue<long>(key1, 77));
            Assert.NotEqual(new KeyValue<int>(key1, 77), new KeyValue<int>(key2, 77));
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_type_return_same_hashcode_when_composite()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.Equal(new KeyValue<int>(key1, 77).GetHashCode(), new KeyValue<int>(key1, 77).GetHashCode());
            Assert.NotEqual(new KeyValue<int>(key1, 77).GetHashCode(), new KeyValue<int>(key1, 88).GetHashCode());
            Assert.NotEqual(new KeyValue<int>(key1, 77).GetHashCode(), new KeyValue<int>(key2, 77).GetHashCode());
        }

        [Fact]
        public void Value_property_is_strongly_typed_when_composite()
        {
            Assert.IsType<object[]>(new KeyValue<object[]>(new Mock<IKey>().Object, new object[] { 77, "Kake" }).Value);
        }

        [Fact]
        public void Base_class_value_property_returns_same_as_strongly_typed_value_property_when_composite()
        {
            var key = new KeyValue<object[]>(new Mock<IKey>().Object, new object[] { 77, "Kake" });

            Assert.Equal(new object[] { 77, "Kake" }, key.Value);
            Assert.Equal(new object[] { 77, "Kake" }, (object[])((IKeyValue)key).Value);
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_entity_type_test_as_equal()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.Equal(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }),
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }));

            Assert.NotEqual<IKeyValue>(
                new KeyValue<object[]>(key1, new object[] { 77 }),
                new KeyValue<int>(key1, 77));

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }),
                new KeyValue<object[]>(key1, new object[] { 77, "Lie" }));

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }),
                new KeyValue<object[]>(key1, new object[] { 77L, "Kake" }));

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }),
                new KeyValue<object[]>(key1, new object[] { null, "Kake" }));

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }),
                new KeyValue<object[]>(key1, new object[] { 77, "Kake", 42 }));

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }),
                new KeyValue<object[]>(key1, new object[] { 88, "Kake" }));

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }),
                new KeyValue<object[]>(key2, new object[] { 77, "Kake" }));
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_type_return_same_hashcode()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.Equal(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }).GetHashCode(),
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }).GetHashCode());

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }).GetHashCode(),
                new KeyValue<object[]>(key1, new object[] { 77, "Lie" }).GetHashCode());

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }).GetHashCode(),
                new KeyValue<object[]>(key1, new object[] { null, "Kake" }).GetHashCode());

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }).GetHashCode(),
                new KeyValue<object[]>(key1, new object[] { 77, "Kake", 42 }).GetHashCode());

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }).GetHashCode(),
                new KeyValue<object[]>(key1, new object[] { 88, "Kake" }).GetHashCode());

            Assert.NotEqual(
                new KeyValue<object[]>(key1, new object[] { 77, "Kake" }).GetHashCode(),
                new KeyValue<object[]>(key2, new object[] { 77, "Kake" }).GetHashCode());
        }

        [Fact]
        public void Uses_structural_comparisons_for_array_matching()
        {
            var key = new Mock<IKey>().Object;

            Assert.Equal(
                new KeyValue<object[]>(key, new object[] { new byte[] { 1, 2, 3 } }),
                new KeyValue<object[]>(key, new object[] { new byte[] { 1, 2, 3 } }));

            Assert.NotEqual(
                new KeyValue<object[]>(key, new object[] { new byte[] { 1, 2, 3 } }),
                new KeyValue<object[]>(key, new object[] { new byte[] { 3, 2, 3 } }));
        }

        [Fact]
        public void Uses_structural_comparisons_for_array_hashcode_generation()
        {
            var key = new Mock<IKey>().Object;

            Assert.Equal(
                new KeyValue<object[]>(key, new object[] { new byte[] { 1, 2, 3 } }).GetHashCode(),
                new KeyValue<object[]>(key, new object[] { new byte[] { 1, 2, 3 } }).GetHashCode());

            Assert.NotEqual(
                new KeyValue<object[]>(key, new object[] { new byte[] { 1, 2, 3 } }).GetHashCode(),
                new KeyValue<object[]>(key, new object[] { new byte[] { 3, 2, 3 } }).GetHashCode());
        }
    }
}
