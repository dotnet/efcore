// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class TransactionFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract DbContext CreateContext(TTestStore testStore);

        public abstract DbContext CreateContext(DbConnection connection);

        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionCustomer>(ps =>
                {
                    ps.Property(c => c.Id).StoreGeneratedPattern(StoreGeneratedPattern.None);
                    ps.ToTable("Customers");
                });
        }

        protected void Seed(DbContext context)
        {
            context.Database.EnsureCreated();

            foreach (var customer in Customers)
            {
                context.Add(customer);
            }

            context.SaveChanges();
        }

        public readonly IReadOnlyList<TransactionCustomer> Customers = new List<TransactionCustomer>
        {
            new TransactionCustomer
            {
                Id = 1,
                Name = "Bob"
            },
            new TransactionCustomer
            {
                Id = 2,
                Name = "Dave"
            }
        };
    }

    public class TransactionCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var otherCustomer = obj as TransactionCustomer;
            if (otherCustomer == null)
            {
                return false;
            }

            return Id == otherCustomer.Id
                   && Name == otherCustomer.Name;
        }

        public override string ToString() => "Id = " + Id + ", Name = " + Name;

        public override int GetHashCode() => Id.GetHashCode() * 397 ^ Name.GetHashCode();
    }
}
