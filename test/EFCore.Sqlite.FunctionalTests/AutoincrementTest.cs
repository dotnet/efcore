// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class AutoincrementTest : IClassFixture<AutoincrementTest.AutoincrementFixture>
    {
        public AutoincrementTest(AutoincrementFixture fixture) => Fixture = fixture;

        protected AutoincrementFixture Fixture { get; }

        [Fact]
        public void Autoincrement_prevents_reusing_rowid()
        {
            using (var context = CreateContext())
            {
                context.People.Add(
                    new PersonA
                    {
                        Name = "Bruce"
                    });
                context.SaveChanges();

                var hero = context.People.First(p => p.Id == 1);

                context.People.Remove(hero);
                context.SaveChanges();
                context.People.Add(
                    new PersonA
                    {
                        Name = "Batman"
                    });
                context.SaveChanges();
                var gone = context.People.FirstOrDefault(p => p.Id == 1);
                var begins = context.People.FirstOrDefault(p => p.Id == 2);

                Assert.Null(gone);
                Assert.NotNull(begins);
            }
        }

        private BatContext CreateContext() => (BatContext)Fixture.CreateContext();

        public class AutoincrementFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "AutoincrementTest";
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            protected override Type ContextType => typeof(BatContext);
        }

        protected class BatContext : PoolableDbContext
        {
            public BatContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<PersonA> People { get; set; }
        }

        protected class PersonA
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
