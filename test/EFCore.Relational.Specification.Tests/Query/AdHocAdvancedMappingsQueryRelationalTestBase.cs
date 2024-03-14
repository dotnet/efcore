// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocAdvancedMappingsQueryRelationalTestBase : AdHocAdvancedMappingsQueryTestBase
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    #region 32911

    [ConditionalFact]
    public virtual async Task Two_similar_complex_properties_projected_with_split_query1()
    {
        var contextFactory = await InitializeAsync<Context32911>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();
        var query = context.Offers
            .Include(e => e.Variations)
            .ThenInclude(v => v.Nested)
            .AsSplitQuery()
            .ToList();

        var resultElement = query.Single();
        foreach (var variation in resultElement.Variations)
        {
            Assert.NotEqual(variation.Payment.Brutto, variation.Nested.Payment.Brutto);
            Assert.NotEqual(variation.Payment.Netto, variation.Nested.Payment.Netto);
        }
    }

    [ConditionalFact]
    public virtual async Task Two_similar_complex_properties_projected_with_split_query2()
    {
        var contextFactory = await InitializeAsync<Context32911>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();
        var query = context.Offers
            .Include(e => e.Variations)
            .ThenInclude(v => v.Nested)
            .AsSplitQuery()
            .Single(x => x.Id == 1);

        foreach (var variation in query.Variations)
        {
            Assert.NotEqual(variation.Payment.Brutto, variation.Nested.Payment.Brutto);
            Assert.NotEqual(variation.Payment.Netto, variation.Nested.Payment.Netto);
        }
    }

    [ConditionalFact]
    public virtual async Task Projecting_one_of_two_similar_complex_types_picks_the_correct_one()
    {
        var contextFactory = await InitializeAsync<Context32911_2>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();

        var query = context.Cs
            .Where(x => x.B.AId.Value == 1)
            .OrderBy(x => x.Id)
            .Take(10)
            .Select(
                x => new
                {
                    x.B.A.Id, x.B.Info.Created,
                }).ToList();

        Assert.Equal(new DateTime(2000, 1, 1), query[0].Created);
    }

    protected class Context32911(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Offer> Offers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Offer>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Variation>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Variation>().ComplexProperty(
                x => x.Payment, cpb =>
                {
                    cpb.IsRequired();
                    cpb.Property(p => p.Netto).HasColumnName("payment_netto");
                    cpb.Property(p => p.Brutto).HasColumnName("payment_brutto");
                });
            modelBuilder.Entity<NestedEntity>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<NestedEntity>().ComplexProperty(
                x => x.Payment, cpb =>
                {
                    cpb.IsRequired();
                    cpb.Property(p => p.Netto).HasColumnName("payment_netto");
                    cpb.Property(p => p.Brutto).HasColumnName("payment_brutto");
                });
        }

        public async Task SeedAsync()
        {
            var v1 = new Variation
            {
                Id = 1,
                Payment = new Payment(1, 10),
                Nested = new NestedEntity { Id = 1, Payment = new Payment(10, 100) }
            };

            var v2 = new Variation
            {
                Id = 2,
                Payment = new Payment(2, 20),
                Nested = new NestedEntity { Id = 2, Payment = new Payment(20, 200) }
            };

            var v3 = new Variation
            {
                Id = 3,
                Payment = new Payment(3, 30),
                Nested = new NestedEntity { Id = 3, Payment = new Payment(30, 300) }
            };

            Offers.Add(
                new Offer
                {
                    Id = 1,
                    Variations = new List<Variation>
                    {
                        v1,
                        v2,
                        v3
                    }
                });

            await SaveChangesAsync();
        }

        public abstract class EntityBase
        {
            public int Id { get; set; }
        }

        public class Offer : EntityBase
        {
            public ICollection<Variation> Variations { get; set; }
        }

        public class Variation : EntityBase
        {
            public Payment Payment { get; set; } = new Payment(0, 0);

            public NestedEntity Nested { get; set; }
        }

        public class NestedEntity : EntityBase
        {
            public Payment Payment { get; set; } = new Payment(0, 0);
        }

        public record Payment(decimal Netto, decimal Brutto);
    }

    protected class Context32911_2(DbContextOptions options) : DbContext(options)
    {
        public DbSet<A> As { get; set; }
        public DbSet<B> Bs { get; set; }
        public DbSet<C> Cs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<A>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<B>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<C>().Property(x => x.Id).ValueGeneratedNever();

            modelBuilder.Entity<B>(x => x.ComplexProperty(b => b.Info).IsRequired());
            modelBuilder.Entity<C>(x => x.ComplexProperty(c => c.Info).IsRequired());
        }

        public async Task SeedAsync()
        {
            var c = new C
            {
                Id = 100,
                Info = new Metadata { Created = new DateTime(2020, 10, 10) },
                B = new B
                {
                    Id = 10,
                    Info = new Metadata { Created = new DateTime(2000, 1, 1) },
                    A = new A { Id = 1 }
                }
            };

            Cs.Add(c);
            await SaveChangesAsync();
        }

        public class Metadata
        {
            public DateTime Created { get; set; }
        }

        public class A
        {
            public int Id { get; set; }
        }

        public class B
        {
            public int Id { get; set; }
            public Metadata Info { get; set; }
            public int? AId { get; set; }

            public A A { get; set; }
        }

        public class C
        {
            public int Id { get; set; }
            public Metadata Info { get; set; }
            public int BId { get; set; }

            public B B { get; set; }
        }
    }

    #endregion

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hierarchy_query_with_abstract_type_sibling_TPC(bool async)
        => Hierarchy_query_with_abstract_type_sibling_helper(
            async,
            mb =>
            {
                mb.Entity<Context28196.Animal>().UseTpcMappingStrategy();
                mb.Entity<Context28196.Pet>().ToTable("Pets");
                mb.Entity<Context28196.Cat>().ToTable("Cats");
                mb.Entity<Context28196.Dog>().ToTable("Dogs");
                mb.Entity<Context28196.FarmAnimal>().ToTable("FarmAnimals");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hierarchy_query_with_abstract_type_sibling_TPT(bool async)
        => Hierarchy_query_with_abstract_type_sibling_helper(
            async,
            mb =>
            {
                mb.Entity<Context28196.Animal>().UseTptMappingStrategy();
                mb.Entity<Context28196.Pet>().ToTable("Pets");
                mb.Entity<Context28196.Cat>().ToTable("Cats");
                mb.Entity<Context28196.Dog>().ToTable("Dogs");
                mb.Entity<Context28196.FarmAnimal>().ToTable("FarmAnimals");
            });
}
