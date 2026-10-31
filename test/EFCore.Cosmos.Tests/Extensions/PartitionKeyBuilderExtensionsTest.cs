// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class PartitionKeyBuilderExtensionsTest
{
    [Theory, InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public void Routing_uses_configured_JSON_representation(bool typeMappingDefault, bool converted)
    {
        var builder = CosmosTestHelpers.Instance.CreateConventionBuilder(
            configureConventions: c =>
            {
                if (typeMappingDefault)
                {
                    c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<Int32AsStringReaderWriter>();
                }
            });
        builder.Entity<Customer3>().HasPartitionKey(e => e.NullableInt);
        var propertyBuilder = builder.Entity<Customer3>().Property(e => e.NullableInt);
        if (!typeMappingDefault)
        {
            propertyBuilder.HasJsonValueReaderWriter<Int32AsStringReaderWriter>();
        }

        var counter = new ConversionCounter();
        if (converted)
        {
            propertyBuilder.HasConversion(new ValueConverter<int, int>(v => counter.Convert(v), v => v));
        }

        var property = builder.FinalizeModel().FindEntityType(typeof(Customer3))!.FindProperty(nameof(Customer3.NullableInt))!;
        Assert.Equal(
            new PartitionKey(converted ? "43" : "42"),
            new PartitionKeyBuilder().Add(42, property).Build());
        Assert.Equal(converted ? 1 : 0, counter.Calls);
        Assert.Equal(PartitionKey.Null, new PartitionKeyBuilder().Add(null, property).Build());
        if (converted)
        {
            Assert.Equal(new PartitionKey("43"), new PartitionKeyBuilder().Add("42", property).Build());
            Assert.Equal(2, counter.Calls);
        }
        else
        {
            Assert.Equal(
                CosmosStrings.PartitionKeyBadValueType("int", nameof(Customer3), nameof(Customer3.NullableInt), "string"),
                Assert.Throws<InvalidOperationException>(() => new PartitionKeyBuilder().Add("42", property)).Message);
        }
    }

    private sealed class ConversionCounter
    {
        public int Calls { get; private set; }

        public int Convert(int value)
        {
            Calls++;
            return value + 1;
        }
    }

    public sealed class Int32AsStringReaderWriter : JsonValueReaderWriter<int>
    {
        public static Int32AsStringReaderWriter Instance { get; } = new();

        public override int FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => int.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

        public override void ToJsonTyped(Utf8JsonWriter writer, int value)
            => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));

        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(Int32AsStringReaderWriter), nameof(Instance));
    }

    [Fact]
    public void Add_expected_value_types()
    {
        using var context = new PartitionKeyContext();

        var builder = new PartitionKeyBuilder();

        builder.Add("1", FindProperty(context, typeof(Customer1), nameof(Customer1.String)));
        builder.Add(1.1, FindProperty(context, typeof(Customer1), nameof(Customer1.Double)));
        builder.Add(1, FindProperty(context, typeof(Customer1), nameof(Customer1.Int)));
        builder.Add(true, FindProperty(context, typeof(Customer2), nameof(Customer2.Bool)));
        builder.Add("1.1", FindProperty(context, typeof(Customer2), nameof(Customer2.NullableString)));
        builder.Add(1.1, FindProperty(context, typeof(Customer2), nameof(Customer2.NullableDouble)));
        builder.Add(true, FindProperty(context, typeof(Customer3), nameof(Customer3.NullableBool)));
        builder.Add(1, FindProperty(context, typeof(Customer3), nameof(Customer3.NullableInt)));
        builder.Add(Guid.NewGuid(), FindProperty(context, typeof(Customer3), nameof(Customer3.Guid)));
        builder.Add(Guid.NewGuid(), FindProperty(context, typeof(Customer4), nameof(Customer4.NullableGuid)));
        builder.Add(1, FindProperty(context, typeof(Customer4), nameof(Customer4.Long)));
        builder.Add(1, FindProperty(context, typeof(Customer4), nameof(Customer4.NullableLong)));

        builder.Add((double?)1.1, FindProperty(context, typeof(Customer2), nameof(Customer2.NullableDouble)));
        builder.Add((bool?)true, FindProperty(context, typeof(Customer3), nameof(Customer3.NullableBool)));
        builder.Add((int?)1, FindProperty(context, typeof(Customer3), nameof(Customer3.NullableInt)));
        builder.Add((Guid?)Guid.NewGuid(), FindProperty(context, typeof(Customer4), nameof(Customer4.NullableGuid)));
        builder.Add((int?)1, FindProperty(context, typeof(Customer4), nameof(Customer4.NullableLong)));

        builder.Add(null, FindProperty(context, typeof(Customer2), nameof(Customer2.NullableDouble)));
        builder.Add(null, FindProperty(context, typeof(Customer3), nameof(Customer3.NullableBool)));
        builder.Add(null, FindProperty(context, typeof(Customer3), nameof(Customer3.NullableInt)));
        builder.Add(null, FindProperty(context, typeof(Customer4), nameof(Customer4.NullableGuid)));
        builder.Add(null, FindProperty(context, typeof(Customer4), nameof(Customer4.NullableLong)));
    }

    [Fact]
    public void Throw_for_unexpected_types()
    {
        using var context = new PartitionKeyContext();

        var builder = new PartitionKeyBuilder();

        Assert.Equal(
            CosmosStrings.PartitionKeyBadValueType("string", nameof(Customer1), nameof(Customer1.String), "int"),
            Assert.Throws<InvalidOperationException>(() => builder.Add(
                1, FindProperty(context, typeof(Customer1), nameof(Customer1.String)))).Message);

        Assert.Equal(
            CosmosStrings.PartitionKeyBadValueType("bool", nameof(Customer2), nameof(Customer2.Bool), "int"),
            Assert.Throws<InvalidOperationException>(() => builder.Add(1, FindProperty(context, typeof(Customer2), nameof(Customer2.Bool))))
                .Message);

        Assert.Equal(
            CosmosStrings.PartitionKeyBadValueType("double", nameof(Customer1), nameof(Customer1.Double), "string"),
            Assert.Throws<InvalidOperationException>(() => builder.Add(
                "1", FindProperty(context, typeof(Customer1), nameof(Customer1.Double)))).Message);

        Assert.Equal(
            CosmosStrings.PartitionKeyBadValueType("int", nameof(Customer1), nameof(Customer1.Int), "string"),
            Assert.Throws<InvalidOperationException>(() => builder.Add(
                "1", FindProperty(context, typeof(Customer1), nameof(Customer1.Int)))).Message);
    }

    private static IProperty FindProperty(PartitionKeyContext context, Type type, string name)
        => context.Model.FindEntityType(type)!.FindProperty(name)!;

    private class PartitionKeyContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer1>(cb =>
            {
                cb.ToContainer("C1");
                cb.HasPartitionKey(c => new
                {
                    PartitionKey1 = c.String,
                    PartitionKey2 = c.Double,
                    PartitionKey3 = c.Int
                });
            });

            modelBuilder.Entity<Customer2>(cb =>
            {
                cb.ToContainer("C2");
                cb.HasPartitionKey(c => new
                {
                    PartitionKey4 = c.Bool,
                    PartitionKey5 = c.NullableString,
                    PartitionKey6 = c.NullableDouble
                });
            });

            modelBuilder.Entity<Customer3>(cb =>
            {
                cb.ToContainer("C3");
                cb.HasPartitionKey(c => new
                {
                    PartitionKey7 = c.NullableInt,
                    PartitionKey8 = c.NullableBool,
                    PartitionKey9 = c.Guid
                });
            });

            modelBuilder.Entity<Customer4>(cb =>
            {
                cb.ToContainer("C4");
                cb.HasPartitionKey(c => new { PartitionKey7 = c.NullableGuid });
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
                "https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "_");
    }

    public class Customer1
    {
        public int Id { get; set; }

        public string String { get; set; } = null!;
        public double Double { get; set; }
        public int Int { get; set; }
    }

    public class Customer2
    {
        public int Id { get; set; }
        public bool Bool { get; set; }
        public string? NullableString { get; set; }
        public double? NullableDouble { get; set; }
    }

    public class Customer3
    {
        public int Id { get; set; }
        public int? NullableInt { get; set; }
        public bool? NullableBool { get; set; }
        public Guid Guid { get; set; }
    }

    public class Customer4
    {
        public int Id { get; set; }
        public Guid? NullableGuid { get; set; }
        public long Long { get; set; }
        public long? NullableLong { get; set; }
    }
}
