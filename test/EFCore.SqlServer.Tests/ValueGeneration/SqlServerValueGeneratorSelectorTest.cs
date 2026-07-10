// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class SqlServerValueGeneratorSelectorTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Returns_built_in_generators_for_types_setup_for_value_generation(bool useTry)
    {
        AssertGenerator<TemporaryIntValueGenerator>("Id", useTry: useTry);
        AssertGenerator<CustomValueGenerator>("Custom", useTry: useTry);
        AssertGenerator<TemporaryLongValueGenerator>("Long", useTry: useTry);
        AssertGenerator<TemporaryShortValueGenerator>("Short", useTry: useTry);
        AssertGenerator<TemporaryByteValueGenerator>("Byte", useTry: useTry);
        AssertGenerator<TemporaryIntValueGenerator>("NullableInt", useTry: useTry);
        AssertGenerator<TemporaryLongValueGenerator>("NullableLong", useTry: useTry);
        AssertGenerator<TemporaryShortValueGenerator>("NullableShort", useTry: useTry);
        AssertGenerator<TemporaryByteValueGenerator>("NullableByte", useTry: useTry);
        AssertGenerator<TemporaryDecimalValueGenerator>("Decimal", useTry: useTry);
        AssertGenerator<StringValueGenerator>("String", useTry: useTry);
        AssertGenerator<SequentialGuidValueGenerator>("Guid", useTry: useTry);
        AssertGenerator<BinaryValueGenerator>("Binary", useTry: useTry);
    }

    private void AssertGenerator<TExpected>(string propertyName, bool useHiLo = false, bool useKeySequence = false, bool useTry = true)
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.Property(e => e.Custom).HasValueGenerator<CustomValueGenerator>();
                b.Property(propertyName).ValueGeneratedOnAdd();
                b.HasKey(propertyName);
            });

        if (useHiLo)
        {
            builder.UseHiLo();
            Assert.NotNull(builder.Model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        if (useKeySequence)
        {
            builder.UseKeySequences();
        }

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        var property = entityType.FindProperty(propertyName)!;
        var generator = CreateValueGenerator(selector, property, useTry);

        Assert.IsType<TExpected>(generator);
    }

    private static ValueGenerator CreateValueGenerator(IValueGeneratorSelector selector, IProperty property, bool useTry)
    {
        ValueGenerator generator;
        if (useTry)
        {
            selector.TrySelect(property, property.DeclaringType, out generator);
        }
        else
        {
#pragma warning disable CS0618 // Type or member is obsolete
            generator = selector.Select(property, property.DeclaringType);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        return generator;
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Returns_temp_guid_generator_when_default_sql_set(bool useTry)
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.Property(e => e.Guid).HasDefaultValueSql("newid()");
                b.HasKey(e => e.Guid);
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        var property = entityType.FindProperty("Guid")!;
        var generator = CreateValueGenerator(selector, property, useTry);
        Assert.IsType<TemporaryGuidValueGenerator>(generator);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Returns_temp_string_generator_when_default_sql_set(bool useTry)
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.Property(e => e.String).ValueGeneratedOnAdd().HasDefaultValueSql("Foo");
                b.HasKey(e => e.String);
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        var property = entityType.FindProperty("String")!;
        var generator = CreateValueGenerator(selector, property, useTry);

        Assert.IsType<TemporaryStringValueGenerator>(generator);
        Assert.True(generator.GeneratesTemporaryValues);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Returns_temp_binary_generator_when_default_sql_set(bool useTry)
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.HasKey(e => e.Binary);
                b.Property(e => e.Binary).HasDefaultValueSql("Foo").ValueGeneratedOnAdd();
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        var property = entityType.FindProperty("Binary")!;
        var generator = CreateValueGenerator(selector, property, useTry);

        Assert.IsType<TemporaryBinaryValueGenerator>(generator);
        Assert.True(generator.GeneratesTemporaryValues);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Returns_sequence_value_generators_when_configured_for_model(bool useTry)
    {
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<int>>("Id", useHiLo: true, useTry: useTry);
        AssertGenerator<CustomValueGenerator>("Custom", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<long>>("Long", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<short>>("Short", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<byte>>("Byte", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<int>>("NullableInt", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<long>>("NullableLong", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<short>>("NullableShort", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<byte>>("NullableByte", useHiLo: true, useTry: useTry);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<decimal>>("Decimal", useHiLo: true, useTry: useTry);
        AssertGenerator<StringValueGenerator>("String", useHiLo: true, useTry: useTry);
        AssertGenerator<SequentialGuidValueGenerator>("Guid", useHiLo: true, useTry: useTry);
        AssertGenerator<BinaryValueGenerator>("Binary", useHiLo: true, useTry: useTry);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Returns_built_in_generators_for_types_setup_for_value_generation_even_with_key_sequences(bool useTry)
    {
        AssertGenerator<TemporaryIntValueGenerator>("Id", useKeySequence: true, useTry: useTry);
        AssertGenerator<CustomValueGenerator>("Custom", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryLongValueGenerator>("Long", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryShortValueGenerator>("Short", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryByteValueGenerator>("Byte", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryIntValueGenerator>("NullableInt", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryLongValueGenerator>("NullableLong", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryShortValueGenerator>("NullableShort", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryByteValueGenerator>("NullableByte", useKeySequence: true, useTry: useTry);
        AssertGenerator<TemporaryDecimalValueGenerator>("Decimal", useKeySequence: true, useTry: useTry);
        AssertGenerator<StringValueGenerator>("String", useKeySequence: true, useTry: useTry);
        AssertGenerator<SequentialGuidValueGenerator>("Guid", useKeySequence: true, useTry: useTry);
        AssertGenerator<BinaryValueGenerator>("Binary", useKeySequence: true, useTry: useTry);
    }

    [ConditionalFact]
    public void Throws_for_unsupported_combinations()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.Property(e => e.Random).ValueGeneratedOnAdd();
                b.HasKey(e => e.Random);
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity));

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.Equal(
            CoreStrings.NoValueGenerator("Random", "AnEntity", "Something"),
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [ConditionalFact]
    public void Returns_null_for_unsupported_combinations()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.Property(e => e.Random).ValueGeneratedOnAdd();
                b.HasKey(e => e.Random);
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.False(selector.TrySelect(entityType.FindProperty("Random")!, entityType, out var valueGenerator));
        Assert.Null(valueGenerator);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Returns_generator_configured_on_model_when_property_is_identity(bool useTry)
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<AnEntity>();

        builder
            .UseHiLo()
            .HasSequence<int>(SqlServerModelExtensions.DefaultHiLoSequenceName);

        var model = builder.UseHiLo().FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity))!;

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        var property = entityType.FindProperty("Id")!;
        var generator = CreateValueGenerator(selector, property, useTry);

        Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(generator);
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
        public string String { get; set; }
        public Guid Guid { get; set; }
        public byte[] Binary { get; set; }
        public float Float { get; set; }
        public decimal Decimal { get; set; }

        [NotMapped]
        public Something Random { get; set; }
    }

    private struct Something : IComparable<Something>
    {
        public int Id { get; set; }

        public int CompareTo(Something other)
            => throw new NotImplementedException();
    }

    private class CustomValueGenerator : ValueGenerator<int>
    {
        public override int Next(EntityEntry entry)
            => throw new NotImplementedException();

        public override bool GeneratesTemporaryValues
            => false;
    }
}
