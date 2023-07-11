// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class TemporaryNumberValueGeneratorFactoryTest
{
    private static readonly IModel _model = BuildModel();

    public static IModel BuildModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>();
        builder.FinalizeModel();
        return (IModel)builder.Model;
    }

    [ConditionalFact]
    public void Can_create_factories_for_all_integer_types()
    {
        var entityType = _model.FindEntityType(typeof(AnEntity));

        Assert.Equal(int.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("Id")));
        Assert.Equal(long.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("Long")));
        Assert.Equal((short)(short.MinValue + 101), CreateAndUseFactory(entityType.FindProperty("Short")));
        Assert.Equal((byte)255, CreateAndUseFactory(entityType.FindProperty("Byte")));
        Assert.Equal(int.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("NullableInt")));
        Assert.Equal(long.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("NullableLong")));
        Assert.Equal((short)(short.MinValue + 101), CreateAndUseFactory(entityType.FindProperty("NullableShort")));
        Assert.Equal((byte)255, CreateAndUseFactory(entityType.FindProperty("NullableByte")));
        Assert.Equal(unchecked((uint)(int.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("UInt")));
        Assert.Equal(unchecked((ulong)(long.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("ULong")));
        Assert.Equal(unchecked((ushort)(short.MinValue + 101)), CreateAndUseFactory(entityType.FindProperty("UShort")));
        Assert.Equal((sbyte)-127, CreateAndUseFactory(entityType.FindProperty("SByte")));
        Assert.Equal(unchecked((uint)(int.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("NullableUInt")));
        Assert.Equal(unchecked((ulong)(long.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("NullableULong")));
        Assert.Equal(unchecked((ushort)(short.MinValue + 101)), CreateAndUseFactory(entityType.FindProperty("NullableUShort")));
        Assert.Equal((sbyte)-127, CreateAndUseFactory(entityType.FindProperty("NullableSByte")));
    }

    private static object CreateAndUseFactory(IProperty property)
        => new TemporaryNumberValueGeneratorFactory().Create(property, property.DeclaringType).Next(null);

    [ConditionalFact]
    public void Throws_for_non_integer_property()
    {
        var entityType = _model.FindEntityType(typeof(AnEntity));
        var property = entityType.FindProperty("BadCheese");

        Assert.Equal(
            CoreStrings.InvalidValueGeneratorFactoryProperty(nameof(TemporaryNumberValueGeneratorFactory), "BadCheese", "AnEntity"),
            Assert.Throws<ArgumentException>(() => new TemporaryNumberValueGeneratorFactory().Create(property, entityType)).Message);
    }

    private class AnEntity
    {
        public int Id { get; set; }
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
        public string BadCheese { get; set; }
    }
}
