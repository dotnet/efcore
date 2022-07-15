// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore;

public class SqlServerValueGeneratorSelectorTest
{
    [ConditionalFact]
    public void Returns_built_in_generators_for_types_setup_for_value_generation()
    {
        AssertGenerator<TemporaryIntValueGenerator>("Id");
        AssertGenerator<CustomValueGenerator>("Custom");
        AssertGenerator<TemporaryLongValueGenerator>("Long");
        AssertGenerator<TemporaryShortValueGenerator>("Short");
        AssertGenerator<TemporaryByteValueGenerator>("Byte");
        AssertGenerator<TemporaryIntValueGenerator>("NullableInt");
        AssertGenerator<TemporaryLongValueGenerator>("NullableLong");
        AssertGenerator<TemporaryShortValueGenerator>("NullableShort");
        AssertGenerator<TemporaryByteValueGenerator>("NullableByte");
        AssertGenerator<TemporaryDecimalValueGenerator>("Decimal");
        AssertGenerator<StringValueGenerator>("String");
        AssertGenerator<SequentialGuidValueGenerator>("Guid");
        AssertGenerator<BinaryValueGenerator>("Binary");
    }

    private void AssertGenerator<TExpected>(string propertyName, bool useHiLo = false, bool useKeySequence = false)
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
            builder.UseKeySequence();
            Assert.NotNull(builder.Model.FindSequence(SqlServerModelExtensions.DefaultKeySequenceName));
        }

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity));

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.IsType<TExpected>(selector.Select(entityType.FindProperty(propertyName), entityType));
    }

    [ConditionalFact]
    public void Returns_temp_guid_generator_when_default_sql_set()
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.Property(e => e.Guid).HasDefaultValueSql("newid()");
                b.HasKey(e => e.Guid);
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity));

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.IsType<TemporaryGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
    }

    [ConditionalFact]
    public void Returns_temp_string_generator_when_default_sql_set()
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.Property(e => e.String).ValueGeneratedOnAdd().HasDefaultValueSql("Foo");
                b.HasKey(e => e.String);
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity));

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        var generator = selector.Select(entityType.FindProperty("String"), entityType);
        Assert.IsType<TemporaryStringValueGenerator>(generator);
        Assert.True(generator.GeneratesTemporaryValues);
    }

    [ConditionalFact]
    public void Returns_temp_binary_generator_when_default_sql_set()
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        builder.Entity<AnEntity>(
            b =>
            {
                b.HasKey(e => e.Binary);
                b.Property(e => e.Binary).HasDefaultValueSql("Foo").ValueGeneratedOnAdd();
            });
        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity));

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        var generator = selector.Select(entityType.FindProperty("Binary"), entityType);
        Assert.IsType<TemporaryBinaryValueGenerator>(generator);
        Assert.True(generator.GeneratesTemporaryValues);
    }

    [ConditionalFact]
    public void Returns_sequence_value_generators_when_configured_for_model()
    {
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<int>>("Id", useHiLo: true);
        AssertGenerator<CustomValueGenerator>("Custom", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<long>>("Long", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<short>>("Short", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<byte>>("Byte", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<int>>("NullableInt", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<long>>("NullableLong", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<short>>("NullableShort", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<byte>>("NullableByte", useHiLo: true);
        AssertGenerator<SqlServerSequenceHiLoValueGenerator<decimal>>("Decimal", useHiLo: true);
        AssertGenerator<StringValueGenerator>("String", useHiLo: true);
        AssertGenerator<SequentialGuidValueGenerator>("Guid", useHiLo: true);
        AssertGenerator<BinaryValueGenerator>("Binary", useHiLo: true);
    }

    [ConditionalFact]
    public void Returns_built_in_generators_for_types_setup_for_value_generation_even_with_key_sequences()
    {
        AssertGenerator<TemporaryIntValueGenerator>("Id", useKeySequence: true);
        AssertGenerator<CustomValueGenerator>("Custom", useKeySequence: true);
        AssertGenerator<TemporaryLongValueGenerator>("Long", useKeySequence: true);
        AssertGenerator<TemporaryShortValueGenerator>("Short", useKeySequence: true);
        AssertGenerator<TemporaryByteValueGenerator>("Byte", useKeySequence: true);
        AssertGenerator<TemporaryIntValueGenerator>("NullableInt", useKeySequence: true);
        AssertGenerator<TemporaryLongValueGenerator>("NullableLong", useKeySequence: true);
        AssertGenerator<TemporaryShortValueGenerator>("NullableShort", useKeySequence: true);
        AssertGenerator<TemporaryByteValueGenerator>("NullableByte", useKeySequence: true);
        AssertGenerator<TemporaryDecimalValueGenerator>("Decimal", useKeySequence: true);
        AssertGenerator<StringValueGenerator>("String", useKeySequence: true);
        AssertGenerator<SequentialGuidValueGenerator>("Guid", useKeySequence: true);
        AssertGenerator<BinaryValueGenerator>("Binary", useKeySequence: true);
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

        var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.Equal(
            CoreStrings.NoValueGenerator("Random", "AnEntity", "Something"),
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
    }

    [ConditionalFact]
    public void Returns_generator_configured_on_model_when_property_is_identity()
    {
        var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<AnEntity>();

        builder
            .UseHiLo()
            .HasSequence<int>(SqlServerModelExtensions.DefaultHiLoSequenceName);

        var model = builder.UseHiLo().FinalizeModel();
        var entityType = model.FindEntityType(typeof(AnEntity));

        var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

        Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
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
