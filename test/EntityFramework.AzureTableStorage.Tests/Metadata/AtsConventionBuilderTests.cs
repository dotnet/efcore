// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class AtsConventionBuilderTests
    {
        public class NoAtsContext : DbContext
        {
            public DbSet<IntKeysPoco> IntKeysPocos { get; set; }

            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseModel(new Model());
            }
        }

        public class WithAtsContext : DbContext
        {
            public DbSet<IntKeysPoco> IntKeysPocos { get; set; }

            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseAzureTableStorage("Papa", "Smurf");
            }
        }

        public class AtsClrContext : DbContext
        {
            public DbSet<ClrPoco> ClrPocos { get; set; }

            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseAzureTableStorage("Papa", "Smurf");
            }
        }

        [Fact]
        public void It_ignores_non_ats_contexts()
        {
            var model = new NoAtsContext().Model;
            Assert.False(model.EntityTypes.Any(e => e.TryGetProperty("ETag") != null));
            Assert.False(model.EntityTypes.Any(e => e.TryGetPropertyByColumnName("PartitionKey") != null));
            Assert.False(model.EntityTypes.Any(e => e.TryGetPropertyByColumnName("RowKey") != null));
        }

        [Fact]
        public void Adds_composite_key()
        {
            var model = new AtsClrContext().Model;
            var key = model.GetEntityType(typeof(ClrPoco)).GetPrimaryKey();
            Assert.Equal(2, key.Properties.Count);
            Assert.Contains("PartitionKey", key.Properties.Select(p => p.AzureTableStorage().Column));
            Assert.Contains("RowKey", key.Properties.Select(p => p.AzureTableStorage().Column));
        }

        [Fact]
        public void Adds_etag()
        {
            var model = new AtsClrContext().Model;
            var etagProp = model.GetEntityType(typeof(ClrPoco)).TryGetProperty("ETag");
            Assert.NotNull(etagProp);
        }
    }
}
