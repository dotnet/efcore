// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using NameSpace1;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AdHocMiscellaneousQueryRelationalTestBase(NonSharedFixture fixture) : AdHocMiscellaneousQueryTestBase(fixture)
    {
        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected void ClearLog()
            => TestSqlLoggerFactory.Clear();

        protected void AssertSql(params string[] expected)
            => TestSqlLoggerFactory.AssertBaseline(expected);

        protected abstract DbContextOptionsBuilder SetParameterizedCollectionMode(
            DbContextOptionsBuilder optionsBuilder,
            ParameterTranslationMode parameterizedCollectionMode);

        #region 2951

        [Fact]
        public virtual async Task Query_when_null_key_in_database_should_throw()
        {
            var contextFactory = await InitializeNonSharedTest<Context2951>(
                onConfiguring: o => o.EnableDetailedErrors(),
                seed: Seed2951);

            using var context = contextFactory.CreateDbContext();

            Assert.Equal(
                RelationalStrings.ErrorMaterializingPropertyNullReference(nameof(Context2951.ZeroKey2951), "Id", typeof(int)),
                (await Assert.ThrowsAsync<InvalidOperationException>(() => context.ZeroKeys.ToListAsync())).Message);
        }

        protected abstract Task Seed2951(Context2951 context);

        protected class Context2951(DbContextOptions options) : DbContext(options)
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<ZeroKey2951>().ToTable("ZeroKey", t => t.ExcludeFromMigrations())
                    .Property(z => z.Id).ValueGeneratedNever();

            public DbSet<ZeroKey2951> ZeroKeys { get; set; }

            public class ZeroKey2951
            {
                public int Id { get; set; }
            }
        }

        #endregion

        #region 11818

        [Fact]
        public virtual async Task GroupJoin_Anonymous_projection_GroupBy_Aggregate_join_elimination()
        {
            var contextFactory = await InitializeNonSharedTest<Context11818>(
                onConfiguring:
                o => o.ConfigureWarnings(w => w.Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)));

            using (var context = contextFactory.CreateDbContext())
            {
                var query = (from e in context.Set<Context11818.Entity11818>()
                             join a in context.Set<Context11818.AnotherEntity11818>()
                                 on e.Id equals a.Id into grouping
                             from a in grouping.DefaultIfEmpty()
                             select new { ename = e.Name, aname = a.Name })
                    .GroupBy(g => g.aname)
                    .Select(g => new { g.Key, cnt = g.Count() + 5 })
                    .ToList();

                Assert.Empty(query);
            }

            using (var context = contextFactory.CreateDbContext())
            {
                var query = (from e in context.Set<Context11818.Entity11818>()
                             join a in context.Set<Context11818.AnotherEntity11818>()
                                 on e.Id equals a.Id into grouping
                             from a in grouping.DefaultIfEmpty()
                             join m in context.Set<Context11818.MaumarEntity11818>()
                                 on e.Id equals m.Id into grouping2
                             from m in grouping2.DefaultIfEmpty()
                             select new { aname = a.Name, mname = m.Name })
                    .GroupBy(g => new { g.aname, g.mname })
                    .Select(g => new { MyKey = g.Key.aname, cnt = g.Count() + 5 })
                    .ToList();

                Assert.Empty(query);
            }

            using (var context = contextFactory.CreateDbContext())
            {
                var query = (from e in context.Set<Context11818.Entity11818>()
                             join a in context.Set<Context11818.AnotherEntity11818>()
                                 on e.Id equals a.Id into grouping
                             from a in grouping.DefaultIfEmpty()
                             join m in context.Set<Context11818.MaumarEntity11818>()
                                 on e.Id equals m.Id into grouping2
                             from m in grouping2.DefaultIfEmpty()
                             select new { aname = a.Name, mname = m.Name })
                    .OrderBy(g => g.aname)
                    .GroupBy(g => new { g.aname, g.mname })
                    .Select(g => new { MyKey = g.Key.aname, cnt = g.Key.mname }).FirstOrDefault();

                Assert.Null(query);
            }
        }

        protected class Context11818(DbContextOptions options) : DbContext(options)
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity11818>().ToTable("Table");
                modelBuilder.Entity<AnotherEntity11818>().ToTable("Table");
                modelBuilder.Entity<MaumarEntity11818>().ToTable("Table");

                modelBuilder.Entity<Entity11818>()
                    .HasOne<AnotherEntity11818>()
                    .WithOne()
                    .HasForeignKey<AnotherEntity11818>(b => b.Id);

                modelBuilder.Entity<Entity11818>()
                    .HasOne<MaumarEntity11818>()
                    .WithOne()
                    .HasForeignKey<MaumarEntity11818>(b => b.Id);
            }

            public class Entity11818
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class AnotherEntity11818
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public bool Exists { get; set; }
            }

            public class MaumarEntity11818
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public bool Exists { get; set; }
            }
        }

        #endregion

        #region 23981

        [Theory, MemberData(nameof(IsAsyncData))]
        public virtual async Task Multiple_different_entity_type_from_different_namespaces(bool async)
        {
            var contextFactory = await InitializeNonSharedTest<Context23981>();
            using var context = contextFactory.CreateDbContext();
            //var good1 = context.Set<NameSpace1.TestQuery>().FromSqlRaw(@"SELECT 1 AS MyValue").ToList(); // OK
            //var good2 = context.Set<NameSpace2.TestQuery>().FromSqlRaw(@"SELECT 1 AS MyValue").ToList(); // OK
            var bad = context.Set<TestQuery>().FromSqlRaw(@"SELECT cast(null as int) AS MyValue").ToList(); // Exception
        }

        protected class Context23981(DbContextOptions options) : DbContext(options)
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var mb = modelBuilder.Entity(typeof(TestQuery));

                mb.HasBaseType((Type)null);
                mb.HasNoKey();
                mb.ToTable((string)null);

                mb = modelBuilder.Entity(typeof(NameSpace2.TestQuery));

                mb.HasBaseType((Type)null);
                mb.HasNoKey();
                mb.ToTable((string)null);
            }
        }

        #endregion

        #region 27954

        [Theory, MemberData(nameof(IsAsyncData))]
        public virtual async Task StoreType_for_UDF_used(bool async)
        {
            var contextFactory = await InitializeNonSharedTest<Context27954>();
            using var context = contextFactory.CreateDbContext();

            var date = new DateTime(2012, 12, 12);
            var query1 = context.Set<Context27954.MyEntity>().Where(x => x.SomeDate == date);
            var query2 = context.Set<Context27954.MyEntity>().Where(x => Context27954.MyEntity.Modify(x.SomeDate) == date);

            if (async)
            {
                await query1.ToListAsync();
                await Assert.ThrowsAnyAsync<Exception>(() => query2.ToListAsync());
            }
            else
            {
                query1.ToList();
                Assert.ThrowsAny<Exception>(() => query2.ToList());
            }
        }

        protected class Context27954(DbContextOptions options) : DbContext(options)
        {
            public DbSet<MyEntity> MyEntities { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder
                    .HasDbFunction(typeof(MyEntity).GetMethod(nameof(MyEntity.Modify)))
                    .HasName("ModifyDate")
                    .HasStoreType("datetime")
                    .HasSchema("dbo");

            public class MyEntity
            {
                public int Id { get; set; }

                [Column(TypeName = "datetime")]
                public DateTime SomeDate { get; set; }

                public static DateTime Modify(DateTime date)
                    => throw new NotSupportedException();
            }
        }

        #endregion

        #region 34752

        [Fact]
        public virtual async Task Mapping_JsonElement_property_throws_a_meaningful_exception()
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => InitializeNonSharedTest<Context34752>())).Message;

            Assert.Equal(
                CoreStrings.PropertyNotAdded(nameof(Context34752.Entity), nameof(Context34752.Entity.Json), nameof(JsonElement)),
                message);
        }

        protected class Context34752(DbContextOptions options) : DbContext(options)
        {
            public DbSet<Entity> Entities { get; set; }

            public class Entity
            {
                public int Id { get; set; }
                public JsonElement Json { get; set; }
            }
        }

        #endregion

        #region Inlined redacting

        [Theory, MemberData(nameof(InlinedRedactingData))]
        public virtual async Task Check_inlined_constants_redacting(bool async, bool enableSensitiveDataLogging)
        {
            var contextFactory = await InitializeNonSharedTest<InlinedRedactingContext>(
                onConfiguring: o =>
                {
                    SetParameterizedCollectionMode(o, ParameterTranslationMode.Constant);
                    o.EnableSensitiveDataLogging(enableSensitiveDataLogging);
                });
            using var context = contextFactory.CreateDbContext();

            var id = 1;
            var ids = new[] { id, 2, 3 };
            var query1 = context.TestEntities.Where(x => ids.Contains(x.Id));
            var query2 = context.TestEntities.Where(x => ids.Where(y => y == x.Id).Any());
            var query3 = context.TestEntities.Where(x => EF.Constant(id) == x.Id);

            if (async)
            {
                await query1.ToListAsync();
                await query2.ToListAsync();
                await query3.ToListAsync();
            }
            else
            {
                query1.ToList();
                query2.ToList();
                query3.ToList();
            }
        }

        protected class InlinedRedactingContext(DbContextOptions options) : DbContext(options)
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            public class TestEntity
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }

        public static readonly IEnumerable<object[]> InlinedRedactingData = [[true, true], [true, false], [false, true], [false, false]];

        #endregion

        #region 36311

        [Theory, MemberData(nameof(IsAsyncData))]
        public virtual async Task Entity_equality_with_Contains_and_Parameter(bool async)
        {
            var contextFactory = await InitializeNonSharedTest<Context36311>(
                onConfiguring: o => SetParameterizedCollectionMode(o, ParameterTranslationMode.Parameter));
            using var context = contextFactory.CreateDbContext();

            List<Context36311.BlogDetails> details = [new() { Id = 1 }, new() { Id = 2 }];
            var query = context.Blogs.Where(b => details.Contains(b.Details));

            var result = async
                ? await query.ToListAsync()
                : query.ToList();
        }

        protected class Context36311(DbContextOptions options) : DbContext(options)
        {
            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public BlogDetails Details { get; set; }
            }

            public class BlogDetails
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }

        #endregion

        #region 36247

        [Theory, MemberData(nameof(IsAsyncData))]
        public virtual async Task Like_on_value_converted_string_column_does_not_produce_cast(bool async)
        {
            var contextFactory = await InitializeNonSharedTest<Context36247>(
                seed: async ctx =>
                {
                    ctx.Users.AddRange(
                        new Context36247.User { Name = new Context36247.FullName("Name1") },
                        new Context36247.User { Name = new Context36247.FullName("Name2") });
                    await ctx.SaveChangesAsync();
                });
            using var context = contextFactory.CreateDbContext();

            var query = context.Users.Where(x => EF.Functions.Like(x.Name, "Name%"));

            var result = async
                ? await query.ToListAsync()
                : [.. query];

            Assert.Equal(2, result.Count);
        }

        protected class Context36247(DbContextOptions options) : DbContext(options)
        {
            public DbSet<User> Users { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<User>().Property(e => e.Name)
                    .HasConversion(v => v.Value, v => new FullName(v));

            public class User
            {
                public int Id { get; set; }
                public FullName Name { get; set; }
            }

            public readonly record struct FullName(string Value)
            {
                public static implicit operator string(FullName fullName)
                    => fullName.Value;
            }
        }

        #endregion

        #region 30915

        // Characterization (golden-master) tests for LEFT JOIN / DefaultIfEmpty / LeftJoin-operator
        // shapes that project a NON-ENTITY (anon type / DTO / struct / GroupBy-aggregate) from the
        // nullable side. EVERY test asserts the ACTUAL CURRENT behavior on the base code (no fix
        // applied): if the query works, it asserts the correct results; if it throws, it asserts the
        // throw. Tests that currently throw are flagged with "#30915 TODO" so they can be flipped to
        // assert results once the underlying issue is fixed. SQLite only.

        // ---------------------------------------------------------------------------------------
        // Category A: whole non-entity object projected from the nullable side
        // ---------------------------------------------------------------------------------------

        [Fact] // 1
        public virtual async Task Anon_whole_object_GroupJoin_DefaultIfEmpty()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        orderby s.PickupStatusId
                        select new { s.PickupStatusId, countInfo };

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            // status 1 -> matched, Count 2
            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].countInfo);
            Assert.Equal(1, result[0].countInfo.pickupStatusId);
            Assert.Equal(2, result[0].countInfo.Count);

            // status 2 -> no match, whole non-entity object is null
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.Null(result[1].countInfo);

            // status 3 -> matched, Count 1
            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].countInfo);
            Assert.Equal(3, result[2].countInfo.pickupStatusId);
            Assert.Equal(1, result[2].countInfo.Count);
        }

        [Fact] // 2
        public virtual async Task Anon_whole_object_LeftJoin_operator()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = context.Statuses
                .LeftJoin(categories, s => s.PickupStatusId, c => c.pickupStatusId, (s, countInfo) => new { s.PickupStatusId, countInfo })
                .OrderBy(e => e.PickupStatusId);

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            // status 1 -> matched, Count 2
            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].countInfo);
            Assert.Equal(1, result[0].countInfo.pickupStatusId);
            Assert.Equal(2, result[0].countInfo.Count);

            // status 2 -> no match, whole non-entity object is null
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.Null(result[1].countInfo);

            // status 3 -> matched, Count 1
            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].countInfo);
            Assert.Equal(3, result[2].countInfo.pickupStatusId);
            Assert.Equal(1, result[2].countInfo.Count);
        }

        [Fact] // 3
        public virtual async Task Anon_client_null_check_GroupJoin()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        orderby s.PickupStatusId
                        select new { s.PickupStatusId, Count = countInfo == null ? 0 : countInfo.Count };

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);
            // matched -> count; no-match (status 2) -> client null-check yields 0
            Assert.Equal((1, 2), (result[0].PickupStatusId, result[0].Count));
            Assert.Equal((2, 0), (result[1].PickupStatusId, result[1].Count));
            Assert.Equal((3, 1), (result[2].PickupStatusId, result[2].Count));
        }

        [Fact] // 4
        public virtual async Task Anon_client_null_check_LeftJoin_operator()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = context.Statuses
                .LeftJoin(
                    categories, s => s.PickupStatusId, c => c.pickupStatusId,
                    (s, countInfo) => new { s.PickupStatusId, Count = countInfo == null ? 0 : countInfo.Count })
                .OrderBy(e => e.PickupStatusId);

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);
            // matched -> count; no-match (status 2) -> client null-check yields 0
            Assert.Equal((1, 2), (result[0].PickupStatusId, result[0].Count));
            Assert.Equal((2, 0), (result[1].PickupStatusId, result[1].Count));
            Assert.Equal((3, 1), (result[2].PickupStatusId, result[2].Count));
        }

        [Fact] // 5 - CONTROL: member access with nullable cast, likely works
        public virtual async Task Anon_member_only_nullable_cast()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        orderby s.PickupStatusId
                        select new { s.PickupStatusId, Count = (int?)countInfo.Count };

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal((1, (int?)2), (result[0].PickupStatusId, result[0].Count));
            Assert.Equal((2, (int?)null), (result[1].PickupStatusId, result[1].Count));
            Assert.Equal((3, (int?)1), (result[2].PickupStatusId, result[2].Count));
        }

        [Fact] // 6
        public virtual async Task Dto_memberinit_whole_object_LeftJoin()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new Context30915.CountDto30915 { PickupStatusId = k, Count = els.Count() });

            var query = context.Statuses
                .LeftJoin(categories, s => s.PickupStatusId, c => c.PickupStatusId, (s, countInfo) => new { s.PickupStatusId, countInfo })
                .OrderBy(e => e.PickupStatusId);

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            // status 1 -> matched, Count 2
            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].countInfo);
            Assert.Equal(1, result[0].countInfo.PickupStatusId);
            Assert.Equal(2, result[0].countInfo.Count);

            // status 2 -> no match, whole DTO object is null
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.Null(result[1].countInfo);

            // status 3 -> matched, Count 1
            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].countInfo);
            Assert.Equal(3, result[2].countInfo.PickupStatusId);
            Assert.Equal(1, result[2].countInfo.Count);
        }

        [Fact] // 7
        public virtual async Task Dto_constructor_whole_object_LeftJoin()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new Context30915.CountDtoCtor30915(k, els.Count()));

            var query = context.Statuses
                .LeftJoin(categories, s => s.PickupStatusId, c => c.PickupStatusId, (s, countInfo) => new { s.PickupStatusId, countInfo })
                .OrderBy(e => e.PickupStatusId);

            // Constructor-bound (read-only) DTO from the nullable side currently fails to translate
            // (rather than failing during materialization like the member-init / struct shapes).
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("could not be translated", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 8
        public virtual async Task Struct_whole_object_LeftJoin()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new Context30915.CountStruct30915 { PickupStatusId = k, Count = els.Count() });

            var query = context.Statuses
                .LeftJoin(categories, s => s.PickupStatusId, c => c.PickupStatusId, (s, countInfo) => new { s.PickupStatusId, countInfo })
                .OrderBy(e => e.PickupStatusId);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("Nullable object must have a value", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 9
        public virtual async Task RecordStruct_whole_object_LeftJoin()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new Context30915.CountRecordStruct30915(k, els.Count()));

            var query = context.Statuses
                .LeftJoin(categories, s => s.PickupStatusId, c => c.PickupStatusId, (s, countInfo) => new { s.PickupStatusId, countInfo })
                .OrderBy(e => e.PickupStatusId);

            // Positional record struct (constructor-bound) from the nullable side currently fails to
            // translate (rather than failing during materialization like the member-init shapes).
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("could not be translated", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 10
        public virtual async Task Nested_anon_whole_object()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        orderby s.PickupStatusId
                        select new { s.PickupStatusId, Wrap = new { countInfo } };

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            // status 1 -> matched: outer wrapper non-null, nested countInfo non-null
            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].Wrap);
            Assert.NotNull(result[0].Wrap.countInfo);
            Assert.Equal(1, result[0].Wrap.countInfo.pickupStatusId);
            Assert.Equal(2, result[0].Wrap.countInfo.Count);

            // status 2 -> no match: outer wrapper still materializes, nested countInfo is null
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.NotNull(result[1].Wrap);
            Assert.Null(result[1].Wrap.countInfo);

            // status 3 -> matched: outer wrapper non-null, nested countInfo non-null
            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].Wrap);
            Assert.NotNull(result[2].Wrap.countInfo);
            Assert.Equal(3, result[2].Wrap.countInfo.pickupStatusId);
            Assert.Equal(1, result[2].Wrap.countInfo.Count);
        }

        // ---------------------------------------------------------------------------------------
        // Category B: post-join operators
        // ---------------------------------------------------------------------------------------

        [Fact] // 11
        public virtual async Task GroupBy_after_join_then_whole_object()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = (from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         select new { s, countInfo })
                .GroupBy(e => e.s.PickupStatusId, (key, els) => new { key, anyInfo = els.Select(e => e.countInfo).FirstOrDefault() })
                .OrderBy(e => e.key);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("Nullable object must have a value", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 12
        public virtual async Task Second_join_after_then_whole_object()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = (from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         select new { s.PickupStatusId, countInfo })
                .Join(context.Statuses, e => e.PickupStatusId, s2 => s2.PickupStatusId, (e, s2) => new { s2.PickupStatusId, e.countInfo })
                .OrderBy(e => e.PickupStatusId);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("Nullable object must have a value", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 13 - plain inner with no aggregate -> no pushdown
        public virtual async Task Plain_inner_no_aggregate_LeftJoin_whole_object()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests.Select(r => new { r.PickupStatusId, Count = 1 });

            var query = context.Statuses
                .LeftJoin(categories, s => s.PickupStatusId, c => c.PickupStatusId, (s, countInfo) => new { s.PickupStatusId, countInfo })
                .OrderBy(e => e.PickupStatusId);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("Nullable object must have a value", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 14
        public virtual async Task Distinct_after_join_member()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = (from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         select new { s.PickupStatusId, Count = countInfo == null ? 0 : countInfo.Count })
                .Distinct();

            var result = (await query.ToListAsync())
                .OrderBy(e => e.PickupStatusId)
                .ToList();

            Assert.Equal(3, result.Count);
            // matched -> count; no-match (status 2) -> client null-check yields 0
            Assert.Equal((1, 2), (result[0].PickupStatusId, result[0].Count));
            Assert.Equal((2, 0), (result[1].PickupStatusId, result[1].Count));
            Assert.Equal((3, 1), (result[2].PickupStatusId, result[2].Count));
        }

        [Fact] // 15
        public virtual async Task Union_of_two_leftjoin_nonentity()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var first = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        select new { s.PickupStatusId, Count = countInfo == null ? 0 : countInfo.Count };

            var second = from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         select new { s.PickupStatusId, Count = countInfo == null ? 0 : countInfo.Count };

            var query = first.Union(second);

            // The client-side null-check projection forces a client projection on each operand,
            // which then can't participate in the set operation.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("Unable to translate set operation", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 16
        public virtual async Task OrderBy_member_of_nullable_projection()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = (from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         orderby (countInfo == null ? 0 : countInfo.Count), s.PickupStatusId
                         select new { s.PickupStatusId, Count = countInfo == null ? 0 : countInfo.Count });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("could not be translated", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 17
        public virtual async Task Take_after_join_whole_object()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = (from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         orderby s.PickupStatusId
                         select new { s.PickupStatusId, countInfo })
                .Take(10);

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            // status 1 -> matched, Count 2
            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].countInfo);
            Assert.Equal(1, result[0].countInfo.pickupStatusId);
            Assert.Equal(2, result[0].countInfo.Count);

            // status 2 -> no match, whole non-entity object is null
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.Null(result[1].countInfo);

            // status 3 -> matched, Count 1
            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].countInfo);
            Assert.Equal(3, result[2].countInfo.pickupStatusId);
            Assert.Equal(1, result[2].countInfo.Count);
        }

        [Fact] // 18
        public virtual async Task Where_nonentity_projection_not_null_serverside()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = (from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         where countInfo != null
                         orderby s.PickupStatusId
                         select s.PickupStatusId);

            // Server-side null-check against a whole non-entity projection from the nullable side
            // currently cannot be translated.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("could not be translated", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        [Fact] // 19
        public virtual async Task Where_nonentity_projection_null_serverside()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() });

            var query = (from s in context.Statuses
                         join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                         from countInfo in g.DefaultIfEmpty()
                         where countInfo == null
                         orderby s.PickupStatusId
                         select s.PickupStatusId);

            // Server-side null-check against a whole non-entity projection from the nullable side
            // currently cannot be translated.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
            Assert.Contains("could not be translated", ex.Message);
            // #30915 TODO: currently throws on base; flip to assert results if/when fixed.
        }

        // ---------------------------------------------------------------------------------------
        // Category C: member kinds inside the projected object
        // ---------------------------------------------------------------------------------------

        [Fact] // 20
        public virtual async Task Projected_object_with_nullable_member()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, MaxPriority = els.Max(x => x.Priority) });

            var query = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        orderby s.PickupStatusId
                        select new { s.PickupStatusId, countInfo };

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            // status 1 -> matched, MaxPriority = Max(5, null) = 5
            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].countInfo);
            Assert.Equal(1, result[0].countInfo.pickupStatusId);
            Assert.Equal(5, result[0].countInfo.MaxPriority);

            // status 2 -> no match, whole non-entity object is null
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.Null(result[1].countInfo);

            // status 3 -> matched, MaxPriority = 7
            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].countInfo);
            Assert.Equal(3, result[2].countInfo.pickupStatusId);
            Assert.Equal(7, result[2].countInfo.MaxPriority);
        }

        [Fact] // 21
        public virtual async Task Projected_object_with_string_member()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count(), Name = "cat" });

            var query = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        orderby s.PickupStatusId
                        select new { s.PickupStatusId, countInfo };

            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            // status 1 -> matched, Count 2, Name "cat"
            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].countInfo);
            Assert.Equal(1, result[0].countInfo.pickupStatusId);
            Assert.Equal(2, result[0].countInfo.Count);
            Assert.Equal("cat", result[0].countInfo.Name);

            // status 2 -> no match, whole non-entity object is null
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.Null(result[1].countInfo);

            // status 3 -> matched, Count 1, Name "cat"
            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].countInfo);
            Assert.Equal(3, result[2].countInfo.pickupStatusId);
            Assert.Equal(1, result[2].countInfo.Count);
            Assert.Equal("cat", result[2].countInfo.Name);
        }

        [Fact] // 22 - only nullable/reference members; all-null may be representable
        public virtual async Task Projected_object_all_nullable_members()
        {
            var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
            using var context = contextFactory.CreateDbContext();

            var categories = context.Requests
                .GroupBy(
                    r => r.PickupStatusId,
                    (k, els) => new { pickupStatusId = (int?)k, MaxPriority = els.Max(x => x.Priority) });

            var query = from s in context.Statuses
                        join c in categories on s.PickupStatusId equals c.pickupStatusId into g
                        from countInfo in g.DefaultIfEmpty()
                        orderby s.PickupStatusId
                        select new { s.PickupStatusId, countInfo };

            // Even though every member is nullable (so an all-null object would be representable), the
            // fix makes DefaultIfEmpty produce default(T) == null for the WHOLE projected object on a
            // no-match, consistent with all the other whole-object shapes.
            var result = await query.ToListAsync();

            Assert.Equal(3, result.Count);

            Assert.Equal(1, result[0].PickupStatusId);
            Assert.NotNull(result[0].countInfo);
            Assert.Equal(1, result[0].countInfo.pickupStatusId);
            Assert.Equal(5, result[0].countInfo.MaxPriority);

            // status 2 -> no match: the whole non-entity object is null (default(T)), not a non-null
            // object with all-null members.
            Assert.Equal(2, result[1].PickupStatusId);
            Assert.Null(result[1].countInfo);

            Assert.Equal(3, result[2].PickupStatusId);
            Assert.NotNull(result[2].countInfo);
            Assert.Equal(3, result[2].countInfo.pickupStatusId);
            Assert.Equal(7, result[2].countInfo.MaxPriority);
        }

        protected abstract Task Seed30915(Context30915 context);

        protected class Context30915(DbContextOptions options) : DbContext(options)
        {
            public DbSet<PickupStatus30915> Statuses { get; set; }
            public DbSet<PickupRequest30915> Requests { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<PickupStatus30915>(
                    b =>
                    {
                        b.HasKey(e => e.PickupStatusId);
                        b.Property(e => e.PickupStatusId).ValueGeneratedNever();
                    });

            public class PickupStatus30915
            {
                public int PickupStatusId { get; set; }
                public string Name { get; set; }
            }

            public class PickupRequest30915
            {
                public int Id { get; set; }
                public int PickupStatusId { get; set; }
                public int? Priority { get; set; }
            }

            public class CountDto30915
            {
                public int PickupStatusId { get; set; }
                public int Count { get; set; }
            }

            public class CountDtoCtor30915(int pickupStatusId, int count)
            {
                public int PickupStatusId { get; } = pickupStatusId;
                public int Count { get; } = count;
            }

            public struct CountStruct30915
            {
                public int PickupStatusId { get; set; }
                public int Count { get; set; }
            }

            public record struct CountRecordStruct30915(int PickupStatusId, int Count);
        }

        #endregion
    }
}

namespace NameSpace1
{
    public class TestQuery
    {
        public int? MyValue { get; set; }
    }
}

namespace NameSpace2
{
    public class TestQuery
    {
        public int MyValue { get; set; }
    }
}
