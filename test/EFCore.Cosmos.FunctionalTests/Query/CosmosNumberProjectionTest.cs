// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

[ConditionalClass(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]// https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/335
public class NumberProjectionCosmosTest : QueryTestBase<NumberProjectionCosmosTest.NumberTypesQueryCosmosFixture>
{
    public NumberProjectionCosmosTest(NumberTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public async Task Int_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = e.Int / (e.Int - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Int"] / (c["Int"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_int_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Int != null).Select(e => new { e.Id, Value = e.Int / (e.Int - 1) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Int != null).Select(e => new { e.Id, Value = e.Int / (e.Int - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Int"] / (c["Int"] - 1))
}
FROM root c
WHERE (c["Int"] != null)
""");
    }

    [Fact]
    public async Task Long_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = e.Long / (e.Long - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Long"] / (c["Long"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_long_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Long != null).Select(e => new { e.Id, Value = e.Long / (e.Long - 1) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Long != null).Select(e => new { e.Id, Value = e.Long / (e.Long - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Long"] / (c["Long"] - 1))
}
FROM root c
WHERE (c["Long"] != null)
""");
    }

    [Fact]
    public async Task Byte_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = (byte)(e.Byte / (e.Byte - 1)) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Byte"] / (c["Byte"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_byte_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Byte != null).Select(e => new { e.Id, Value = (byte?)(e.Byte / (e.Byte - 1)) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Byte != null).Select(e => new { e.Id, Value = (byte?)(e.Byte / (e.Byte - 1)) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Byte"] / (c["Byte"] - 1))
}
FROM root c
WHERE (c["Byte"] != null)
""");
    }

    [Fact]
    public async Task Short_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = e.Short / (e.Short - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Short"] / (c["Short"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_short_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Short != null).Select(e => new { e.Id, Value = e.Short / (e.Short - 1) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Short != null).Select(e => new { e.Id, Value = e.Short / (e.Short - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Short"] / (c["Short"] - 1))
}
FROM root c
WHERE (c["Short"] != null)
""");
    }

    [Fact]
    public async Task Float_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = e.Float / (e.Float - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Float"] / (c["Float"] - 1.0))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_float_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Float != null).Select(e => new { e.Id, Value = e.Float / (e.Float - 1) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.Float != null).Select(e => new { e.Id, Value = e.Float / (e.Float - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["Float"] / (c["Float"] - 1.0))
}
FROM root c
WHERE (c["Float"] != null)
""");
    }

    [Fact]
    public async Task SByte_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = (byte)(e.SByte / (e.SByte - 1)) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["SByte"] / (c["SByte"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_sbyte_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.SByte != null).Select(e => new { e.Id, Value = (byte?)(e.SByte / (e.SByte - 1)) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.SByte != null).Select(e => new { e.Id, Value = (byte?)(e.SByte / (e.SByte - 1)) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["SByte"] / (c["SByte"] - 1))
}
FROM root c
WHERE (c["SByte"] != null)
""");
    }

    [Fact]
    public async Task UShort_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = e.UShort / (e.UShort - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["UShort"] / (c["UShort"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_ushort_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.UShort != null).Select(e => new { e.Id, Value = e.UShort / (e.UShort - 1) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.UShort != null).Select(e => new { e.Id, Value = e.UShort / (e.UShort - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["UShort"] / (c["UShort"] - 1))
}
FROM root c
WHERE (c["UShort"] != null)
""");
    }

    [Fact]
    public async Task UInt_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = e.UInt / (e.UInt - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["UInt"] / (c["UInt"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_uint_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.UInt != null).Select(e => new { e.Id, Value = e.UInt / (e.UInt - 1) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.UInt != null).Select(e => new { e.Id, Value = e.UInt / (e.UInt - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["UInt"] / (c["UInt"] - 1))
}
FROM root c
WHERE (c["UInt"] != null)
""");
    }

    [Fact]
    public async Task ULong_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new { e.Id, Value = e.ULong / (e.ULong - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["ULong"] / (c["ULong"] - 1))
}
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_ulong_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.ULong != null).Select(e => new { e.Id, Value = e.ULong / (e.ULong - 1) }),
            ss => ss.Set<NullableNumberTypesEntity>().Where(e => e.ULong != null).Select(e => new { e.Id, Value = e.ULong / (e.ULong - 1) }),
            x => x.Id, (e, a) => Assert.Equal(e.Value, a.Value));

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["Id"],
    "Value" : (c["ULong"] / (c["ULong"] - 1))
}
FROM root c
WHERE (c["ULong"] != null)
""");
    }

    [Fact]
    public async Task Int_subprojection_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new[] { e.Int / (e.Int - 1), e.Int / (e.Int - 1) }.Average()),
            ss => ss.Set<NumberTypesEntity>().Select(e => new[] { (double)e.Int / (e.Int - 1), (double)e.Int / (e.Int - 1) }.Average())); // Cosmos treats numbers as doubles

        AssertSql(
            """
SELECT VALUE (
    SELECT VALUE AVG(a)
    FROM a IN (SELECT VALUE [(c["Int"] / (c["Int"] - 1)), (c["Int"] / (c["Int"] - 1))]))
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_int_subprojection_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Select(e => new[] { e.Int!.Value / (e.Int.Value - 1), e.Int.Value / (e.Int.Value - 1) }.Average()),
            ss => ss.Set<NullableNumberTypesEntity>().Where(x => x.Int != null).Select(e => new[] { (double)e.Int! / (e.Int.Value - 1), (double)e.Int! / (e.Int.Value - 1) }.Average())); // Cosmos treats numbers as doubles

        AssertSql(
            """
SELECT VALUE (
    SELECT VALUE AVG(a)
    FROM a IN (SELECT VALUE [(c["Int"] / (c["Int"] - 1)), (c["Int"] / (c["Int"] - 1))]))
FROM root c
""");
    }

    [Fact]
    public async Task Float_subprojection_devided()
    {
        await AssertQuery(ss => ss.Set<NumberTypesEntity>().Select(e => new float[] { e.Float / (e.Float - 1), e.Float / (e.Float - 1) }.Average()),
            ss => ss.Set<NumberTypesEntity>().Select(e => new float[] { e.Float / (e.Float - 1), e.Float / (e.Float - 1) }.Average()));

        AssertSql(
            """
SELECT VALUE (
    SELECT VALUE AVG(a)
    FROM a IN (SELECT VALUE [(c["Float"] / (c["Float"] - 1.0)), (c["Float"] / (c["Float"] - 1.0))]))
FROM root c
""");
    }

    [Fact]
    public async Task Nullable_float_subprojection_devided()
    {
        await AssertQuery(ss => ss.Set<NullableNumberTypesEntity>().Select(e => new float[] { e.Float!.Value / (e.Float.Value - 1), e.Float.Value / (e.Float.Value - 1) }.Average()),
            ss => ss.Set<NullableNumberTypesEntity>().Where(x => x.Float != null).Select(e => new float[] { e.Float!.Value / (e.Float.Value - 1), e.Float.Value / (e.Float.Value - 1) }.Average()));

        AssertSql(
            """
SELECT VALUE (
    SELECT VALUE AVG(a)
    FROM a IN (SELECT VALUE [(c["Float"] / (c["Float"] - 1.0)), (c["Float"] / (c["Float"] - 1.0))]))
FROM root c
""");
    }


    [Fact]
    public async Task Int_devided_zero_aggregated()
    {
        await AssertSum(true, ss => ss.Set<NumberTypesEntity>().Select(e => e.Int / (e.Int - 1)),
                              (e, a) => Assert.Equal(4, a)); // Cosmos treats numbers as doubles, so this becomes 1.1 * 4 which is 4.4 which is 4 in ints

        AssertSql(
            """
SELECT VALUE SUM((c["Int"] / (c["Int"] - 1)))
FROM root c
""");
    }

    [Fact]
    public async Task Int_devided_aggregated()
    {
        await AssertSum(true, ss => ss.Set<NumberTypesEntity>().Select(e => e.Int / (e.Int + 1)),
                              ss => ss.Set<NumberTypesEntity>().Select(e => e.Int == 0 ? 0 : 1),
                              (e, a) => Assert.Equal(3, a)); // Cosmos treats numbers as doubles, so this becomes 0.9 * 4, which is 3.6, which is 3 in ints

        AssertSql(
            """
SELECT VALUE SUM((c["Int"] / (c["Int"] + 1)))
FROM root c
""");
    }

    [Fact]
    public async Task Int_as_double_devided_aggregated()
    {
        await AssertSum(true, ss => ss.Set<NumberTypesEntity>().Select(e => (double)e.Int / (e.Int - 1)));

        AssertSql(
            """
SELECT VALUE SUM((c["Int"] / (c["Int"] - 1)))
FROM root c
""");
    }

    [Fact]
    public async Task Float_devided_aggregated()
    {
        await AssertSum(true, ss => ss.Set<NumberTypesEntity>().Select(e => e.Float / (e.Float - 1)));

        AssertSql(
            """
SELECT VALUE SUM((c["Float"] / (c["Float"] - 1.0)))
FROM root c
""");
    }

    [Fact]
    public async Task Float_devided_int_aggregated()
    {
        await AssertSum(true, ss => ss.Set<NumberTypesEntity>().Select(e => e.Float / 2));

        AssertSql(
            """
SELECT VALUE SUM((c["Float"] / 2.0))
FROM root c
""");
    }

    private void AssertSql(params string[] expected)
       => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class NumberTypesEntity
    {
        public required int Id { get; set; }
        public byte Byte { get; set; }
        public short Short { get; set; }
        public int Int { get; set; }
        public long Long { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public sbyte SByte { get; set; }
        public ushort UShort { get; set; }
        public uint UInt { get; set; }
        public ulong ULong { get; set; }
    }

    public class NullableNumberTypesEntity
    {
        public required int Id { get; set; }
        public byte? Byte { get; set; }
        public short? Short { get; set; }
        public int? Int { get; set; }
        public long? Long { get; set; }
        public float? Float { get; set; }
        public double? Double { get; set; }
        public decimal? Decimal { get; set; }
        public sbyte? SByte { get; set; }
        public ushort? UShort { get; set; }
        public uint? UInt { get; set; }
        public ulong? ULong { get; set; }
    }

    public class NumberTypesContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<NumberTypesEntity> NumberTypesEntities { get; set; } = null!;
        public DbSet<NullableNumberTypesEntity> NullableNumberTypesEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumberTypesEntity>().Property(b => b.Id).ValueGeneratedNever();
            modelBuilder.Entity<NullableNumberTypesEntity>().Property(b => b.Id).ValueGeneratedNever();
        }
    }

    public class NumberTypesData : ISetSource
    {
        public IReadOnlyList<NumberTypesEntity> NumberTypesEntities { get; } = CreateNumberTypesEntities();
        public IReadOnlyList<NullableNumberTypesEntity> NullableNumberTypesEntities { get; } = CreateNullableNumberTypesEntities();

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(NumberTypesEntity))
            {
                return (IQueryable<TEntity>)NumberTypesEntities.AsQueryable();
            }

            if (typeof(TEntity) == typeof(NullableNumberTypesEntity))
            {
                return (IQueryable<TEntity>)NullableNumberTypesEntities.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static IReadOnlyList<NumberTypesEntity> CreateNumberTypesEntities()
            =>
            [
                new()
                {
                    Id = 1,
                    Byte = 0, Short = 0, Int = 0, Long = 0,
                    Float = 0, Double = 0, Decimal = 0,
                    SByte = 0, UShort = 0, UInt = 0, ULong = 0,
                },
                new()
                {
                    Id = 2,
                    Byte = 8, Short = 8, Int = 8, Long = 8,
                    Float = 8.6f, Double = 8.6, Decimal = 8.6m,
                    SByte = 8, UShort = 8, UInt = 8, ULong = 8,
                },
                new()
                {
                    Id = 3,
                    Byte = 255, Short = 255, Int = 255, Long = 255,
                    Float = 255.12f, Double = 255.12, Decimal = 255.12m,
                    SByte = 127, UShort = 255, UInt = 255, ULong = 255,
                },
                new()
                {
                    Id = 4,
                    Byte = 9, Short = -9, Int = -9, Long = -9,
                    Float = -9.5f, Double = -9.5, Decimal = -9.5m,
                    SByte = -9, UShort = 9, UInt = 9, ULong = 9,
                },
                new()
                {
                    Id = 5,
                    Byte = 12, Short = 12, Int = 12, Long = 12,
                    Float = 12, Double = 12, Decimal = 12,
                    SByte = 12, UShort = 12, UInt = 12, ULong = 12,
                },
            ];

        public static IReadOnlyList<NullableNumberTypesEntity> CreateNullableNumberTypesEntities() => CreateNumberTypesEntities()
                .Select(n => new NullableNumberTypesEntity
                {
                    Id = n.Id,
                    Byte = n.Byte,
                    Short = n.Short,
                    Int = n.Int,
                    Long = n.Long,
                    Float = n.Float,
                    Double = n.Double,
                    Decimal = n.Decimal,
                    SByte = n.SByte,
                    UShort = n.UShort,
                    UInt = n.UInt,
                    ULong = n.ULong,
                })
                .Append(
                    new NullableNumberTypesEntity
                    {
                        Id = -1,
                        Byte = null,
                        Short = null,
                        Int = null,
                        Long = null,
                        Float = null,
                        Double = null,
                        Decimal = null,
                        SByte = null,
                        UShort = null,
                        UInt = null,
                        ULong = null,
                    })
                .ToArray();
    }

    public class NumberTypesQueryCosmosFixture : QueryFixtureBase<NumberTypesContext>
    {
        private NumberTypesData? _expectedData;

        protected override string StoreName
            => "NumberTypesTest";

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(NumberTypesContext context)
        {
            var data = new NumberTypesData();
            context.AddRange(data.NumberTypesEntities);
            context.AddRange(data.NullableNumberTypesEntities);
            return context.SaveChangesAsync();
        }

        public override ISetSource GetExpectedData()
            => _expectedData ??= new NumberTypesData();

        public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, object>();

        public override IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, object>();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<NumberTypesEntity>(builder =>
            {
                builder.ToContainer(nameof(NumberTypesEntity));
                builder.HasPartitionKey(b => b.Id);
            });
            modelBuilder.Entity<NullableNumberTypesEntity>(builder =>
            {
                builder.ToContainer(nameof(NullableNumberTypesEntity));
                builder.HasPartitionKey(n => n.Id);
            });
        }
    }
}
