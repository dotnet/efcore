// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class AtsNamedValueBufferTests
    {
        [Fact]
        public void It_constructs_from_entity_dictionary()
        {
            var data = new Dictionary<string, EntityProperty>
                {
                    { "string", new EntityProperty("string") },
                    { "double", new EntityProperty(324.34d) },
                    { "byte", new EntityProperty(new byte[] { 4, 8, 15, 16, 23, 42 }) },
                    { "long", new EntityProperty(429496729623) }
                };
            var buffer = new AtsNamedValueBuffer(data);
            foreach (var property in data)
            {
                Assert.Equal(data[property.Key].PropertyAsObject, buffer[property.Key]);
            }
        }

        [Fact]
        public void Indexed_by_name()
        {
            var buffer = new AtsNamedValueBuffer(new Dictionary<string, EntityProperty>());

            Assert.Throws<KeyNotFoundException>(() => buffer["Key"]);
            Assert.Null(buffer.TryGet("Key"));

            Assert.DoesNotThrow(() => buffer.Add("Key", "value"));

            Assert.DoesNotThrow(() => buffer["Key"]);
            Assert.NotNull(buffer.TryGet("Key"));
        }

        [Fact]
        public void Inserts_strings_and_timestamp()
        {
            var buffer = new AtsNamedValueBuffer(new Dictionary<string, EntityProperty>());
            var stamp = DateTime.UtcNow;
            buffer.Add("RowKey", "string data");
            buffer.Add("Timestamp", stamp);
            Assert.Equal("string data", buffer["RowKey"]);
            Assert.Equal(stamp, buffer["Timestamp"]);
        }
    }
}
