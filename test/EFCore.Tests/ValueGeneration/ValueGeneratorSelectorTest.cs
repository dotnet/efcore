// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class ValueGeneratorSelectorTest
{
    [ConditionalFact]
    public void Returns_built_in_generators_for_types_setup_for_value_generation_using_Try_method()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = new ValueGeneratorSelector(
            new ValueGeneratorSelectorDependencies(new ValueGeneratorCache(new ValueGeneratorCacheDependencies())));

        Assert.IsType<CustomValueGenerator>(
            selector.TrySelect(entityType.FindProperty("Custom")!, entityType, out var generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("Id")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("Long")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("Short")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("Byte")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableInt")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableLong")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableShort")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableByte")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("UInt")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("ULong")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("UShort")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("SByte")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableUInt")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableULong")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableUShort")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableSByte")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("Decimal")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableDecimal")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("Float")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableFloat")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("Double")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableDouble")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("DateTime")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableDateTime")!, entityType, out generator) ? generator : null);

        Assert.Null(selector.TrySelect(entityType.FindProperty("DateTimeOffset")!, entityType, out generator) ? generator : null);
        Assert.Null(selector.TrySelect(entityType.FindProperty("NullableDateTimeOffset")!, entityType, out generator) ? generator : null);

        Assert.IsType<StringValueGenerator>(
            selector.TrySelect(entityType.FindProperty("String")!, entityType, out generator) ? generator : null);

        Assert.IsType<GuidValueGenerator>(
            selector.TrySelect(entityType.FindProperty("Guid")!, entityType, out generator) ? generator : null);
        Assert.IsType<GuidValueGenerator>(
            selector.TrySelect(entityType.FindProperty("NullableGuid")!, entityType, out generator) ? generator : null);

        Assert.IsType<BinaryValueGenerator>(
            selector.TrySelect(entityType.FindProperty("Binary")!, entityType, out generator) ? generator : null);
    }

    [ConditionalFact]
    public void Returns_null_for_unsupported_combinations_with_Try_method()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);

        var selector = contextServices.GetRequiredService<IValueGeneratorSelector>();

        Assert.False(selector.TrySelect(entityType.FindProperty("Random")!, entityType, out var valueGenerator));
        Assert.Null(valueGenerator);
    }

    private static IModel BuildModel(bool generateValues = true)
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>().Property(e => e.Custom).HasValueGenerator<CustomValueGenerator>();
        var entityType = builder.Model.FindEntityType(typeof(AnEntity));

        foreach (var property in entityType.GetProperties())
        {
            property.ValueGenerated = generateValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
        }

        return builder.FinalizeModel();
    }

    private class AnEntity
    {
        public int Id { get; set; }
        public int Custom { get; set; }
        public long Long { get; set; }
        public short Short { get; set; }
        public byte Byte { get; set; }
        public int? NullableInt { get; set; }
        public long? NullableLong { get; set; }
        public short? NullableShort { get; set; }
        public byte? NullableByte { get; set; }
        public uint UInt { get; set; }
        public ulong ULong { get; set; }
        public ushort UShort { get; set; }
        public sbyte SByte { get; set; }
        public uint? NullableUInt { get; set; }
        public ulong? NullableULong { get; set; }
        public ushort? NullableUShort { get; set; }
        public sbyte? NullableSByte { get; set; }
        public string String { get; set; }
        public Guid Guid { get; set; }
        public Guid? NullableGuid { get; set; }
        public byte[] Binary { get; set; }
        public float Float { get; set; }
        public float? NullableFloat { get; set; }
        public double Double { get; set; }
        public double? NullableDouble { get; set; }
        public decimal Decimal { get; set; }
        public decimal? NullableDecimal { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public DateTimeOffset? NullableDateTimeOffset { get; set; }
        public char Random { get; set; }
    }

    private class CustomValueGenerator : ValueGenerator<int>
    {
        public override int Next(EntityEntry entry)
            => throw new NotImplementedException();

        public override bool GeneratesTemporaryValues
            => false;
    }

    private static object CreateAndUseFactory(IProperty property)
    {
        var model = BuildModel();

        var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        selector.TrySelect(property, property.DeclaringType, out var generator);

        return generator!.Next(null!);
    }
}
