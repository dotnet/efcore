// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class TestFixture : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _tableSuffix = Guid.NewGuid().ToString().Replace("-", "");

        public TestFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddAzureTableStorage()
                .ServiceCollection
                .AddTestModelSource(OnModelCreating)
                .BuildServiceProvider();
        }

        public AtsTestStore CreateTestStore(string testPartition)
        {
            var store = new AtsTestStore(_tableSuffix);
            using (var context = CreateContext(store))
            {
                context.Database.EnsureCreated();
                Seed(context, testPartition);
            }

            store.CleanupAction = () =>
            {
                using (var context = CreateContext(store))
                {
                    Cleanup(context, testPartition);
                }
            };

            return store;
        }

        public DbContext CreateContext(AtsTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseAzureTableStorage(testStore.ConnectionString);

            return new DbContext(_serviceProvider, options);
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Purchase>(b =>
                {
                    b.Property(s => s.Awesomeness);
                    b.Property(s => s.Cost);
                    b.Property(s => s.Count);
                    b.Property(s => s.GlobalGuid);
                    b.Property(s => s.Name);
                    b.Property(s => s.PartitionKey);
                    b.Property(s => s.Purchased);
                    b.Property(s => s.RowKey);
                    b.Property(s => s.Timestamp);
                    b.Property(s => s.ETag);
                    b.ForAzureTableStorage(ab =>
                        {
                            ab.PartitionAndRowKey(s => s.PartitionKey, s => s.RowKey);
                            ab.Table("Purchase" + _tableSuffix);
                        });
                });
        }

        public void Dispose()
        {
            using (var testStore = CreateTestStore(""))
            {
                using (var context = CreateContext(testStore))
                {
                    context.Database.EnsureDeleted();
                }
            }
        }

        public void Seed(DbContext context, string testPartition)
        {
            context.Set<Purchase>().AddRange(SampleData(testPartition));
            context.SaveChanges();
        }

        public void Cleanup(DbContext context, string testPartition)
        {
            context.Set<Purchase>().RemoveRange(context.Set<Purchase>().Where(p => p.PartitionKey == testPartition));
            context.SaveChanges();
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
                    new Purchase
                        {
                            PartitionKey = testPartition,
                            RowKey = "Concurrency_entity",
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
        public string ETag { get; set; }

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
