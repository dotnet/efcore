// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalObjectArrayValueReaderTest
    {
        [Fact]
        public void IsNull_returns_true_for_DBNull()
        {
            var reader = new RelationalObjectArrayValueReader(CreateDataReader(DBNull.Value, "Smokey"));

            Assert.True(reader.IsNull(0));
            Assert.False(reader.IsNull(1));
        }

        [Fact]
        public void Can_read_value()
        {
            var reader = new RelationalObjectArrayValueReader(CreateDataReader(77, "Smokey"));

            Assert.Equal(77, reader.ReadValue<int>(0));
            Assert.Equal("Smokey", reader.ReadValue<string>(1));
        }

        [Fact]
        public void Can_get_count()
        {
            var reader = new RelationalObjectArrayValueReader(CreateDataReader(77, "Smokey"));

            Assert.Equal(2, reader.Count);
        }

        private static DbDataReader CreateDataReader(params object[] values)
        {
            var readerMock = new Mock<DbDataReader>();
            readerMock.Setup(m => m.FieldCount).Returns(2);
            readerMock.Setup(m => m.GetValues(It.IsAny<object[]>()))
                .Callback<object[]>(b =>
                    {
                        b[0] = values[0];
                        b[1] = values[1];
                    });

            var dataReader = readerMock.Object;
            return dataReader;
        }
    }
}
