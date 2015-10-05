// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerEndToEndTest : IClassFixture<SqlServerFixture>
    {
        [Fact]
        public void Can_run_linq_query_on_entity_set()
        {
            using (SqlServerNorthwindContext.GetSharedStore())
            {
                using (var db = new NorthwindContext(_fixture.ServiceProvider))
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
        public void Can_run_linq_query_on_entity_set_with_value_buffer_reader()
        {
            using (SqlServerNorthwindContext.GetSharedStore())
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection
                    .AddEntityFramework()
                    .AddSqlServer();

                serviceCollection.AddSingleton<IRelationalValueBufferFactoryFactory, TestTypedValueBufferFactoryFactory>();
                var serviceProvider = serviceCollection.BuildServiceProvider();

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
                }
            }
        }

        public class TestTypedValueBufferFactoryFactory : TypedRelationalValueBufferFactoryFactory
        {
        }

        [Fact]
        public void Can_enumerate_entity_set()
        {
            using (SqlServerNorthwindContext.GetSharedStore())
            {
                using (var db = new NorthwindContext(_fixture.ServiceProvider))
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
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync())
            {
                var optionsBuilder = new DbContextOptionsBuilder();

                optionsBuilder
                    .UseSqlServer(testDatabase.Connection.ConnectionString)
                    .LogSqlParameterValues();

                using (var db = new BloggingContext(_fixture.ServiceProvider, optionsBuilder.Options))
                {
                    await CreateBlogDatabaseAsync<Blog>(db);
                }

                var loggingFactory = new TestSqlLoggerFactory();
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection()
                    .AddInstance<ILoggerFactory>(loggingFactory)
                    .BuildServiceProvider();
                using (var db = new BloggingContext(serviceProvider, optionsBuilder.Options))
                {
                    var toUpdate = db.Blogs.Single(b => b.Name == "Blog1");
                    toUpdate.Name = "Blog is Updated";
                    var updatedId = toUpdate.Id;
                    var toDelete = db.Blogs.Single(b => b.Name == "Blog2");
                    toDelete.Name = "Blog to delete";
                    var deletedId = toDelete.Id;

                    db.Entry(toUpdate).State = EntityState.Modified;
                    db.Entry(toDelete).State = EntityState.Deleted;

                    var toAdd = db.Add(new Blog
                        {
                            Name = "Blog to Insert",
                            George = true,
                            TheGu = new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9BF"),
                            NotFigTime = new DateTime(1973, 9, 3, 0, 10, 33, 777),
                            ToEat = 64,
                            OrNothing = 0.123456789,
                            Fuse = 777,
                            WayRound = 9876543210,
                            Away = 0.12345f,
                            AndChew = new byte[16]
                        }).Entity;

                    await db.SaveChangesAsync();

                    var addedId = toAdd.Id;
                    Assert.NotEqual(0, addedId);

                    Assert.Equal(EntityState.Unchanged, db.Entry(toUpdate).State);
                    Assert.Equal(EntityState.Unchanged, db.Entry(toAdd).State);
                    Assert.DoesNotContain(toDelete, db.ChangeTracker.Entries().Select(e => e.Entity));

                    Assert.Equal(3, TestSqlLoggerFactory.SqlStatements.Count);
                    Assert.Contains("SELECT", TestSqlLoggerFactory.SqlStatements[0]);
                    Assert.Contains("SELECT", TestSqlLoggerFactory.SqlStatements[1]);
                    Assert.Contains("@p0: " + deletedId, TestSqlLoggerFactory.SqlStatements[2]);
                    Assert.Contains("DELETE", TestSqlLoggerFactory.SqlStatements[2]);
                    Assert.Contains("UPDATE", TestSqlLoggerFactory.SqlStatements[2]);
                    Assert.Contains("INSERT", TestSqlLoggerFactory.SqlStatements[2]);

                    var rows = await testDatabase.ExecuteScalarAsync<int>(
                        $@"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = {updatedId} AND Name = 'Blog is Updated'",
                        CancellationToken.None);

                    Assert.Equal(1, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        $@"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = {deletedId}",
                        CancellationToken.None);

                    Assert.Equal(0, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        $@"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = {addedId} AND Name = 'Blog to Insert'",
                        CancellationToken.None);

                    Assert.Equal(1, rows);
                }

            }
        }

        [Fact]
        public async Task Can_save_changes_in_tracked_entities()
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync())
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(testDatabase.Connection.ConnectionString);

                int updatedId;
                int deletedId;
                int addedId;
                using (var db = new BloggingContext(_fixture.ServiceProvider, optionsBuilder.Options))
                {
                    var blogs = await CreateBlogDatabaseAsync<Blog>(db);

                    var toAdd = db.Blogs.Add(new Blog
                    {
                        Name = "Blog to Insert",
                        George = true,
                        TheGu = new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9BF"),
                        NotFigTime = new DateTime(1973, 9, 3, 0, 10, 33, 777),
                        ToEat = 64,
                        OrNothing = 0.123456789,
                        Fuse = 777,
                        WayRound = 9876543210,
                        Away = 0.12345f,
                        AndChew = new byte[16]
                    }).Entity;
                    db.Entry(toAdd).State = EntityState.Detached;

                    var toUpdate = blogs[0];
                    toUpdate.Name = "Blog is Updated";
                    updatedId = toUpdate.Id;
                    var toDelete = blogs[1];
                    toDelete.Name = "Blog to delete";
                    deletedId = toDelete.Id;

                    db.Remove(toDelete);
                    db.Entry(toAdd).State = EntityState.Added;

                    await db.SaveChangesAsync();

                    addedId = toAdd.Id;
                    Assert.NotEqual(0, addedId);

                    Assert.Equal(EntityState.Unchanged, db.Entry(toUpdate).State);
                    Assert.Equal(EntityState.Unchanged, db.Entry(toAdd).State);
                    Assert.DoesNotContain(toDelete, db.ChangeTracker.Entries().Select(e => e.Entity));
                }

                using (var db = new BloggingContext(_fixture.ServiceProvider, optionsBuilder.Options))
                {
                    var toUpdate = db.Blogs.Single(b => b.Id == updatedId);
                    Assert.Equal("Blog is Updated", toUpdate.Name);
                    Assert.Equal(0, db.Blogs.Count(b => b.Id == deletedId));
                    Assert.Equal("Blog to Insert", db.Blogs.Single(b => b.Id == addedId).Name);
                }
            }
        }

        [Fact]
        public async Task Tracking_entities_asynchronously_returns_tracked_entities_back()
        {
            using (SqlServerNorthwindContext.GetSharedStore())
            {
                using (var db = new NorthwindContext(_fixture.ServiceProvider))
                {
                    var customer = await db.Customers.FirstOrDefaultAsync();

                    var trackedCustomerEntry = db.ChangeTracker.Entries().Single();
                    Assert.Same(trackedCustomerEntry.Entity, customer);

                    // if references are different this will throw
                    db.Customers.Remove(customer);
                }
            }
        }

        [Fact] // Issue #931
        public async Task Can_save_and_query_with_schema()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .AddDbContext<SchemaContext>()
                    .ServiceCollection()
                    .BuildServiceProvider();

            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync())
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE SCHEMA Apple");
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE Apple.Jack (MyKey int)");
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE Apple.Black (MyKey int)");

                using (var context = serviceProvider
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope()
                    .ServiceProvider
                    .GetRequiredService<SchemaContext>())
                {
                    context.Connection = testDatabase.Connection;

                    context.Add(new Jack { MyKey = 1 });
                    context.Add(new Black { MyKey = 2 });
                    context.SaveChanges();
                }

                using (var context = serviceProvider
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope()
                    .ServiceProvider
                    .GetRequiredService<SchemaContext>())
                {
                    context.Connection = testDatabase.Connection;

                    Assert.Equal(1, context.Jacks.Count());
                    Assert.Equal(1, context.Blacks.Count());
                }
            }
        }

        private class SchemaContext : DbContext
        {
            public SchemaContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbConnection Connection { get; set; }

            public DbSet<Jack> Jacks { get; set; }
            public DbSet<Black> Blacks { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(Connection);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Jack>()
                    .ToTable("Jack", "Apple")
                    .HasKey(e => e.MyKey);

                modelBuilder
                    .Entity<Black>()
                    .ToSqlServerTable("Black", "Apple")
                    .HasKey(e => e.MyKey);
            }
        }

        private class Jack
        {
            public int MyKey { get; set; }
        }

        private class Black
        {
            public int MyKey { get; set; }
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

        private async Task RoundTripChanges<TBlog>() where TBlog : class, IBlog, new()
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync())
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(testDatabase.Connection.ConnectionString);

                int blog1Id;
                int blog2Id;
                int blog3Id;

                using (var context = new BloggingContext<TBlog>(_fixture.ServiceProvider, optionsBuilder.Options))
                {
                    var blogs = await CreateBlogDatabaseAsync<TBlog>(context);
                    blog1Id = blogs[0].Id;
                    blog2Id = blogs[1].Id;

                    Assert.NotEqual(0, blog1Id);
                    Assert.NotEqual(0, blog2Id);
                    Assert.NotEqual(blog1Id, blog2Id);
                }

                using (var context = new BloggingContext<TBlog>(_fixture.ServiceProvider, optionsBuilder.Options))
                {
                    var blogs = context.Blogs.ToList();
                    Assert.Equal(2, blogs.Count);

                    var blog1 = blogs.Single(b => b.Name == "Blog1");
                    Assert.Equal(blog1Id, blog1.Id);

                    Assert.Equal("Blog1", blog1.Name);
                    Assert.True(blog1.George);
                    Assert.Equal(new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9BF"), blog1.TheGu);
                    Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 33, 777), blog1.NotFigTime);
                    Assert.Equal(64, blog1.ToEat);
                    Assert.Equal(0.123456789, blog1.OrNothing);
                    Assert.Equal(777, blog1.Fuse);
                    Assert.Equal(9876543210, blog1.WayRound);
                    Assert.Equal(0.12345f, blog1.Away);
                    Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, blog1.AndChew);

                    blog1.Name = "New Name";

                    var blog2 = blogs.Single(b => b.Name == "Blog2");
                    Assert.Equal(blog2Id, blog2.Id);

                    blog2.Name = null;
                    blog2.NotFigTime = new DateTime();
                    blog2.AndChew = null;

                    var blog3 = context.Add(new TBlog()).Entity;

                    await context.SaveChangesAsync();

                    blog3Id = blog3.Id;
                    Assert.NotEqual(0, blog3Id);
                }

                using (var context = new BloggingContext<TBlog>(_fixture.ServiceProvider, optionsBuilder.Options))
                {
                    var blogs = context.Blogs.ToList();
                    Assert.Equal(3, blogs.Count);

                    Assert.Equal("New Name", blogs.Single(b => b.Id == blog1Id).Name);

                    var blog2 = blogs.Single(b => b.Id == blog2Id);
                    Assert.Null(blog2.Name);
                    Assert.Equal(blog2.NotFigTime, new DateTime());
                    Assert.Null(blog2.AndChew);

                    var blog3 = blogs.Single(b => b.Id == blog3Id);
                    Assert.Null(blog3.Name);
                    Assert.Equal(blog3.NotFigTime, new DateTime());
                    Assert.Null(blog3.AndChew);
                }
            }
        }

        private static async Task<TBlog[]> CreateBlogDatabaseAsync<TBlog>(DbContext context) where TBlog : class, IBlog, new()
        {
            await context.Database.EnsureCreatedAsync();
            var blog1 = context.Add(new TBlog
            {
                Name = "Blog1",
                George = true,
                TheGu = new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9BF"),
                NotFigTime = new DateTime(1973, 9, 3, 0, 10, 33, 777),
                ToEat = 64,
                //CupOfChar = 'C', // TODO: Conversion failed when converting the nvarchar value 'C' to data type int.
                OrNothing = 0.123456789,
                Fuse = 777,
                WayRound = 9876543210,
                //NotToEat = -64, // TODO: The parameter data type of SByte is invalid.
                Away = 0.12345f,
                //OrULong = 888, // TODO: The parameter data type of UInt16 is invalid.
                //OrUSkint = 8888888, // TODO: The parameter data type of UInt32 is invalid.
                //OrUShort = 888888888888888, // TODO: The parameter data type of UInt64 is invalid.
                AndChew = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
            }).Entity;
            var blog2 = context.Add(new TBlog
            {
                Name = "Blog2",
                George = false,
                TheGu = new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9CF"),
                NotFigTime = new DateTime(1973, 9, 3, 0, 10, 33, 778),
                ToEat = 65,
                //CupOfChar = 'D', // TODO: Conversion failed when converting the nvarchar value 'C' to data type int.
                OrNothing = 0.987654321,
                Fuse = 778,
                WayRound = 98765432100,
                //NotToEat = -64, // TODO: The parameter data type of SByte is invalid.
                Away = 0.12345f,
                //OrULong = 888, // TODO: The parameter data type of UInt16 is invalid.
                //OrUSkint = 8888888, // TODO: The parameter data type of UInt32 is invalid.
                //OrUShort = 888888888888888, // TODO: The parameter data type of UInt64 is invalid.
                AndChew = new byte[16]
            }).Entity;
            await context.SaveChangesAsync();

            return new[] { blog1, blog2 };
        }

        private readonly SqlServerFixture _fixture;

        public SqlServerEndToEndTest(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        private class NorthwindContext : DbContext
        {
            public NorthwindContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(SqlServerNorthwindContext.ConnectionString);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.HasKey(c => c.CustomerID);
                        b.ToSqlServerTable("Customers");
                    });
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
            public BloggingContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }
        }

        private class Blog : IBlog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool George { get; set; }
            public Guid TheGu { get; set; }
            public DateTime NotFigTime { get; set; }
            public byte ToEat { get; set; }
            //public char CupOfChar { get; set; }
            public double OrNothing { get; set; }
            public short Fuse { get; set; }
            public long WayRound { get; set; }
            //public sbyte NotToEat { get; set; }
            public float Away { get; set; }
            //public ushort OrULong { get; set; }
            //public uint OrUSkint { get; set; }
            //public ulong OrUShort { get; set; }
            public byte[] AndChew { get; set; }
        }

        private class BloggingContext<TBlog> : DbContext
            where TBlog : class, IBlog
        {
            public BloggingContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TBlog>().ToTable("Blog", "dbo");
            }

            public DbSet<TBlog> Blogs { get; set; }
        }

        private interface IBlog
        {
            int Id { get; set; }
            string Name { get; set; }
            bool George { get; set; }
            Guid TheGu { get; set; }
            DateTime NotFigTime { get; set; }
            byte ToEat { get; set; }
            //char CupOfChar { get; set; }
            double OrNothing { get; set; }
            short Fuse { get; set; }
            long WayRound { get; set; }
            //sbyte NotToEat { get; set; }
            float Away { get; set; }
            //ushort OrULong { get; set; }
            //uint OrUSkint { get; set; }
            //ulong OrUShort { get; set; }
            byte[] AndChew { get; set; }
        }

        private class ChangedChangingBlog : INotifyPropertyChanging, INotifyPropertyChanged, IBlog
        {
            private int _id;
            private string _name;
            private bool _george;
            private Guid _theGu;
            private DateTime _notFigTime;
            private byte _toEat;
            //private char _cupOfChar;
            private double _orNothing;
            private short _fuse;
            private long _wayRound;
            //private sbyte _notToEat;
            private float _away;
            //private ushort _orULong;
            //private uint _orUSkint;
            //private ulong _orUShort;
            private byte[] _andChew;

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

            public bool George
            {
                get { return _george; }
                set
                {
                    if (_george != value)
                    {
                        NotifyChanging();
                        _george = value;
                        NotifyChanged();
                    }
                }
            }

            public Guid TheGu
            {
                get { return _theGu; }
                set
                {
                    if (_theGu != value)
                    {
                        NotifyChanging();
                        _theGu = value;
                        NotifyChanged();
                    }
                }
            }

            public DateTime NotFigTime
            {
                get { return _notFigTime; }
                set
                {
                    if (_notFigTime != value)
                    {
                        NotifyChanging();
                        _notFigTime = value;
                        NotifyChanged();
                    }
                }
            }

            public byte ToEat
            {
                get { return _toEat; }
                set
                {
                    if (_toEat != value)
                    {
                        NotifyChanging();
                        _toEat = value;
                        NotifyChanged();
                    }
                }
            }

            //public char CupOfChar
            //{
            //    get { return _cupOfChar; }
            //    set
            //    {
            //        if (_cupOfChar != value)
            //        {
            //            NotifyChanging();
            //            _cupOfChar = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            public double OrNothing
            {
                get { return _orNothing; }
                set
                {
                    if (_orNothing != value)
                    {
                        NotifyChanging();
                        _orNothing = value;
                        NotifyChanged();
                    }
                }
            }

            public short Fuse
            {
                get { return _fuse; }
                set
                {
                    if (_fuse != value)
                    {
                        NotifyChanging();
                        _fuse = value;
                        NotifyChanged();
                    }
                }
            }

            public long WayRound
            {
                get { return _wayRound; }
                set
                {
                    if (_wayRound != value)
                    {
                        NotifyChanging();
                        _wayRound = value;
                        NotifyChanged();
                    }
                }
            }

            //public sbyte NotToEat
            //{
            //    get { return _notToEat; }
            //    set
            //    {
            //        if (_notToEat != value)
            //        {
            //            NotifyChanging();
            //            _notToEat = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            public float Away
            {
                get { return _away; }
                set
                {
                    if (_away != value)
                    {
                        NotifyChanging();
                        _away = value;
                        NotifyChanged();
                    }
                }
            }

            //public ushort OrULong
            //{
            //    get { return _orULong; }
            //    set
            //    {
            //        if (_orULong != value)
            //        {
            //            NotifyChanging();
            //            _orULong = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            //public uint OrUSkint
            //{
            //    get { return _orUSkint; }
            //    set
            //    {
            //        if (_orUSkint != value)
            //        {
            //            NotifyChanging();
            //            _orUSkint = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            //public ulong OrUShort
            //{
            //    get { return _orUShort; }
            //    set
            //    {
            //        if (_orUShort != value)
            //        {
            //            NotifyChanging();
            //            _orUShort = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            public byte[] AndChew
            {
                get { return _andChew; }
                set
                {
                    if (_andChew != value) // Not a great way to compare byte arrays
                    {
                        NotifyChanging();
                        _andChew = value;
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
            private bool _george;
            private Guid _theGu;
            private DateTime _notFigTime;
            private byte _toEat;
            //private char _cupOfChar;
            private double _orNothing;
            private short _fuse;
            private long _wayRound;
            //private sbyte _notToEat;
            private float _away;
            //private ushort _orULong;
            //private uint _orUSkint;
            //private ulong _orUShort;
            private byte[] _andChew;

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

            public bool George
            {
                get { return _george; }
                set
                {
                    if (_george != value)
                    {
                        _george = value;
                        NotifyChanged();
                    }
                }
            }

            public Guid TheGu
            {
                get { return _theGu; }
                set
                {
                    if (_theGu != value)
                    {
                        _theGu = value;
                        NotifyChanged();
                    }
                }
            }

            public DateTime NotFigTime
            {
                get { return _notFigTime; }
                set
                {
                    if (_notFigTime != value)
                    {
                        _notFigTime = value;
                        NotifyChanged();
                    }
                }
            }

            public byte ToEat
            {
                get { return _toEat; }
                set
                {
                    if (_toEat != value)
                    {
                        _toEat = value;
                        NotifyChanged();
                    }
                }
            }

            //public char CupOfChar
            //{
            //    get { return _cupOfChar; }
            //    set
            //    {
            //        if (_cupOfChar != value)
            //        {
            //            _cupOfChar = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            public double OrNothing
            {
                get { return _orNothing; }
                set
                {
                    if (_orNothing != value)
                    {
                        _orNothing = value;
                        NotifyChanged();
                    }
                }
            }

            public short Fuse
            {
                get { return _fuse; }
                set
                {
                    if (_fuse != value)
                    {
                        _fuse = value;
                        NotifyChanged();
                    }
                }
            }

            public long WayRound
            {
                get { return _wayRound; }
                set
                {
                    if (_wayRound != value)
                    {
                        _wayRound = value;
                        NotifyChanged();
                    }
                }
            }

            //public sbyte NotToEat
            //{
            //    get { return _notToEat; }
            //    set
            //    {
            //        if (_notToEat != value)
            //        {
            //            _notToEat = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            public float Away
            {
                get { return _away; }
                set
                {
                    if (_away != value)
                    {
                        _away = value;
                        NotifyChanged();
                    }
                }
            }

            //public ushort OrULong
            //{
            //    get { return _orULong; }
            //    set
            //    {
            //        if (_orULong != value)
            //        {
            //            _orULong = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            //public uint OrUSkint
            //{
            //    get { return _orUSkint; }
            //    set
            //    {
            //        if (_orUSkint != value)
            //        {
            //            _orUSkint = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            //public ulong OrUShort
            //{
            //    get { return _orUShort; }
            //    set
            //    {
            //        if (_orUShort != value)
            //        {
            //            _orUShort = value;
            //            NotifyChanged();
            //        }
            //    }
            //}

            public byte[] AndChew
            {
                get { return _andChew; }
                set
                {
                    if (_andChew != value) // Not a great way to compare byte arrays
                    {
                        _andChew = value;
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
