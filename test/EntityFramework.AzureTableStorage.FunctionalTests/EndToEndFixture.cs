// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class EndToEndFixture
    {
        public DbContext CreateContext(string tableName)
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel(tableName))
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString);
            return new DbContext(options);
        }

        public IModel CreateModel(string tableName)
        {
            var model = new Model();
            var builder = new ModelBuilder(model);
            builder.Entity<Purchase>()
                .Properties(pb =>
                    {
                        pb.Property(s => s.Awesomeness);
                        pb.Property(s => s.Cost);
                        pb.Property(s => s.Count);
                        pb.Property(s => s.GlobalGuid);
                        pb.Property(s => s.Name);
                        pb.Property(s => s.PartitionKey);
                        pb.Property(s => s.Purchased);
                        pb.Property(s => s.RowKey);
                        pb.Property(s => s.Timestamp);
                    })
                .PartitionAndRowKey(s => s.PartitionKey, s => s.RowKey)
                .TableName(tableName);
            return model;
        }

        public static IEnumerable<Purchase> SampleData(string testPartition)
        {
            return new List<Purchase>
                {
                    new Purchase
                        {
                            PartitionKey = testPartition,
                            RowKey = "It_deletes_entity_test",
                            Purchased = DateTime.Now,
                        },
                    new Purchase
                        {
                            PartitionKey = testPartition,
                            RowKey = "Sample_entity",
                            Name = "Sample",
                            GlobalGuid = new Guid(),
                            Cost = -234.543,
                            Count = 359,
                            Purchased = DateTime.Parse("Tue, 1 Jan 2013 22:11:20 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                            Awesomeness = true,
                        },
                };
        }
    }

    public class Purchase
    {
        protected bool Equals(Purchase other)
        {
            // intentionally leaves out Timestamp (changed by server)
            return string.Equals(PartitionKey, other.PartitionKey) && string.Equals(RowKey, other.RowKey) && Cost.Equals(other.Cost) && string.Equals(Name, other.Name) && Purchased.Equals(other.Purchased) && Count == other.Count && GlobalGuid.Equals(other.GlobalGuid) && Awesomeness.Equals(other.Awesomeness);
        }

        public static bool operator ==(Purchase left, Purchase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Purchase left, Purchase right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (PartitionKey != null ? PartitionKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RowKey != null ? RowKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ Cost.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Purchased.GetHashCode();
                hashCode = (hashCode * 397) ^ Count;
                hashCode = (hashCode * 397) ^ GlobalGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ Awesomeness.GetHashCode();
                return hashCode;
            }
        }

        public Purchase()
        {
            Purchased = DateTime.Parse("Jan 1, 1601 00:00:00 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public double Cost { get; set; }
        public string Name { get; set; }
        public DateTime Purchased { get; set; }
        public int Count { get; set; }
        public Guid GlobalGuid { get; set; }
        public bool Awesomeness { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Purchase)obj);
        }
    }
}
