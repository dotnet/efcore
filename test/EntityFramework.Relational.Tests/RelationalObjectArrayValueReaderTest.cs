// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalObjectArrayValueReaderTest
    {
        [Fact]
        public void IsNull_returns_true_for_DBNull()
        {
            var reader = new RelationalObjectArrayValueReader(new object[] { DBNull.Value, "Smokey" }, 0);

            Assert.True(reader.IsNull(0));
            Assert.False(reader.IsNull(1));
        }

        [Fact]
        public void Can_read_value()
        {
            var reader = new RelationalObjectArrayValueReader(new object[] { 77, "Smokey" }, 0);

            Assert.Equal(77, reader.ReadValue<int>(0));
            Assert.Equal("Smokey", reader.ReadValue<string>(1));
        }

        [Fact]
        public void Can_get_count()
        {
            var reader = new RelationalObjectArrayValueReader(new object[] { 77, "Smokey" }, 0);

            Assert.Equal(2, reader.Count);
        }
    }
}
