// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Update;
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
                var configuration = new EntityConfigurationBuilder()
                    .SqlServerConnectionString(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();

                using (var db = new BloggingContext(configuration))
                {
                    await CreateBlogDatabase<Blog>(db);
                }

                using (var db = new BloggingContext(configuration))
                {
                    var toUpdate = db.Blogs.Single(b => b.Id == 77);
                    toUpdate.Name = "Blog is Updated";
                    var toDelete = db.Blogs.Single(b => b.Id == 78);
                    toDelete.Name = "Blog to delete";

                    db.ChangeTracker.Entry(toUpdate).State = EntityState.Modified;
                    db.ChangeTracker.Entry(toDelete).State = EntityState.Deleted;

                    var toAdd = db.Blogs.Add(new Blog()
                        {
                            Id = 79,
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
                        });

                    await db.SaveChangesAsync();

                    Assert.NotEqual(0, toAdd.Id);

                    Assert.Equal(EntityState.Unchanged, db.ChangeTracker.Entry(toUpdate).State);
                    Assert.Equal(EntityState.Unchanged, db.ChangeTracker.Entry(toAdd).State);
                    Assert.DoesNotContain(toDelete, db.ChangeTracker.Entries().Select(e => e.Entity));

                    var rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 77 AND Name = 'Blog is Updated'",
                        CancellationToken.None);

                    Assert.Equal(1, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 78",
                        CancellationToken.None);

                    Assert.Equal(0, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 79 AND Name = 'Blog to Insert'",
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

        private async Task RoundTripChanges<TBlog>() where TBlog : class, IBlog, new()
        {
            using (var testDatabase = await TestDatabase.Scratch())
            {
                var configuration = new EntityConfigurationBuilder()
                    .SqlServerConnectionString(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();

                using (var context = new BloggingContext<TBlog>(configuration))
                {
                    await CreateBlogDatabase<TBlog>(context);
                }

                using (var context = new BloggingContext<TBlog>(configuration))
                {
                    var blogs = context.Blogs.ToList();
                    Assert.Equal(2, blogs.Count);

                    var blog = blogs.Single(b => b.Id == 77);

                    Assert.Equal(77, blog.Id);
                    Assert.Equal("Blog1", blog.Name);
                    Assert.True(blog.George);
                    Assert.Equal(new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9BF"), blog.TheGu);
                    Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 33, 777), blog.NotFigTime);
                    Assert.Equal(64, blog.ToEat);
                    Assert.Equal(0.123456789, blog.OrNothing);
                    Assert.Equal(777, blog.Fuse);
                    Assert.Equal(9876543210, blog.WayRound);
                    Assert.Equal(0.12345f, blog.Away);
                    Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, blog.AndChew);

                    blog.Name = "New Name";

                    await context.SaveChangesAsync();
                }

                using (var context = new BloggingContext<TBlog>(configuration))
                {
                    var blogs = context.Blogs.ToList();
                    Assert.Equal(2, blogs.Count);

                    Assert.Equal("New Name", blogs.Single(b => b.Id == 77).Name);
                    Assert.Equal("Blog2", blogs.Single(b => b.Id == 78).Name);
                }
            }
        }

        private static async Task CreateBlogDatabase<TBlog>(DbContext context) where TBlog : class, IBlog, new()
        {
            await context.Database.CreateAsync();
            await context.AddAsync(new TBlog()
                {
                    Id = 77,
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
                });
            await context.AddAsync(new TBlog()
                {
                    Id = 78,
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
                });
            await context.SaveChangesAsync();
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
            public BloggingContext(EntityConfiguration configuration)
                : base(configuration)
            {
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<TBlog>().StorageName("Blog");
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
