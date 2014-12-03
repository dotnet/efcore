// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ObjectArrayValueReaderTest
    {
        [Fact]
        public void IsNull_returns_true_only_if_value_is_null()
        {
            var reader = new ObjectArrayValueReader(new object[] { null, "" });

            Assert.True(reader.IsNull(0));
            Assert.False(reader.IsNull(1));
        }

        [Fact]
        public void Can_read_value()
        {
            var reader = new ObjectArrayValueReader(new object[] { 77, "Smokey" });

            Assert.Equal(77, reader.ReadValue<int>(0));
            Assert.Equal("Smokey", reader.ReadValue<string>(1));
        }

        [Fact]
        public void Can_get_count()
        {
            var reader = new ObjectArrayValueReader(new object[] { 77, "Smokey" });

            Assert.Equal(2, reader.Count);
        }
    }
}
