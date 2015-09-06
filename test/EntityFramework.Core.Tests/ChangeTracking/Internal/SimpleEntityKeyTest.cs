// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class SimpleEntityKeyTest
    {
        [Fact]
        public void Value_property_is_strongly_typed()
        {
            Assert.IsType<int>(new SimpleEntityKey<int>(new Mock<IKey>().Object, 77).Value);
        }

        [Fact]
        public void Base_class_value_property_returns_same_as_strongly_typed_value_property()
        {
            var key = new SimpleEntityKey<int>(new Mock<IKey>().Object, 77);

            Assert.Equal(77, key.Value);
            Assert.Equal(77, ((EntityKey)key).Value);
        }

        [Fact]
        public void Only_keys_with_the_same_value_type_and_entity_type_test_as_equal()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.True(new SimpleEntityKey<int>(key1, 77).Equals(new SimpleEntityKey<int>(key1, 77)));
            Assert.False(new SimpleEntityKey<int>(key1, 77).Equals(null));
            Assert.False(new SimpleEntityKey<int>(key1, 77).Equals(new CompositeEntityKey(key1, new object[] { 77 })));
            Assert.False(new SimpleEntityKey<int>(key1, 77).Equals(new SimpleEntityKey<int>(key1, 88)));
            Assert.False(new SimpleEntityKey<int>(key1, 77).Equals(new SimpleEntityKey<long>(key1, 77)));
            Assert.False(new SimpleEntityKey<int>(key1, 77).Equals(new SimpleEntityKey<int>(key2, 77)));
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_type_return_same_hashcode()
        {
            var key1 = new Mock<IKey>().Object;
            var key2 = new Mock<IKey>().Object;

            Assert.Equal(new SimpleEntityKey<int>(key1, 77).GetHashCode(), new SimpleEntityKey<int>(key1, 77).GetHashCode());
            Assert.NotEqual(new SimpleEntityKey<int>(key1, 77).GetHashCode(), new SimpleEntityKey<int>(key1, 88).GetHashCode());
            Assert.NotEqual(new SimpleEntityKey<int>(key1, 77).GetHashCode(), new SimpleEntityKey<int>(key2, 77).GetHashCode());
        }
    }
}
