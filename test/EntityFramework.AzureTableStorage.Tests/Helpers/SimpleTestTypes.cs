// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class IntKeysPoco
    {
        public int PartitionID { get; set; }
        public int RowID { get; set; }

        public static EntityType EntityType()
        {
            var entityType = new EntityType(typeof(IntKeysPoco));
            entityType.AddProperty("PartitionID", typeof(int)).SetColumnName("PartitionKey");
            entityType.AddProperty("RowID", typeof(int)).SetColumnName("RowKey");
            return entityType;
        }
    }

    public class NullablePoco
    {
        public int? NullInt { get; set; }
        public double? NullDouble { get; set; }

        public static EntityType EntityType()
        {
            var entityType = new EntityType(typeof(NullablePoco));
            entityType.AddProperty("NullInt", typeof(int?));
            entityType.AddProperty("NullDouble", typeof(double?));
            return entityType;
        }
    }
    
    public class ClrPoco
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public class GuidKeysPoco
    {
        public Guid PartitionGuid { get; set; }
        public Guid RowGuid { get; set; }
    }

    public class ClrPocoWithProp : ClrPoco
    {
        public string StringProp { get; set; }
        public int IntProp { get; set; }
    }
}
