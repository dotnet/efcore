// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using NameSpace1;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SimpleQueryRelationalTestBase : SimpleQueryTestBase
    {
        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected void ClearLog()
            => TestSqlLoggerFactory.Clear();

        protected void AssertSql(params string[] expected)
            => TestSqlLoggerFactory.AssertBaseline(expected);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Multiple_different_entity_type_from_different_namespaces(bool async)
        {
            var contextFactory = await InitializeAsync<Context23981>();
            using var context = contextFactory.CreateContext();
            //var good1 = context.Set<NameSpace1.TestQuery>().FromSqlRaw(@"SELECT 1 AS MyValue").ToList(); // OK
            //var good2 = context.Set<NameSpace2.TestQuery>().FromSqlRaw(@"SELECT 1 AS MyValue").ToList(); // OK
            var bad = context.Set<TestQuery>().FromSqlRaw(@"SELECT cast(null as int) AS MyValue").ToList(); // Exception
        }

        protected class Context23981 : DbContext
        {
            public Context23981(DbContextOptions options)
                : base(options)
            {
            }

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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task StoreType_for_UDF_used(bool async)
        {
            var contextFactory = await InitializeAsync<Context27954>();
            using var context = contextFactory.CreateContext();

            var date = new DateTime(2012, 12, 12);
            var query1 = context.Set<MyEntity>().Where(x => x.SomeDate == date);
            var query2 = context.Set<MyEntity>().Where(x => MyEntity.Modify(x.SomeDate) == date);

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

        protected class Context27954 : DbContext
        {
            public Context27954(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<MyEntity> MyEntities { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder
                    .HasDbFunction(typeof(MyEntity).GetMethod(nameof(MyEntity.Modify)))
                    .HasName("ModifyDate")
                    .HasStoreType("datetime")
                    .HasSchema("dbo");
        }

        protected class MyEntity
        {
            public int Id { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime SomeDate { get; set; }

            public static DateTime Modify(DateTime date)
                => throw new NotSupportedException();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Hierarchy_query_with_abstract_type_sibling_TPC(bool async)
            => Hierarchy_query_with_abstract_type_sibling_helper(
                async,
                mb =>
                {
                    mb.Entity<Animal>().UseTpcMappingStrategy();
                    mb.Entity<Pet>().ToTable("Pets");
                    mb.Entity<Cat>().ToTable("Cats");
                    mb.Entity<Dog>().ToTable("Dogs");
                    mb.Entity<FarmAnimal>().ToTable("FarmAnimals");
                });

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Hierarchy_query_with_abstract_type_sibling_TPT(bool async)
            => Hierarchy_query_with_abstract_type_sibling_helper(
                async,
                mb =>
                {
                    mb.Entity<Animal>().UseTptMappingStrategy();
                    mb.Entity<Pet>().ToTable("Pets");
                    mb.Entity<Cat>().ToTable("Cats");
                    mb.Entity<Dog>().ToTable("Dogs");
                    mb.Entity<FarmAnimal>().ToTable("FarmAnimals");
                });
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
