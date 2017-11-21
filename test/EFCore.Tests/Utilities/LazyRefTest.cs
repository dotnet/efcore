// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class LazyRefTest
    {
        [Fact]
        public void Has_value_is_false_until_value_accessed()
        {
            var lazy = new LazyRef<string>(() => "Cherry Coke");

            Assert.False(lazy.HasValue);
            Assert.Equal("Cherry Coke", lazy.Value);
            Assert.True(lazy.HasValue);
        }

        [Fact]
        public void Has_value_is_true_if_constructed_with_value()
        {
            var lazy = new LazyRef<string>((string)null);

            Assert.True(lazy.HasValue);
            Assert.Null(lazy.Value);
        }

        [Fact]
        public void Value_can_be_set_explicitly()
        {
            var lazy = new LazyRef<string>(() => "Cherry Coke")
            {
                Value = "Fresca"
            };

            Assert.True(lazy.HasValue);
            Assert.Equal("Fresca", lazy.Value);
        }

        [Fact]
        public void Initialization_can_be_reset()
        {
            var lazy = new LazyRef<string>(() => "Cherry Coke");

            Assert.Equal("Cherry Coke", lazy.Value);

            lazy.Reset(() => "Fresca");

            Assert.False(lazy.HasValue);
            Assert.Equal("Fresca", lazy.Value);
            Assert.True(lazy.HasValue);
        }
    }
}
