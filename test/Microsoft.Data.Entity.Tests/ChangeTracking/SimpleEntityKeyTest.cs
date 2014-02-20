// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class SimpleEntityKeyTest
    {
        [Fact]
        public void Value_property_is_strongly_typed()
        {
            Assert.IsType<int>(new SimpleEntityKey<object, int>(77).Value);
        }

        [Fact]
        public void Base_class_value_property_returns_same_as_strongly_typed_value_property()
        {
            var key = new SimpleEntityKey<object, int>(77);

            Assert.Equal(77, key.Value);
            Assert.Equal(77, ((EntityKey)key).Value);
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_entity_type_test_as_equal()
        {
            Assert.True(new SimpleEntityKey<object, int>(77).Equals(new SimpleEntityKey<object, int>(77)));
            Assert.False(new SimpleEntityKey<object, int>(77).Equals(new SimpleEntityKey<object, int>(88)));
            Assert.False(new SimpleEntityKey<object, int>(77).Equals(new SimpleEntityKey<Random, int>(77)));
        }

        [Fact]
        public void Only_keys_with_the_same_value_and_type_return_same_hashcode()
        {
            Assert.Equal(new SimpleEntityKey<object, int>(77).GetHashCode(), new SimpleEntityKey<object, int>(77).GetHashCode());
            Assert.NotEqual(new SimpleEntityKey<object, int>(77).GetHashCode(), new SimpleEntityKey<object, int>(88).GetHashCode());
            Assert.NotEqual(new SimpleEntityKey<object, int>(77).GetHashCode(), new SimpleEntityKey<Random, int>(77).GetHashCode());
        }
    }
}
