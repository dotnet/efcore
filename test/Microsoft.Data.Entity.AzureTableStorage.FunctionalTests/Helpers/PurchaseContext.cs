// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.Helpers
{
    public class PurchaseContext : DbContext
    {
        private readonly string _tableName;
        private bool _batching;

        public PurchaseContext(string tableName, bool batching = false)
        {
            _tableName = tableName;
            _batching = batching;
        }

        public DbSet<Purchase> Purchases { get; set; }

        protected override void OnConfiguring(DbContextOptions builder)
        {
            builder.UseAzureTableStorage(ConfigurationManager.AppSettings["TestConnectionString"], _batching);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
                .Entity<Purchase>()
                .UseDefaultAzureTableKey()
                .StorageName(_tableName);
        }
    }
}
