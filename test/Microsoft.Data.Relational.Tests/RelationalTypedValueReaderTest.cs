// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using Moq;
using Xunit;

namespace Microsoft.Data.Relational.Tests
{
    public class RelationalTypedValueReaderTest
    {
        [Fact]
        public void IsNull_delegates_to_IsDBNull()
        {
            var readerMock = new Mock<DbDataReader>();
            readerMock.Setup(m => m.IsDBNull(0)).Returns(true);
            readerMock.Setup(m => m.IsDBNull(1)).Returns(false);

            var reader = new RelationalTypedValueReader(readerMock.Object);

            Assert.True(reader.IsNull(0));
            Assert.False(reader.IsNull(1));
        }

        [Fact]
        public void Can_read_value()
        {
            var readerMock = new Mock<DbDataReader>();
            readerMock.Setup(m => m.GetFieldValue<int>(0)).Returns(77);
            readerMock.Setup(m => m.GetFieldValue<string>(1)).Returns("Smokey");

            var reader = new RelationalTypedValueReader(readerMock.Object);

            Assert.Equal(77, reader.ReadValue<int>(0));
            Assert.Equal("Smokey", reader.ReadValue<string>(1));
        }

        [Fact]
        public void Can_get_count()
        {
            var readerMock = new Mock<DbDataReader>();
            readerMock.Setup(m => m.FieldCount).Returns(2);

            var reader = new RelationalTypedValueReader(readerMock.Object);

            Assert.Equal(2, reader.Count);
        }
    }
}
