// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocMiscellaneousQueryCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    #region 21006

    [ConditionalFact]
    public virtual async Task Project_all_types_entity_with_missing_scalars()
    {
        var contextFactory = await InitializeAsync<JsonContext21006>(
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<JsonContext21006.Entity>();

        var result = await query.ToListAsync();
    }

    public void OnModelCreating21006(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JsonContext21006.Entity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.ToContainer("Entities");
            b.Property(x => x.TestDecimal).HasPrecision(18, 3);
            b.OwnsOne(x => x.Reference, bb =>
            {
                bb.Property(x => x.TestDecimal).HasPrecision(18, 3);
                bb.Property(x => x.TestEnumWithIntConverter).HasConversion<int>();
            });
        });
    }

    protected async Task Seed21006(JsonContext21006 context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var missingTopLevel =
$$"""
{
  "Id": 1,
  "$type": "Entity",
  "id": "1",
  "Reference": {
    "Text": "e2 or"
  }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingTopLevel,
            CancellationToken.None);
    }

    protected class JsonContext21006(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public class Entity
        {
            public int Id { get; set; }

            public short TestInt16 { get; set; }
            public int TestInt32 { get; set; }
            public long TestInt64 { get; set; }
            public double TestDouble { get; set; }
            public decimal TestDecimal { get; set; }
            public DateTime TestDateTime { get; set; }
            public DateTimeOffset TestDateTimeOffset { get; set; }
            public TimeSpan TestTimeSpan { get; set; }
            public DateOnly TestDateOnly { get; set; }
            public TimeOnly TestTimeOnly { get; set; }
            public float TestSingle { get; set; }
            public bool TestBoolean { get; set; }
            public byte TestByte { get; set; }

            public byte[] TestByteArray { get; set; }
            public Guid TestGuid { get; set; }
            public ushort TestUnsignedInt16 { get; set; }
            public uint TestUnsignedInt32 { get; set; }
            public ulong TestUnsignedInt64 { get; set; }
            public char TestCharacter { get; set; }
            public sbyte TestSignedByte { get; set; }
            public int? TestNullableInt32 { get; set; }
            public JsonEnum TestEnum { get; set; }
            public byte[] TestByteCollection { get; set; }
            public IList<ushort> TestUnsignedInt16Collection { get; set; }
            public uint[] TestUnsignedInt32Collection { get; set; }
            public sbyte[] TestSignedByteCollection { get; set; }
            public JsonEntity Reference { get; set; }
        }

        public class JsonEntity
        {
            public string Text { get; set; }

            public short TestInt16 { get; set; }
            public int TestInt32 { get; set; }
            public long TestInt64 { get; set; }
            public double TestDouble { get; set; }
            public decimal TestDecimal { get; set; }
            public DateTime TestDateTime { get; set; }
            public DateTimeOffset TestDateTimeOffset { get; set; }
            public TimeSpan TestTimeSpan { get; set; }
            public DateOnly TestDateOnly { get; set; }
            public TimeOnly TestTimeOnly { get; set; }
            public float TestSingle { get; set; }
            public bool TestBoolean { get; set; }
            public byte TestByte { get; set; }
            public byte[] TestByteArray { get; set; }
            public Guid TestGuid { get; set; }
            public ushort TestUnsignedInt16 { get; set; }
            public uint TestUnsignedInt32 { get; set; }
            public ulong TestUnsignedInt64 { get; set; }
            public char TestCharacter { get; set; }
            public sbyte TestSignedByte { get; set; }
            public int? TestNullableInt32 { get; set; }
            public JsonEnum TestEnum { get; set; }
            public JsonEnum TestEnumWithIntConverter { get; set; }

            public byte[] TestByteCollection { get; set; }
            public IList<ushort> TestUnsignedInt16Collection { get; set; }
            public uint[] TestUnsignedInt32Collection { get; set; }

            public sbyte[] TestSignedByteCollection { get; set; }
        }

        public enum JsonEnum
        {
            One = -1,
            Two = 2,
            Three = -3
        }
    }

    #endregion

    #region 34911

    [ConditionalFact]
    public virtual async Task Enum_partition_key()
    {
        var contextFactory = await InitializeAsync<Context34911>(
            onModelCreating: b => b.Entity<Context34911.Member>().HasPartitionKey(d => d.MemberType),
            seed: async context =>
            {
                context.Members.Add(new Context34911.Member { MemberType = Context34911.MemberType.Admin, Name = "Some Admin" });
                await context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateContext())
        {
            var admin = await context.Members.Where(p => p.MemberType == Context34911.MemberType.Admin).SingleAsync();
            Assert.Equal("Some Admin", admin.Name);
        }
    }

    protected class Context34911(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Member> Members { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Member>().HasData(new Member { Id = 1, Name = "Product 1" });

        public class Member
        {
            public int Id { get; set; }
            public MemberType MemberType { get; set; }
            public string Name { get; set; }
        }

        public enum MemberType
        {
            User,
            Admin
        }
    }

    #endregion 34911

    #region 35094

    // TODO: Move these tests to a better location. They require nullable properties with nulls in the database.

    [ConditionalFact]
    public virtual async Task Min_over_value_type_containing_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().MinAsync(p => p.NullableVal));
    }

    [ConditionalFact]
    public virtual async Task Min_over_value_type_containing_all_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().Where(e => e.NullableVal == null).MinAsync(p => p.NullableVal));
    }

    [ConditionalFact]
    public virtual async Task Min_over_reference_type_containing_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().MinAsync(p => p.NullableRef));
    }

    [ConditionalFact]
    public virtual async Task Min_over_reference_type_containing_all_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().Where(e => e.NullableRef == null).MinAsync(p => p.NullableRef));
    }

    [ConditionalFact]
    public virtual async Task Min_over_reference_type_containing_no_data()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().Where(e => e.Id < 0).MinAsync(p => p.NullableRef));
    }

    [ConditionalFact]
    public virtual async Task Max_over_value_type_containing_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Equal(3.14, await context.Set<Context35094.Product>().MaxAsync(p => p.NullableVal));
    }

    [ConditionalFact]
    public virtual async Task Max_over_value_type_containing_all_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().Where(e => e.NullableVal == null).MaxAsync(p => p.NullableVal));
    }

    [ConditionalFact]
    public virtual async Task Max_over_reference_type_containing_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Equal("Value", await context.Set<Context35094.Product>().MaxAsync(p => p.NullableRef));
    }

    [ConditionalFact]
    public virtual async Task Max_over_reference_type_containing_all_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().Where(e => e.NullableRef == null).MaxAsync(p => p.NullableRef));
    }

    [ConditionalFact]
    public virtual async Task Max_over_reference_type_containing_no_data()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().Where(e => e.Id < 0).MaxAsync(p => p.NullableRef));
    }

    [ConditionalFact]
    public virtual async Task Average_over_value_type_containing_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().AverageAsync(p => p.NullableVal));
    }

    [ConditionalFact]
    public virtual async Task Average_over_value_type_containing_all_nulls()
    {
        await using var context = (await InitializeAsync<Context35094>()).CreateContext();
        Assert.Null(await context.Set<Context35094.Product>().Where(e => e.NullableVal == null).AverageAsync(p => p.NullableVal));
    }

    protected class Context35094(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, NullableRef = "Value", NullableVal = 3.14 },
                new Product { Id = 2, NullableVal = 3.14 },
                new Product { Id = 3, NullableRef = "Value" });

        public class Product
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }
            public double? NullableVal { get; set; }
            public string NullableRef { get; set; }
        }
    }

    #endregion 35094

    protected override string StoreName
        => "AdHocMiscellaneousQueryTests";

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b => b.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
