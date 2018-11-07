// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerEndToEndTest : IClassFixture<SqlServerFixture>
    {
        private const string DatabaseName = "SqlServerEndToEndTest";

        protected SqlServerFixture Fixture { get; }

        public SqlServerEndToEndTest(SqlServerFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact]
        public void Can_use_decimal_and_byte_as_identity_columns()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var nownNum1 = new NownNum
                {
                    Id = 77.0m,
                    TheWalrus = "Crying"
                };
                var nownNum2 = new NownNum
                {
                    Id = 78.0m,
                    TheWalrus = "Walrus"
                };

                var numNum1 = new NumNum
                {
                    TheWalrus = "I"
                };
                var numNum2 = new NumNum
                {
                    TheWalrus = "Am"
                };

                var anNum1 = new AnNum
                {
                    TheWalrus = "Goo goo"
                };
                var anNum2 = new AnNum
                {
                    TheWalrus = "g'joob"
                };

                var adNum1 = new AdNum
                {
                    TheWalrus = "Eggman"
                };
                var adNum2 = new AdNum
                {
                    TheWalrus = "Eggmen"
                };

                var byteNownNum1 = new ByteNownNum
                {
                    Id = 77,
                    Lucy = "Tangerine"
                };
                var byteNownNum2 = new ByteNownNum
                {
                    Id = 78,
                    Lucy = "Trees"
                };

                var byteNum1 = new ByteNum
                {
                    Lucy = "Marmalade"
                };
                var byteNum2 = new ByteNum
                {
                    Lucy = "Skies"
                };

                var byteAnNum1 = new ByteAnNum
                {
                    Lucy = "Cellophane"
                };
                var byteAnNum2 = new ByteAnNum
                {
                    Lucy = "Flowers"
                };

                var byteAdNum1 = new ByteAdNum
                {
                    Lucy = "Kaleidoscope"
                };
                var byteAdNum2 = new ByteAdNum
                {
                    Lucy = "Eyes"
                };

                decimal[] preSaveValues;
                byte[] preSaveByteValues;

                var options = Fixture.CreateOptions(testDatabase);
                using (var context = new NumNumContext(options))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        nownNum1, nownNum2, numNum1, numNum2, adNum1, adNum2, anNum1, anNum2,
                        byteNownNum1, byteNownNum2, byteNum1, byteNum2, byteAdNum1, byteAdNum2, byteAnNum1, byteAnNum2);

                    preSaveValues = new[]
                        { numNum1.Id, numNum2.Id, adNum1.Id, adNum2.Id, anNum1.Id, anNum2.Id };

                    preSaveByteValues = new[]
                        { byteNum1.Id, byteNum2.Id, byteAdNum1.Id, byteAdNum2.Id, byteAnNum1.Id, byteAnNum2.Id };

                    context.SaveChanges();
                }

                using (var context = new NumNumContext(options))
                {
                    Assert.Equal(nownNum1.Id, context.NownNums.Single(e => e.TheWalrus == "Crying").Id);
                    Assert.Equal(nownNum2.Id, context.NownNums.Single(e => e.TheWalrus == "Walrus").Id);
                    Assert.Equal(77.0m, nownNum1.Id);
                    Assert.Equal(78.0m, nownNum2.Id);

                    Assert.Equal(numNum1.Id, context.NumNums.Single(e => e.TheWalrus == "I").Id);
                    Assert.Equal(numNum2.Id, context.NumNums.Single(e => e.TheWalrus == "Am").Id);
                    Assert.NotEqual(numNum1.Id, preSaveValues[0]);
                    Assert.NotEqual(numNum2.Id, preSaveValues[1]);

                    Assert.Equal(anNum1.Id, context.AnNums.Single(e => e.TheWalrus == "Goo goo").Id);
                    Assert.Equal(anNum2.Id, context.AnNums.Single(e => e.TheWalrus == "g'joob").Id);
                    Assert.NotEqual(adNum1.Id, preSaveValues[2]);
                    Assert.NotEqual(adNum2.Id, preSaveValues[3]);

                    Assert.Equal(adNum1.Id, context.AdNums.Single(e => e.TheWalrus == "Eggman").Id);
                    Assert.Equal(adNum2.Id, context.AdNums.Single(e => e.TheWalrus == "Eggmen").Id);
                    Assert.NotEqual(anNum1.Id, preSaveValues[4]);
                    Assert.NotEqual(anNum2.Id, preSaveValues[5]);

                    Assert.Equal(byteNownNum1.Id, context.ByteNownNums.Single(e => e.Lucy == "Tangerine").Id);
                    Assert.Equal(byteNownNum2.Id, context.ByteNownNums.Single(e => e.Lucy == "Trees").Id);
                    Assert.Equal(77, byteNownNum1.Id);
                    Assert.Equal(78, byteNownNum2.Id);

                    Assert.Equal(byteNum1.Id, context.ByteNums.Single(e => e.Lucy == "Marmalade").Id);
                    Assert.Equal(byteNum2.Id, context.ByteNums.Single(e => e.Lucy == "Skies").Id);
                    Assert.NotEqual(byteNum1.Id, preSaveByteValues[0]);
                    Assert.NotEqual(byteNum2.Id, preSaveByteValues[1]);

                    Assert.Equal(byteAnNum1.Id, context.ByteAnNums.Single(e => e.Lucy == "Cellophane").Id);
                    Assert.Equal(byteAnNum2.Id, context.ByteAnNums.Single(e => e.Lucy == "Flowers").Id);
                    Assert.NotEqual(byteAdNum1.Id, preSaveByteValues[2]);
                    Assert.NotEqual(byteAdNum2.Id, preSaveByteValues[3]);

                    Assert.Equal(byteAdNum1.Id, context.ByteAdNums.Single(e => e.Lucy == "Kaleidoscope").Id);
                    Assert.Equal(byteAdNum2.Id, context.ByteAdNums.Single(e => e.Lucy == "Eyes").Id);
                    Assert.NotEqual(byteAnNum1.Id, preSaveByteValues[4]);
                    Assert.NotEqual(byteAnNum2.Id, preSaveByteValues[5]);
                }
            }
        }

        private class NumNumContext : DbContext
        {
            public NumNumContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<NownNum> NownNums { get; set; }
            public DbSet<NumNum> NumNums { get; set; }
            public DbSet<AnNum> AnNums { get; set; }
            public DbSet<AdNum> AdNums { get; set; }

            public DbSet<ByteNownNum> ByteNownNums { get; set; }
            public DbSet<ByteNum> ByteNums { get; set; }
            public DbSet<ByteAnNum> ByteAnNums { get; set; }
            public DbSet<ByteAdNum> ByteAdNums { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<NumNum>()
                    .Property(e => e.Id)
                    .HasColumnType("numeric(18, 0)")
                    .UseSqlServerIdentityColumn();

                modelBuilder
                    .Entity<AdNum>()
                    .Property(e => e.Id)
                    .HasColumnType("decimal(10, 0)")
                    .ValueGeneratedOnAdd();

                modelBuilder
                    .Entity<ByteNum>()
                    .Property(e => e.Id)
                    .UseSqlServerIdentityColumn();

                modelBuilder
                    .Entity<ByteAdNum>()
                    .Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                modelBuilder
                    .Entity<NownNum>()
                    .Property(e => e.Id)
                    .HasColumnType("numeric(18, 0)");
            }
        }

        private class NownNum
        {
            public decimal Id { get; set; }
            public string TheWalrus { get; set; }
        }

        private class NumNum
        {
            public decimal Id { get; set; }
            public string TheWalrus { get; set; }
        }

        private class AnNum
        {
            [Column(TypeName = "decimal(18, 0)")]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public decimal Id { get; set; }

            public string TheWalrus { get; set; }
        }

        private class AdNum
        {
            public decimal Id { get; set; }
            public string TheWalrus { get; set; }
        }

        private class ByteNownNum
        {
            public byte Id { get; set; }
            public string Lucy { get; set; }
        }

        private class ByteNum
        {
            public byte Id { get; set; }
            public string Lucy { get; set; }
        }

        private class ByteAnNum
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public byte Id { get; set; }

            public string Lucy { get; set; }
        }

        private class ByteAdNum
        {
            public byte Id { get; set; }
            public string Lucy { get; set; }
        }

        [Fact]
        public void Can_use_string_enum_or_byte_array_as_key()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var sNum1 = new SNum
                {
                    TheWalrus = "I"
                };
                var sNum2 = new SNum
                {
                    TheWalrus = "Am"
                };

                var enNum1 = new EnNum
                {
                    TheWalrus = "Goo goo",
                    Id = ENum.BNum
                };
                var enNum2 = new EnNum
                {
                    TheWalrus = "g'joob",
                    Id = ENum.CNum
                };

                var bNum1 = new BNum
                {
                    TheWalrus = "Eggman"
                };
                var bNum2 = new BNum
                {
                    TheWalrus = "Eggmen"
                };

                var options = Fixture.CreateOptions(testDatabase);
                using (var context = new ENumContext(options))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(sNum1, sNum2, enNum1, enNum2, bNum1, bNum2);

                    context.SaveChanges();
                }

                using (var context = new ENumContext(options))
                {
                    Assert.Equal(sNum1.Id, context.SNums.Single(e => e.TheWalrus == "I").Id);
                    Assert.Equal(sNum2.Id, context.SNums.Single(e => e.TheWalrus == "Am").Id);

                    Assert.Equal(enNum1.Id, context.EnNums.Single(e => e.TheWalrus == "Goo goo").Id);
                    Assert.Equal(enNum2.Id, context.EnNums.Single(e => e.TheWalrus == "g'joob").Id);

                    Assert.Equal(bNum1.Id, context.BNums.Single(e => e.TheWalrus == "Eggman").Id);
                    Assert.Equal(bNum2.Id, context.BNums.Single(e => e.TheWalrus == "Eggmen").Id);
                }
            }
        }

        [Fact]
        public void Can_remove_multiple_byte_array_as_key()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var bNum1 = new BNum
                {
                    TheWalrus = "Eggman"
                };
                var bNum2 = new BNum
                {
                    TheWalrus = "Eggmen"
                };

                var options = Fixture.CreateOptions(testDatabase);
                using (var context = new ENumContext(options))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(bNum1, bNum2);

                    context.SaveChanges();
                }

                using (var context = new ENumContext(options))
                {
                    Assert.Equal(bNum1.Id, context.BNums.Single(e => e.TheWalrus == "Eggman").Id);
                    Assert.Equal(bNum2.Id, context.BNums.Single(e => e.TheWalrus == "Eggmen").Id);

                    context.RemoveRange(context.BNums);

                    context.SaveChanges();
                }
            }
        }

        private class ENumContext : DbContext
        {
            public ENumContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<SNum> SNums { get; set; }
            public DbSet<EnNum> EnNums { get; set; }
            public DbSet<BNum> BNums { get; set; }
        }

        private class SNum
        {
            public string Id { get; set; }
            public string TheWalrus { get; set; }
        }

        private class EnNum
        {
            public ENum Id { get; set; }

            public string TheWalrus { get; set; }
        }

        private enum ENum
        {
            // ReSharper disable once UnusedMember.Local
            ANum,
            BNum,
            CNum
        }

        private class BNum
        {
            public byte[] Id { get; set; }
            public string TheWalrus { get; set; }
        }

        [Fact]
        public void Can_run_linq_query_on_entity_set()
        {
            using (var testStore = SqlServerTestStore.GetNorthwindStore())
            {
                using (var db = new NorthwindContext(Fixture.CreateOptions(testStore)))
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
            using (var testStore = SqlServerTestStore.GetNorthwindStore())
            {
                using (var db = new NorthwindContext(Fixture.CreateOptions(testStore)))
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
        public void Can_enumerate_entity_set()
        {
            using (var testStore = SqlServerTestStore.GetNorthwindStore())
            {
                using (var db = new NorthwindContext(Fixture.CreateOptions(testStore)))
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
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);
                using (var db = new BloggingContext(options))
                {
                    await CreateBlogDatabaseAsync<Blog>(db);
                }

                Fixture.TestSqlLoggerFactory.Clear();

                using (var db = new BloggingContext(options))
                {
                    var toUpdate = db.Blogs.Single(b => b.Name == "Blog1");
                    toUpdate.Name = "Blog is Updated";
                    var updatedId = toUpdate.Id;
                    var toDelete = db.Blogs.Single(b => b.Name == "Blog2");
                    toDelete.Name = "Blog to delete";
                    var deletedId = toDelete.Id;

                    db.Entry(toUpdate).State = EntityState.Modified;
                    db.Entry(toDelete).State = EntityState.Deleted;

                    var toAdd = db.Add(
                        new Blog
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

                    Assert.Equal(5, Fixture.TestSqlLoggerFactory.SqlStatements.Count);
                    Assert.Contains("SELECT", Fixture.TestSqlLoggerFactory.SqlStatements[0]);
                    Assert.Contains("SELECT", Fixture.TestSqlLoggerFactory.SqlStatements[1]);
                    Assert.Contains("@p0='" + deletedId, Fixture.TestSqlLoggerFactory.SqlStatements[2]);
                    Assert.Contains("DELETE", Fixture.TestSqlLoggerFactory.SqlStatements[2]);
                    Assert.Contains("UPDATE", Fixture.TestSqlLoggerFactory.SqlStatements[3]);
                    Assert.Contains("INSERT", Fixture.TestSqlLoggerFactory.SqlStatements[4]);

                    var rows = await testDatabase.ExecuteScalarAsync<int>(
                        $"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = {updatedId} AND Name = 'Blog is Updated'");

                    Assert.Equal(1, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        $"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = {deletedId}");

                    Assert.Equal(0, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        $"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = {addedId} AND Name = 'Blog to Insert'");

                    Assert.Equal(1, rows);
                }
            }
        }

        [Fact]
        public async Task Can_save_changes_in_tracked_entities()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                int updatedId;
                int deletedId;
                int addedId;
                var options = Fixture.CreateOptions(testDatabase);
                using (var db = new BloggingContext(options))
                {
                    var blogs = await CreateBlogDatabaseAsync<Blog>(db);

                    var toAdd = db.Blogs.Add(
                        new Blog
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

                using (var db = new BloggingContext(options))
                {
                    var toUpdate = db.Blogs.Single(b => b.Id == updatedId);
                    Assert.Equal("Blog is Updated", toUpdate.Name);
                    Assert.Equal(0, db.Blogs.Count(b => b.Id == deletedId));
                    Assert.Equal("Blog to Insert", db.Blogs.Single(b => b.Id == addedId).Name);
                }
            }
        }

        [Fact]
        public void Can_track_an_entity_with_more_than_10_properties()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);
                using (var context = new GameDbContext(options))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.Characters.Add(
                        new PlayerCharacter(
                            new Level
                            {
                                Game = new Game()
                            }));

                    context.SaveChanges();
                }

                using (var context = new GameDbContext(options))
                {
                    var character = context.Characters
                        .Include(c => c.Level.Game)
                        .OrderBy(c => c.Id)
                        .First();

                    Assert.NotNull(character.Game);
                    Assert.NotNull(character.Level);
                    Assert.NotNull(character.Level.Game);
                }
            }
        }

        [Fact]
        public void Adding_an_item_to_a_collection_marks_it_as_modified()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                using (var context = new GameDbContext(options))
                {
                    context.Database.EnsureCreatedResiliently();

                    var player = new PlayerCharacter(
                        new Level
                        {
                            Game = new Game()
                        });

                    var weapon = new Item
                    {
                        Id = 1,
                        Game = player.Game
                    };

                    context.Characters.Add(player);

                    context.SaveChanges();

                    player.Items.Add(weapon);

                    context.ChangeTracker.DetectChanges();

                    Assert.True(context.Entry(player).Collection(p => p.Items).IsModified);
                }
            }
        }

        [Fact]
        public void Can_set_reference_twice()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                using (var context = new GameDbContext(options))
                {
                    context.Database.EnsureCreatedResiliently();

                    var player = new PlayerCharacter(
                        new Level
                        {
                            Game = new Game()
                        });

                    var weapon = new Item
                    {
                        Id = 1,
                        Game = player.Game
                    };

                    player.Items.Add(weapon);
                    context.Characters.Add(player);

                    context.SaveChanges();

                    player.CurrentWeapon = weapon;
                    context.SaveChanges();

                    player.CurrentWeapon = null;
                    context.SaveChanges();

                    player.CurrentWeapon = weapon;
                    context.SaveChanges();
                }

                using (var context = new GameDbContext(options))
                {
                    var player = context.Characters
                        .Include(c => c.Items)
                        .ToList().Single();

                    Assert.Equal(player.Items.Single(), player.CurrentWeapon);
                }
            }
        }

        [Fact]
        public void Can_include_on_loaded_entity()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                using (var context = new GameDbContext(options))
                {
                    context.Database.EnsureCreatedResiliently();

                    var player = new PlayerCharacter(
                        new Level
                        {
                            Game = new Game()
                        });

                    var weapon = new Item
                    {
                        Id = 1,
                        Game = player.Game
                    };

                    player.Items.Add(weapon);

                    player.Items.Add(
                        new Item
                        {
                            Id = 2,
                            Game = player.Game
                        });

                    context.Characters.Add(player);

                    context.SaveChanges();

                    player.CurrentWeapon = weapon;

                    context.SaveChanges();
                }

                using (var context = new GameDbContext(options))
                {
                    var player = context.Characters
                        .Include(p => p.CurrentWeapon)
                        .Single();

                    Assert.Equal(1, player.Items.Count);

                    context.Attach(player);

                    Assert.Equal(1, player.Items.Count);

                    context.Levels
                        .Include(l => l.Actors)
                        .ThenInclude(a => a.Items)
                        .Load();

                    Assert.Equal(2, player.Items.Count);
                }

                using (var context = new GameDbContext(options))
                {
                    var player = context.Characters
                        .Include(p => p.CurrentWeapon)
                        .AsNoTracking()
                        .Single();

                    Assert.Equal(0, player.Items.Count);

                    context.Entry(player.CurrentWeapon).Property("ActorId").CurrentValue = 0;

                    context.Attach(player);

                    Assert.Equal(1, player.Items.Count);

                    context.Levels
                        .Include(l => l.Actors)
                        .ThenInclude(a => a.Items)
                        .Load();

                    Assert.Equal(2, player.Items.Count);
                }
            }
        }

        public abstract class Actor
        {
            protected Actor()
            {
            }

            protected Actor(Level level)
            {
                Level = level;
                Game = level.Game;
            }

            public virtual int Id { get; private set; }
            public virtual Level Level { get; set; }
            public virtual int GameId { get; private set; }
            public virtual Game Game { get; set; }
            public virtual ICollection<Item> Items { get; set; } = new HashSet<Item>();
        }

        public class PlayerCharacter : Actor
        {
            public PlayerCharacter()
            {
            }

            public PlayerCharacter(Level level)
                : base(level)
            {
            }

            public virtual string Name { get; set; }

            public virtual int Strength { get; set; }
            public virtual int Dexterity { get; set; }
            public virtual int Speed { get; set; }
            public virtual int Constitution { get; set; }
            public virtual int Intelligence { get; set; }
            public virtual int Willpower { get; set; }

            public virtual int MaxHP { get; set; }
            public virtual int HP { get; set; }

            public virtual int MaxMP { get; set; }
            public virtual int MP { get; set; }

            public virtual Item CurrentWeapon { get; set; }
        }

        public class Level
        {
            public virtual int Id { get; set; }
            public virtual int GameId { get; set; }
            public virtual Game Game { get; set; }
            public virtual ICollection<Actor> Actors { get; set; } = new HashSet<Actor>();
            public virtual ICollection<Item> Items { get; set; } = new HashSet<Item>();
        }

        public class Item
        {
            public virtual int Id { get; set; }
            public virtual int GameId { get; set; }
            public virtual Game Game { get; set; }
            public virtual Level Level { get; set; }
            public virtual Actor Actor { get; set; }
        }

        public class Container : Item
        {
            public virtual ICollection<Item> Items { get; set; } = new HashSet<Item>();
        }

        public class Game
        {
            public virtual int Id { get; set; }
            public virtual ICollection<Actor> Actors { get; set; } = new HashSet<Actor>();
            public virtual ICollection<Level> Levels { get; set; } = new HashSet<Level>();
        }

        public class GameDbContext : DbContext
        {
            public GameDbContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Game> Games { get; set; }
            public DbSet<Level> Levels { get; set; }
            public DbSet<PlayerCharacter> Characters { get; set; }
            public DbSet<Item> Items { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Level>(
                    eb =>
                    {
                        eb.HasKey(
                            l => new
                            {
                                l.GameId,
                                l.Id
                            });
                    });

                modelBuilder.Entity<Actor>(
                    eb =>
                    {
                        eb.HasKey(
                            a => new
                            {
                                a.GameId,
                                a.Id
                            });
                        eb.HasOne(a => a.Level)
                            .WithMany(l => l.Actors)
                            .HasForeignKey(nameof(Actor.GameId), "LevelId")
                            .IsRequired();

                        eb.HasMany(a => a.Items)
                            .WithOne(i => i.Actor)
                            .HasForeignKey(nameof(Item.GameId), "ActorId");
                    });

                modelBuilder.Entity<PlayerCharacter>(
                    eb =>
                    {
                        eb.HasOne(p => p.CurrentWeapon)
                            .WithOne()
                            .HasForeignKey<PlayerCharacter>(nameof(PlayerCharacter.GameId), "CurrentWeaponId");
                    });

                modelBuilder.Entity<Item>(
                    eb =>
                    {
                        eb.HasKey(
                            l => new
                            {
                                l.GameId,
                                l.Id
                            });
                    });

                modelBuilder.Entity<Container>();

                modelBuilder.Entity<Game>(
                    eb =>
                    {
                        eb.Property(g => g.Id)
                            .ValueGeneratedOnAdd();
                        eb.HasMany(g => g.Levels)
                            .WithOne(l => l.Game)
                            .HasForeignKey(l => l.GameId);
                        eb.HasMany(g => g.Actors)
                            .WithOne(a => a.Game)
                            .HasForeignKey(a => a.GameId)
                            .OnDelete(DeleteBehavior.Restrict);
                    });
            }
        }

        [Fact]
        public async Task Tracking_entities_asynchronously_returns_tracked_entities_back()
        {
            using (var testStore = SqlServerTestStore.GetNorthwindStore())
            {
                using (var db = new NorthwindContext(Fixture.CreateOptions(testStore)))
                {
                    var customer = await db.Customers.OrderBy(c => c.CustomerID).FirstOrDefaultAsync();

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
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testStore);

                await testStore.ExecuteNonQueryAsync("CREATE SCHEMA Apple");
                await testStore.ExecuteNonQueryAsync("CREATE TABLE Apple.Jack (MyKey int)");
                await testStore.ExecuteNonQueryAsync("CREATE TABLE Apple.Black (MyKey int)");

                using (var context = new SchemaContext(options))
                {
                    context.Add(
                        new Jack
                        {
                            MyKey = 1
                        });
                    context.Add(
                        new Black
                        {
                            MyKey = 2
                        });
                    context.SaveChanges();
                }

                using (var context = new SchemaContext(options))
                {
                    Assert.Equal(1, context.Jacks.Count());
                    Assert.Equal(1, context.Blacks.Count());
                }
            }
        }

        private class SchemaContext : DbContext
        {
            public SchemaContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Jack> Jacks { get; set; }
            public DbSet<Black> Blacks { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Jack>()
                    .ToTable("Jack", "Apple")
                    .HasKey(e => e.MyKey);

                modelBuilder
                    .Entity<Black>()
                    .ToTable("Black", "Apple")
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
        public Task Can_round_trip_changes_with_snapshot_change_tracking()
        {
            return RoundTripChanges<Blog>();
        }

        [Fact]
        public Task Can_round_trip_changes_with_full_notification_entities()
        {
            return RoundTripChanges<ChangedChangingBlog>();
        }

        [Fact]
        public Task Can_round_trip_changes_with_changed_only_notification_entities()
        {
            return RoundTripChanges<ChangedOnlyBlog>();
        }

        private async Task RoundTripChanges<TBlog>()
            where TBlog : class, IBlog, new()
        {
            using (var testDatabase = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                int blog1Id;
                int blog2Id;
                int blog3Id;

                using (var context = new BloggingContext<TBlog>(options))
                {
                    var blogs = await CreateBlogDatabaseAsync<TBlog>(context);
                    blog1Id = blogs[0].Id;
                    blog2Id = blogs[1].Id;

                    Assert.NotEqual(0, blog1Id);
                    Assert.NotEqual(0, blog2Id);
                    Assert.NotEqual(blog1Id, blog2Id);
                }

                using (var context = new BloggingContext<TBlog>(options))
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

                using (var context = new BloggingContext<TBlog>(options))
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

        private static async Task<TBlog[]> CreateBlogDatabaseAsync<TBlog>(DbContext context)
            where TBlog : class, IBlog, new()
        {
            context.Database.EnsureCreatedResiliently();

            var blog1 = context.Add(
                new TBlog
                {
                    Name = "Blog1",
                    George = true,
                    TheGu = new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9BF"),
                    NotFigTime = new DateTime(1973, 9, 3, 0, 10, 33, 777),
                    ToEat = 64,
                    CupOfChar = 'C',
                    OrNothing = 0.123456789,
                    Fuse = 777,
                    WayRound = 9876543210,
                    NotToEat = -64,
                    Away = 0.12345f,
                    OrULong = 888,
                    OrUSkint = 8888888,
                    OrUShort = 888888888888888,
                    AndChew = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                }).Entity;
            var blog2 = context.Add(
                new TBlog
                {
                    Name = "Blog2",
                    George = false,
                    TheGu = new Guid("0456AEF1-B7FC-47AA-8102-975D6BA3A9CF"),
                    NotFigTime = new DateTime(1973, 9, 3, 0, 10, 33, 778),
                    ToEat = 65,
                    CupOfChar = 'D',
                    OrNothing = 0.987654321,
                    Fuse = 778,
                    WayRound = 98765432100,
                    NotToEat = -64,
                    Away = 0.12345f,
                    OrULong = 888,
                    OrUSkint = 8888888,
                    OrUShort = 888888888888888,
                    AndChew = new byte[16]
                }).Entity;
            await context.SaveChangesAsync();

            return new[] { blog1, blog2 };
        }

        private class NorthwindContext : DbContext
        {
            public NorthwindContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.HasKey(c => c.CustomerID);
                        b.ToTable("Customers");
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
            public BloggingContext(DbContextOptions options)
                : base(options)
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

            public char CupOfChar { get; set; }
            public double OrNothing { get; set; }

            public short Fuse { get; set; }

            public long WayRound { get; set; }

            public sbyte NotToEat { get; set; }
            public float Away { get; set; }

            public ushort OrULong { get; set; }
            public uint OrUSkint { get; set; }
            public ulong OrUShort { get; set; }
            public byte[] AndChew { get; set; }
        }

        private class BloggingContext<TBlog> : DbContext
            where TBlog : class, IBlog
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                if (typeof(INotifyPropertyChanging).GetTypeInfo().IsAssignableFrom(typeof(TBlog).GetTypeInfo()))
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
                }
                else if (typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeof(TBlog).GetTypeInfo()))
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
                }

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

            char CupOfChar { get; set; }
            double OrNothing { get; set; }

            short Fuse { get; set; }

            long WayRound { get; set; }

            sbyte NotToEat { get; set; }
            float Away { get; set; }

            ushort OrULong { get; set; }
            uint OrUSkint { get; set; }
            ulong OrUShort { get; set; }
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

            private char _cupOfChar;
            private double _orNothing;

            private short _fuse;

            private long _wayRound;

            private sbyte _notToEat;
            private float _away;

            private ushort _orULong;
            private uint _orUSkint;
            private ulong _orUShort;
            private byte[] _andChew;

            public int Id
            {
                get => _id;
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
                get => _name;
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
                get => _george;
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
                get => _theGu;
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
                get => _notFigTime;
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
                get => _toEat;
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

            public char CupOfChar
            {
                get => _cupOfChar;
                set
                {
                    if (_cupOfChar != value)
                    {
                        NotifyChanging();
                        _cupOfChar = value;
                        NotifyChanged();
                    }
                }
            }

            public double OrNothing
            {
                get => _orNothing;
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
                get => _fuse;
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
                get => _wayRound;
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

            public sbyte NotToEat
            {
                get => _notToEat;
                set
                {
                    if (_notToEat != value)
                    {
                        NotifyChanging();
                        _notToEat = value;
                        NotifyChanged();
                    }
                }
            }

            public float Away
            {
                get => _away;
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

            public ushort OrULong
            {
                get => _orULong;
                set
                {
                    if (_orULong != value)
                    {
                        NotifyChanging();
                        _orULong = value;
                        NotifyChanged();
                    }
                }
            }

            public uint OrUSkint
            {
                get => _orUSkint;
                set
                {
                    if (_orUSkint != value)
                    {
                        NotifyChanging();
                        _orUSkint = value;
                        NotifyChanged();
                    }
                }
            }

            public ulong OrUShort
            {
                get => _orUShort;
                set
                {
                    if (_orUShort != value)
                    {
                        NotifyChanging();
                        _orUShort = value;
                        NotifyChanged();
                    }
                }
            }

            public byte[] AndChew
            {
                get => _andChew;
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

            private void NotifyChanged([CallerMemberName] string propertyName = "")
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private void NotifyChanging([CallerMemberName] string propertyName = "")
                => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        private class ChangedOnlyBlog : INotifyPropertyChanged, IBlog
        {
            private int _id;
            private string _name;
            private bool _george;
            private Guid _theGu;
            private DateTime _notFigTime;

            private byte _toEat;

            private char _cupOfChar;
            private double _orNothing;

            private short _fuse;

            private long _wayRound;

            private sbyte _notToEat;
            private float _away;

            private ushort _orULong;
            private uint _orUSkint;
            private ulong _orUShort;
            private byte[] _andChew;

            public int Id
            {
                get => _id;
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
                get => _name;
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
                get => _george;
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
                get => _theGu;
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
                get => _notFigTime;
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
                get => _toEat;
                set
                {
                    if (_toEat != value)
                    {
                        _toEat = value;
                        NotifyChanged();
                    }
                }
            }

            public char CupOfChar
            {
                get => _cupOfChar;
                set
                {
                    if (_cupOfChar != value)
                    {
                        _cupOfChar = value;
                        NotifyChanged();
                    }
                }
            }

            public double OrNothing
            {
                get => _orNothing;
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
                get => _fuse;
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
                get => _wayRound;
                set
                {
                    if (_wayRound != value)
                    {
                        _wayRound = value;
                        NotifyChanged();
                    }
                }
            }

            public sbyte NotToEat
            {
                get => _notToEat;
                set
                {
                    if (_notToEat != value)
                    {
                        _notToEat = value;
                        NotifyChanged();
                    }
                }
            }

            public float Away
            {
                get => _away;
                set
                {
                    if (_away != value)
                    {
                        _away = value;
                        NotifyChanged();
                    }
                }
            }

            public ushort OrULong
            {
                get => _orULong;
                set
                {
                    if (_orULong != value)
                    {
                        _orULong = value;
                        NotifyChanged();
                    }
                }
            }

            public uint OrUSkint
            {
                get => _orUSkint;
                set
                {
                    if (_orUSkint != value)
                    {
                        _orUSkint = value;
                        NotifyChanged();
                    }
                }
            }

            public ulong OrUShort
            {
                get => _orUShort;
                set
                {
                    if (_orUShort != value)
                    {
                        _orUShort = value;
                        NotifyChanged();
                    }
                }
            }

            public byte[] AndChew
            {
                get => _andChew;
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

            private void NotifyChanged([CallerMemberName] string propertyName = "")
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
