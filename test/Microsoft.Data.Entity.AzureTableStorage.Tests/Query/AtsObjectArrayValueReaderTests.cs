// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class AtsObjectArrayValueReaderTests
    {
        [Fact]
        public void It_reads_int_from_string()
        {
            var reader = new AtsObjectArrayValueReader(new object[] { "3249" });
            Assert.Equal(3249, reader.ReadValue<int>(0));
            Assert.Equal(3249, reader.ReadValue<Int32>(0));
        }

        [Fact]
        public void It_reads_guids_from_string()
        {
            var reader = new AtsObjectArrayValueReader(new object[] { "9dc902e2-be1c-4377-88ff-9569f1db0f29" });
            Assert.Equal(new Guid("9dc902e2-be1c-4377-88ff-9569f1db0f29"), reader.ReadValue<Guid>(0));
        }

        [Fact]
        public void It_reads_long_from_string()
        {
            var reader = new AtsObjectArrayValueReader(new object[] { "3248932498387543324" });
            Assert.Equal(3248932498387543324, reader.ReadValue<long>(0));
            Assert.Equal(3248932498387543324, reader.ReadValue<Int64>(0));
        }

        [Fact]
        public void It_reads_dates_from_string()
        {
            var reader = new AtsObjectArrayValueReader(new object[] { "11/11/2011 8:11:11.000000 AM -3:00"});
            Assert.Equal(new DateTimeOffset(2011, 11, 11, 11, 11, 11, 0, TimeSpan.Zero), reader.ReadValue<DateTimeOffset>(0));
            Assert.Equal(new DateTime(2011, 11, 11, 11, 11, 11, 0, DateTimeKind.Utc), reader.ReadValue<DateTime>(0));
        }

        [Fact]
        public void It_reads_doubles_from_string()
        {
            var reader = new AtsObjectArrayValueReader(new object[] { "3.14159" });
            Assert.Equal(3.14159,reader.ReadValue<double>(0));
        }

        [Fact]
        public void It_identifies_nulls()
        {
            int? i = null;
            PocoTestType p = null;

            var reader = new AtsObjectArrayValueReader(new object[] { null, i, p });
            Assert.Equal(3,reader.Count);
            for (var idx = 0; idx < reader.Count; idx++)
            {
                Assert.True(reader.IsNull(idx));
                Assert.Null(reader.ReadValue<object>(idx));
            }
        }

        [Fact]
        public void Throws_if_cannot_parse()
        {
            var reader = new AtsObjectArrayValueReader(new object[] { "value" });
            Assert.Throws<InvalidCastException>(() => reader.ReadValue<bool>(0));
        }
    }
}