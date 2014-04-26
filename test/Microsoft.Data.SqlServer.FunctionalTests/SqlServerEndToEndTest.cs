// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Update;
using Microsoft.Data.SqlServer.Utilities;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class SqlServerEndToEndTest
    {
        [Fact]
        public async Task Can_run_linq_query_on_entity_set()
        {
            using (await TestDatabase.Northwind())
            {
                using (var db = new NorthwindContext())
                {
                    var results = db.Customers
                        .Where(c => c.CompanyName.StartsWith("A"))
                        .OrderByDescending(c => c.CustomerID)
                        .ToList();

                    Assert.Equal(4, results.Count);
                    Assert.Equal("AROUT", results[0].CustomerID);
                    Assert.Equal("ANTON", results[1].CustomerID);
                    Assert.Equal("ANATR", results[2].CustomerID);
                    Assert.Equal("ALFKI", results[3].CustomerID);

                    Assert.Equal("(171) 555-6750", results[0].Fax);
                    Assert.Null(results[1].Fax);
                    Assert.Equal("(5) 555-3745", results[2].Fax);
                    Assert.Equal("030-0076545", results[3].Fax);
                }
            }
        }

        [Fact]
        public async Task Can_run_linq_query_on_entity_set_with_value_buffer_reader()
        {
            using (await TestDatabase.Northwind())
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s =>
                        {
                            s.AddSqlServer();
                            s.ServiceCollection.AddScoped<SqlServerDataStore, SqlStoreWithBufferReader>();
                        })
                    .BuildServiceProvider();

                using (var db = new NorthwindContext(serviceProvider))
                {
                    var results = db.Customers
                        .Where(c => c.CompanyName.StartsWith("A"))
                        .OrderByDescending(c => c.CustomerID)
                        .ToList();

                    Assert.Equal(4, results.Count);
                    Assert.Equal("AROUT", results[0].CustomerID);
                    Assert.Equal("ANTON", results[1].CustomerID);
                    Assert.Equal("ANATR", results[2].CustomerID);
                    Assert.Equal("ALFKI", results[3].CustomerID);

                    Assert.Equal("(171) 555-6750", results[0].Fax);
                    Assert.Null(results[1].Fax);
                    Assert.Equal("(5) 555-3745", results[2].Fax);
                    Assert.Equal("030-0076545", results[3].Fax);

                    Assert.IsType<SqlStoreWithBufferReader>(db.Configuration.DataStore);
                }
            }
        }

        private class SqlStoreWithBufferReader : SqlServerDataStore
        {
            public SqlStoreWithBufferReader(
                ContextConfiguration configuration,
                SqlServerConnection connection,
                DatabaseBuilder databaseBuilder,
                CommandBatchPreparer batchPreparer,
                SqlServerBatchExecutor batchExecutor)
                : base(configuration, connection, databaseBuilder, batchPreparer, batchExecutor)
            {
            }

            protected override RelationalValueReaderFactory ValueReaderFactory
            {
                get { return new RelationalObjectArrayValueReaderFactory(); }
            }
        }

        [Fact]
        public async Task Can_enumerate_entity_set()
        {
            using (await TestDatabase.Northwind())
            {
                using (var db = new NorthwindContext())
                {
                    var results = new List<Customer>();
                    foreach (var item in db.Customers)
                    {
                        results.Add(item);
                    }

                    Assert.Equal(91, results.Count);
                    Assert.Equal("ALFKI", results[0].CustomerID);
                    Assert.Equal("Alfreds Futterkiste", results[0].CompanyName);
                }
            }
        }

        [Fact]
        public async Task Can_save_changes()
        {
            using (var testDatabase = await TestDatabase.Scratch())
            {
                await CreateBlogDatabase(testDatabase);

                var configuration = new EntityConfigurationBuilder()
                    .SqlServerConnectionString(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();

                using (var db = new BloggingContext(configuration))
                {
                    var toUpdate = new Blog { Id = 1, Name = "Blog is Updated" };
                    var toDelete = new Blog { Id = 2, Name = "Blog to Delete" };

                    db.ChangeTracker.Entry(toUpdate).State = EntityState.Modified;
                    db.ChangeTracker.Entry(toDelete).State = EntityState.Deleted;
                    var toAdd = db.Blogs.Add(new Blog { Name = "Blog to Insert" });

                    await db.SaveChangesAsync();

                    Assert.NotEqual(0, toAdd.Id);

                    Assert.Equal(EntityState.Unchanged, db.ChangeTracker.Entry(toUpdate).State);
                    Assert.Equal(EntityState.Unchanged, db.ChangeTracker.Entry(toAdd).State);
                    Assert.DoesNotContain(toDelete, db.ChangeTracker.Entries().Select(e => e.Entity));

                    var rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 1 AND Name = 'Blog is Updated'",
                        CancellationToken.None);

                    Assert.Equal(1, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 2",
                        CancellationToken.None);

                    Assert.Equal(0, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 3 AND Name = 'Blog to Insert'",
                        CancellationToken.None);

                    Assert.Equal(1, rows);
                }
            }
        }

        [Fact]
        public async Task Can_round_trip_changes_with_snapshot_change_tracking()
        {
            await RoundTripChanges<Blog>();
        }

        [Fact]
        public async Task Can_round_trip_changes_with_full_notification_entities()
        {
            await RoundTripChanges<ChangedChangingBlog>();
        }

        [Fact]
        public async Task Can_round_trip_changes_with_changed_only_notification_entities()
        {
            await RoundTripChanges<ChangedOnlyBlog>();
        }

        private async Task RoundTripChanges<TBlog>() where TBlog : class, IBlog
        {
            using (var testDatabase = await TestDatabase.Scratch())
            {
                await CreateBlogDatabase(testDatabase);

                var configuration = new EntityConfigurationBuilder()
                    .SqlServerConnectionString(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();

                using (var context = new BloggingContext<TBlog>(configuration))
                {
                    var blogs = context.Blogs.ToList();
                    Assert.Equal(2, blogs.Count);

                    blogs.Single(b => b.Id == 1).Name = "New Name";

                    await context.SaveChangesAsync();
                }

                using (var context = new BloggingContext<TBlog>(configuration))
                {
                    var blogs = context.Blogs.ToList();
                    Assert.Equal(2, blogs.Count);

                    Assert.Equal("New Name", blogs.Single(b => b.Id == 1).Name);
                    Assert.Equal("Blog2", blogs.Single(b => b.Id == 2).Name);
                }
            }
        }

        private static async Task CreateBlogDatabase(TestDatabase testDatabase)
        {
            await testDatabase.ExecuteNonQueryAsync(
                @"CREATE TABLE [dbo].[Blog](
                      [Id] [int] NOT NULL IDENTITY,
                      [Name] [nvarchar](max) NULL,
                      CONSTRAINT [PK_Blogging] PRIMARY KEY CLUSTERED ( [Id] ASC ))");

            await testDatabase.ExecuteNonQueryAsync(@"INSERT INTO [dbo].[Blog] (Name) VALUES ('Blog1')");
            await testDatabase.ExecuteNonQueryAsync(@"INSERT INTO [dbo].[Blog] (Name) VALUES ('Blog2')");
        }

        private class NorthwindContext : DbContext
        {
            public NorthwindContext()
            {
            }

            public NorthwindContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnConfiguring(EntityConfigurationBuilder builder)
            {
                builder.SqlServerConnectionString(TestDatabase.NorthwindConnectionString);
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder
                    .Entity<Customer>()
                    .Key(c => c.CustomerID)
                    .StorageName("Customers");
            }
        }

        private class Customer
        {
            public string CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string Fax { get; set; }
        }

        private class BloggingContext : BloggingContext<Blog>
        {
            public BloggingContext(EntityConfiguration configuration)
                : base(configuration)
            {
            }
        }

        private class Blog : IBlog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class BloggingContext<TBlog> : DbContext
            where TBlog : class, IBlog
        {
            public BloggingContext(EntityConfiguration configuration)
                : base(configuration)
            {
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Model
                    .GetEntityType(typeof(TBlog))
                    .GetProperty("Id")
                    .ValueGenerationStrategy = ValueGenerationStrategy.StoreIdentity;

                builder.Entity<TBlog>().StorageName("Blog");
            }

            public DbSet<TBlog> Blogs { get; set; }
        }

        private interface IBlog
        {
            int Id { get; set; }
            string Name { get; set; }
        }

        private class ChangedChangingBlog : INotifyPropertyChanging, INotifyPropertyChanged, IBlog
        {
            private int _id;
            private string _name;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        NotifyChanging();
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        NotifyChanging();
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private void NotifyChanging([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanging != null)
                {
                    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
                }
            }
        }

        private class ChangedOnlyBlog : INotifyPropertyChanged, IBlog
        {
            private int _id;
            private string _name;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
