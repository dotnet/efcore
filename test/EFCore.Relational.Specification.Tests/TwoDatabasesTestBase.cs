// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TwoDatabasesTestBase
    {
        protected FixtureBase Fixture { get; }

        protected TwoDatabasesTestBase(FixtureBase fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public virtual void Can_query_from_one_connection_string_and_save_changes_to_another()
        {
            using var context1 = CreateBackingContext("TwoDatabasesOne");
            using var context2 = CreateBackingContext("TwoDatabasesTwo");

            var connectionString1 = context1.Database.GetConnectionString();
            var connectionString2 = context2.Database.GetConnectionString();

            Assert.NotEqual(context1.Database.GetConnectionString(), context2.Database.GetConnectionString());

            context1.Database.EnsureCreatedResiliently();
            context2.Database.EnsureCreatedResiliently();

            using (var context = new TwoDatabasesContext(CreateTestOptions(new DbContextOptionsBuilder()).Options))
            {
                context.Database.SetConnectionString(connectionString1);

                var data = context.Foos.ToList();
                data[0].Bar = "Modified One";
                data[1].Bar = "Modified Two";

                context.Database.SetConnectionString(connectionString2);

                context.SaveChanges();
            }

            Assert.Equal(new[] { "One", "Two" }, context1.Foos.Select(e => e.Bar).ToList());
            Assert.Equal(new[] { "Modified One", "Modified Two" }, context2.Foos.Select(e => e.Bar).ToList());
        }

        [ConditionalFact]
        public virtual void Can_query_from_one_connection_and_save_changes_to_another()
        {
            using var context1 = CreateBackingContext("TwoDatabasesOneB");
            using var context2 = CreateBackingContext("TwoDatabasesTwoB");

            Assert.NotSame(context1.Database.GetDbConnection(), context2.Database.GetDbConnection());

            context1.Database.EnsureCreatedResiliently();
            context2.Database.EnsureCreatedResiliently();

            using (var context = new TwoDatabasesContext(CreateTestOptions(new DbContextOptionsBuilder()).Options))
            {
                context.Database.SetDbConnection(context1.Database.GetDbConnection());

                var data = context.Foos.ToList();
                data[0].Bar = "Modified One";
                data[1].Bar = "Modified Two";

                context.Database.SetDbConnection(context2.Database.GetDbConnection());

                context.SaveChanges();
            }

            Assert.Equal(new[] { "One", "Two" }, context1.Foos.Select(e => e.Bar).ToList());
            Assert.Equal(new[] { "Modified One", "Modified Two" }, context2.Foos.Select(e => e.Bar).ToList());
        }

        [ConditionalFact]
        public virtual void Can_set_connection_string_in_interceptor()
        {
            using var context1 = CreateBackingContext("TwoDatabasesIntercept");

            var connectionString1 = context1.Database.GetConnectionString();

            context1.Database.EnsureCreatedResiliently();

            using (var context = new TwoDatabasesContext(
                CreateTestOptions(new DbContextOptionsBuilder(), withConnectionString: true)
                    .AddInterceptors(
                        new ConnectionStringConnectionInterceptor(
                            connectionString1, DummyConnectionString))
                    .Options))
            {
                var data = context.Foos.ToList();
                data[0].Bar = "Modified One";
                data[1].Bar = "Modified Two";

                context.SaveChanges();
            }

            Assert.Equal(new[] { "Modified One", "Modified Two" }, context1.Foos.Select(e => e.Bar).ToList());
        }

        protected class ConnectionStringConnectionInterceptor : DbConnectionInterceptor
        {
            private readonly string _goodConnectionString;
            private readonly string _dummyConnectionString;

            public ConnectionStringConnectionInterceptor(string goodConnectionString, string dummyConnectionString)
            {
                _goodConnectionString = goodConnectionString;
                _dummyConnectionString = dummyConnectionString;
            }

            public override InterceptionResult ConnectionOpening(
                DbConnection connection,
                ConnectionEventData eventData,
                InterceptionResult result)
            {
                Assert.Equal(_dummyConnectionString, eventData.Context.Database.GetConnectionString());
                eventData.Context.Database.SetConnectionString(_goodConnectionString);

                return result;
            }

            public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
            {
                Assert.Equal(_goodConnectionString, eventData.Context.Database.GetConnectionString());
                eventData.Context.Database.SetConnectionString(_dummyConnectionString);
            }
        }

        protected abstract DbContextOptionsBuilder CreateTestOptions(
            DbContextOptionsBuilder optionsBuilder,
            bool withConnectionString = false);

        protected abstract TwoDatabasesWithDataContext CreateBackingContext(string databaseName);

        protected abstract string DummyConnectionString { get; }

        protected class TwoDatabasesContext : DbContext
        {
            public TwoDatabasesContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Foo>();
            }

            public IQueryable<Foo> Foos
                => Set<Foo>().OrderBy(e => e.Id);
        }

        protected class TwoDatabasesWithDataContext : TwoDatabasesContext
        {
            public TwoDatabasesWithDataContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder
                    .Entity<Foo>()
                    .HasData(
                        new Foo { Id = 1, Bar = "One" },
                        new Foo { Id = 2, Bar = "Two" });
            }
        }

        protected class Foo
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Bar { get; set; }
        }
    }
}
