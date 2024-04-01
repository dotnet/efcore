// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class ConvertToProviderTypesTestBase<TFixture> : BuiltInDataTypesTestBase<TFixture>
    where TFixture : BuiltInDataTypesTestBase<TFixture>.BuiltInDataTypesFixtureBase, new()
{
    protected ConvertToProviderTypesTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual void Equals_method_over_enum_works()
    {
        using var context = CreateContext();

        var query = context.Set<BuiltInDataTypes>().Where(t => t.Id == -1 && t.Enum8.Equals(Enum8.SomeValue)).ToList();

        Assert.Empty(query);
    }

    [ConditionalFact]
    public virtual void Object_equals_method_over_enum_works()
    {
        using var context = CreateContext();

        var query = context.Set<BuiltInDataTypes>().Where(t => t.Id == -1 && Equals(t.Enum8, Enum8.SomeValue)).ToList();

        Assert.Empty(query);
    }

    public override Task Object_to_string_conversion()
        => Task.CompletedTask;

    public abstract class ConvertToProviderTypesFixtureBase : BuiltInDataTypesFixtureBase
    {
        protected override string StoreName
            => "ConvertToProviderTypes";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BuiltInDataTypes>(
                b =>
                {
                    b.Property(e => e.PartitionId).HasConversion<long>();
                    b.Property(e => e.TestInt16).HasConversion<long>();
                    b.Property(e => e.TestInt32).HasConversion<long>();
                    b.Property(e => e.TestInt64).HasConversion<long>();
                    b.Property(e => e.TestDouble).HasConversion<decimal>();
                    b.Property(e => e.TestDecimal).HasConversion<byte[]>();
                    b.Property(e => e.TestDateTime).HasConversion<long>();
                    b.Property(e => e.TestDateTimeOffset).HasConversion<long>();
                    b.Property(e => e.TestTimeSpan).HasConversion<long>();
                    b.Property(e => e.TestSingle).HasConversion<decimal>();
                    b.Property(e => e.TestBoolean).HasConversion<string>();
                    b.Property(e => e.TestByte).HasConversion<ushort>();
                    b.Property(e => e.TestUnsignedInt16).HasConversion<ulong>();
                    b.Property(e => e.TestUnsignedInt32).HasConversion<ulong>();
                    b.Property(e => e.TestUnsignedInt64).HasConversion<decimal>();
                    b.Property(e => e.TestCharacter).HasConversion<long>();
                    b.Property(e => e.TestSignedByte).HasConversion<long>();
                    b.Property(e => e.Enum64).HasConversion<long>();
                    b.Property(e => e.Enum32).HasConversion<long>();
                    b.Property(e => e.Enum16).HasConversion<long>();
                    b.Property(e => e.Enum8).HasConversion<string>().HasMaxLength(17);
                    b.Property(e => e.EnumU64).HasConversion<long>();
                    b.Property(e => e.EnumU32).HasConversion<long>();
                    b.Property(e => e.EnumU16).HasConversion<long>();
                    b.Property(e => e.EnumS8).HasConversion<string>().IsUnicode(false);
                });

            modelBuilder.Entity<BuiltInNullableDataTypes>(
                b =>
                {
                    b.Property(e => e.PartitionId).HasConversion<long>();
                    b.Property(e => e.TestNullableInt16).HasConversion<long?>();
                    b.Property(e => e.TestNullableInt32).HasConversion<long>();
                    b.Property(e => e.TestNullableInt64).HasConversion<long?>();
                    b.Property(e => e.TestNullableDouble).HasConversion<decimal?>();
                    b.Property(e => e.TestNullableDecimal).HasConversion<byte[]>();
                    b.Property(e => e.TestNullableDateTime).HasConversion<long?>();
                    b.Property(e => e.TestNullableDateTimeOffset).HasConversion<long>();
                    b.Property(e => e.TestNullableTimeSpan).HasConversion<long?>();
                    b.Property(e => e.TestNullableSingle).HasConversion<decimal>();
                    b.Property(e => e.TestNullableBoolean).HasConversion<string>();
                    b.Property(e => e.TestNullableByte).HasConversion<ushort?>();
                    b.Property(e => e.TestNullableUnsignedInt16).HasConversion<ulong>();
                    b.Property(e => e.TestNullableUnsignedInt32).HasConversion<ulong?>();
                    b.Property(e => e.TestNullableUnsignedInt64).HasConversion<decimal>();
                    b.Property(e => e.TestNullableCharacter).HasConversion<long>();
                    b.Property(e => e.TestNullableSignedByte).HasConversion<long?>();
                    b.Property(e => e.Enum64).HasConversion<long>();
                    b.Property(e => e.Enum32).HasConversion<long?>();
                    b.Property(e => e.Enum16).HasConversion<long>();
                    b.Property(e => e.Enum8).HasConversion<string>();
                    b.Property(e => e.EnumU64).HasConversion<long>();
                    b.Property(e => e.EnumU32).HasConversion<long>();
                    b.Property(e => e.EnumU16).HasConversion<long?>();
                    b.Property(e => e.EnumS8).HasConversion<string>();
                });

            modelBuilder.Entity<BuiltInDataTypesShadow>(
                b =>
                {
                    b.Property(nameof(BuiltInDataTypes.PartitionId)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestInt16)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestInt32)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestInt64)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestDouble)).HasConversion<decimal>();
                    b.Property(nameof(BuiltInDataTypes.TestDecimal)).HasConversion<byte[]>();
                    b.Property(nameof(BuiltInDataTypes.TestDateTime)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestDateTimeOffset)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestTimeSpan)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestSingle)).HasConversion<decimal>();
                    b.Property(nameof(BuiltInDataTypes.TestBoolean)).HasConversion<string>();
                    b.Property(nameof(BuiltInDataTypes.TestByte)).HasConversion<ushort>();
                    b.Property(nameof(BuiltInDataTypes.TestUnsignedInt16)).HasConversion<ulong>();
                    b.Property(nameof(BuiltInDataTypes.TestUnsignedInt32)).HasConversion<ulong>();
                    b.Property(nameof(BuiltInDataTypes.TestUnsignedInt64)).HasConversion<decimal>();
                    b.Property(nameof(BuiltInDataTypes.TestCharacter)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.TestSignedByte)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.Enum64)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.Enum32)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.Enum16)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.Enum8)).HasConversion<string>();
                    b.Property(nameof(BuiltInDataTypes.EnumU64)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.EnumU32)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.EnumU16)).HasConversion<long>();
                    b.Property(nameof(BuiltInDataTypes.EnumS8)).HasConversion<string>();
                });

            modelBuilder.Entity<BuiltInNullableDataTypesShadow>(
                b =>
                {
                    b.Property(nameof(BuiltInNullableDataTypes.PartitionId)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt16)).HasConversion<long?>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt32)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt64)).HasConversion<long?>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableDouble)).HasConversion<decimal?>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableDecimal)).HasConversion<byte[]>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableDateTime)).HasConversion<long?>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)).HasConversion<long?>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableSingle)).HasConversion<decimal>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableBoolean)).HasConversion<string>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableByte)).HasConversion<ushort?>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)).HasConversion<ulong>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)).HasConversion<ulong?>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)).HasConversion<decimal>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableCharacter)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)).HasConversion<long?>();
                    b.Property(nameof(BuiltInNullableDataTypes.Enum64)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.Enum32)).HasConversion<long?>();
                    b.Property(nameof(BuiltInNullableDataTypes.Enum16)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.Enum8)).HasConversion<string>();
                    b.Property(nameof(BuiltInNullableDataTypes.EnumU64)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.EnumU32)).HasConversion<long>();
                    b.Property(nameof(BuiltInNullableDataTypes.EnumU16)).HasConversion<long?>();
                    b.Property(nameof(BuiltInNullableDataTypes.EnumS8)).HasConversion<string>();
                });

            modelBuilder.Entity<BinaryKeyDataType>(
                b => b.Property(e => e.Id).HasConversion<string>());

            modelBuilder.Entity<StringKeyDataType>(
                b => b.Property(e => e.Id).HasConversion<byte[]>());

            modelBuilder.Entity<MaxLengthDataTypes>(
                b =>
                {
                    b.Property(e => e.String3).HasConversion<byte[]>();
                    b.Property(e => e.String9000).HasConversion<byte[]>();
                    b.Property(e => e.StringUnbounded).HasConversion<byte[]>();
                    b.Property(e => e.ByteArray5).HasConversion<string>().HasMaxLength(8);
                    b.Property(e => e.ByteArray9000).HasConversion<string>().HasMaxLength(LongStringLength * 2);
                });

            modelBuilder.Entity<AnimalIdentification>(
                b =>
                {
                    b.Property(e => e.Method).HasConversion<string>().HasMaxLength(6);
                });
        }
    }
}
