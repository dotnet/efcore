// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class InMemoryValueGeneratorSelectorTest
{
    [ConditionalFact]
    public void Returns_built_in_generators_for_types_setup_for_value_generation_using_Try_method()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.IsType<CustomValueGenerator>(
            selector.TrySelect(entityType.FindProperty("Custom")!, entityType, out var generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<int>>(
            selector.TrySelect(entityType.FindProperty("Id")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<long>>(
            selector.TrySelect(entityType.FindProperty("Long")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<short>>(
            selector.TrySelect(entityType.FindProperty("Short")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<byte>>(
            selector.TrySelect(entityType.FindProperty("Byte")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<int>>(
            selector.TrySelect(entityType.FindProperty("NullableInt")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<long>>(
            selector.TrySelect(entityType.FindProperty("NullableLong")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<short>>(
            selector.TrySelect(entityType.FindProperty("NullableShort")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<byte>>(
            selector.TrySelect(entityType.FindProperty("NullableByte")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<uint>>(
            selector.TrySelect(entityType.FindProperty("UInt")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<ulong>>(
            selector.TrySelect(entityType.FindProperty("ULong")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<ushort>>(
            selector.TrySelect(entityType.FindProperty("UShort")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<sbyte>>(
            selector.TrySelect(entityType.FindProperty("SByte")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<uint>>(
            selector.TrySelect(entityType.FindProperty("NullableUInt")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<ulong>>(
            selector.TrySelect(entityType.FindProperty("NullableULong")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<ushort>>(
            selector.TrySelect(entityType.FindProperty("NullableUShort")!, entityType, out generator) ? generator : null);
        Assert.IsType<InMemoryIntegerValueGenerator<sbyte>>(
            selector.TrySelect(entityType.FindProperty("NullableSByte")!, entityType, out generator) ? generator : null);
        Assert.IsType<StringValueGenerator>(
            selector.TrySelect(entityType.FindProperty("String")!, entityType, out generator) ? generator : null);
        Assert.IsType<GuidValueGenerator>(
            selector.TrySelect(entityType.FindProperty("Guid")!, entityType, out generator) ? generator : null);
        Assert.IsType<BinaryValueGenerator>(
            selector.TrySelect(entityType.FindProperty("Binary")!, entityType, out generator) ? generator : null);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public void Can_create_factories_for_all_integer_types(bool useTry)
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        Assert.Equal(1, CreateAndUseFactory(entityType.FindProperty("Id")));
        Assert.Equal(1L, CreateAndUseFactory(entityType.FindProperty("Long")));
        Assert.Equal((short)1, CreateAndUseFactory(entityType.FindProperty("Short")));
        Assert.Equal((byte)1, CreateAndUseFactory(entityType.FindProperty("Byte")));
        Assert.Equal((int?)1, CreateAndUseFactory(entityType.FindProperty("NullableInt")));
        Assert.Equal((long?)1, CreateAndUseFactory(entityType.FindProperty("NullableLong")));
        Assert.Equal((short?)1, CreateAndUseFactory(entityType.FindProperty("NullableShort")));
        Assert.Equal((byte?)1, CreateAndUseFactory(entityType.FindProperty("NullableByte")));
        Assert.Equal((uint)1, CreateAndUseFactory(entityType.FindProperty("UInt")));
        Assert.Equal((ulong)1, CreateAndUseFactory(entityType.FindProperty("ULong")));
        Assert.Equal((ushort)1, CreateAndUseFactory(entityType.FindProperty("UShort")));
        Assert.Equal((sbyte)1, CreateAndUseFactory(entityType.FindProperty("SByte")));
        Assert.Equal((uint?)1, CreateAndUseFactory(entityType.FindProperty("NullableUInt")));
        Assert.Equal((ulong?)1, CreateAndUseFactory(entityType.FindProperty("NullableULong")));
        Assert.Equal((ushort?)1, CreateAndUseFactory(entityType.FindProperty("NullableUShort")));
        Assert.Equal((sbyte?)1, CreateAndUseFactory(entityType.FindProperty("NullableSByte")));
    }

    private static object CreateAndUseFactory(IProperty property)
    {
        var model = BuildModel();

        var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        selector.TrySelect(property, property.DeclaringType, out var generator);

        return generator!.Next(null!);
    }

    [ConditionalFact]
    public void Returns_null_for_unsupported_combinations()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.False(selector.TrySelect(entityType.FindProperty("Float")!, entityType, out var valueGenerator));
        Assert.Null(valueGenerator);
    }

    private static IModel BuildModel(bool generateValues = true)
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>().Property(e => e.Custom).HasValueGenerator<CustomValueGenerator>();
        var model = builder.Model;
        var entityType = model.FindEntityType(typeof(AnEntity));

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
        public byte[] Binary { get; set; }
        public float Float { get; set; }
    }

    private class CustomValueGenerator : ValueGenerator<int>
    {
        public override int Next(EntityEntry entry)
            => throw new NotImplementedException();

        public override bool GeneratesTemporaryValues
            => false;
    }
}
