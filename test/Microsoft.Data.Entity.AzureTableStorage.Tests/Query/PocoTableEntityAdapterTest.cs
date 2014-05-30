// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class PocoTableEntityAdapterTest
    {
        public class GoodPoco
        {
            public string PartitionKey { get; set; }

            public string RowKey { get; set; }
        }

        private class PrivateButGoodPoco
        {
            public string PartitionKey { get; set; }

            public string RowKey { get; set; }
        }

        [Theory]
        [InlineData(typeof(GoodPoco))]
        [InlineData(typeof(PrivateButGoodPoco))]
        public void It_wraps_poco_in_adapter(Type type)
        {
            var p = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            var adapterType = typeof(PocoTableEntityAdapter<>).MakeGenericType(type);
            Assert.DoesNotThrow(() => { adapterType.GetMethod("CheckProperties").Invoke(null, null); });
        }

        public class NoParitionKeyPoco
        {
            public string RowKey { get; set; }
        }

        public class NoRowKeyPoco
        {
            public string PartitionKey { get; set; }
        }

        public class NoPublicGettersPoco
        {
            public string PartitionKey { private get; set; }
            public string RowKey { private get; set; }
        }

        public class NoPublicSettersPoco
        {
            public string PartitionKey { get; private set; }
            public string RowKey { get; private set; }
        }

        [Theory]
        [InlineData(typeof(NoParitionKeyPoco))]
        [InlineData(typeof(NoRowKeyPoco))]
        [InlineData(typeof(NoPublicGettersPoco))]
        [InlineData(typeof(NoPublicSettersPoco))]
        public void It_checks_for_keys(Type type)
        {
            var p = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            var adapterType = typeof(PocoTableEntityAdapter<>).MakeGenericType(type);
            Assert.ThrowsAny<Exception>(() => { adapterType.GetMethod("CheckProperties").Invoke(null, null); });
        }

        [Fact]
        public void It_adapts_reserved_properties()
        {
            var time = new DateTimeOffset(123456, TimeSpan.Zero);
            var p = new PocoTestType() { PartitionKey = "abc", RowKey = "def", ETag = "ghi", Timestamp = time };
            var w = new PocoTableEntityAdapter<PocoTestType>(p);
            Assert.Equal(p.PartitionKey, w.PartitionKey);
            Assert.Equal(p.RowKey, w.RowKey);
            Assert.Equal(p.Timestamp, w.Timestamp);
            Assert.Equal(p.ETag, w.ETag);

            p.PartitionKey = "zyx";
            p.RowKey = "wvu";
            p.ETag = "tsr";
            p.Timestamp = new DateTimeOffset(987654, TimeSpan.Zero);

            Assert.Equal(p.PartitionKey, w.PartitionKey);
            Assert.Equal(p.RowKey, w.RowKey);
            Assert.Equal(p.Timestamp, w.Timestamp);
            Assert.Equal(p.ETag, w.ETag);

            w.PartitionKey = "zyx";
            w.RowKey = "wvu";
            w.ETag = "tsr";
            w.Timestamp = new DateTimeOffset(987654, TimeSpan.Zero);

            Assert.Equal(p.PartitionKey, w.PartitionKey);
            Assert.Equal(p.RowKey, w.RowKey);
            Assert.Equal(p.Timestamp, w.Timestamp);
            Assert.Equal(p.ETag, w.ETag);
        }
    }
}
