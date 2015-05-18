// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class BuiltInDataTypesFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract DbContext CreateContext(TTestStore testStore);

        public virtual void OnModelCreating(ModelBuilder model)
        {
            model.Entity<BuiltInNonNullableDataTypes>();
            model.Entity<BuiltInNullableDataTypes>();
        }

        public virtual void Cleanup(DbContext context)
        {
            context.Set<BuiltInNonNullableDataTypes>().RemoveRange(context.Set<BuiltInNonNullableDataTypes>());
            context.Set<BuiltInNullableDataTypes>().RemoveRange(context.Set<BuiltInNullableDataTypes>());

            context.SaveChanges();
        }
    }

    public class BuiltInNonNullableDataTypes
    {
        public int Id { get; set; }
        public int PartitionId { get; set; }
        public short TestInt16 { get; set; }
        public int TestInt32 { get; set; }
        public long TestInt64 { get; set; }
        public double TestDouble { get; set; }
        public decimal TestDecimal { get; set; }
        public DateTime TestDateTime { get; set; }
        public DateTimeOffset TestDateTimeOffset { get; set; }
        public float TestSingle { get; set; }
        public bool TestBoolean { get; set; }
        public byte TestByte { get; set; }
        public ushort TestUnsignedInt16 { get; set; }
        public uint TestUnsignedInt32 { get; set; }
        public ulong TestUnsignedInt64 { get; set; }
        public char TestCharacter { get; set; }
        public sbyte TestSignedByte { get; set; }
    }

    public class BuiltInNullableDataTypes
    {
        public int Id { get; set; }
        public int PartitionId { get; set; }
        public string TestString { get; set; }
        public short? TestNullableInt16 { get; set; }
        public int? TestNullableInt32 { get; set; }
        public long? TestNullableInt64 { get; set; }
        public double? TestNullableDouble { get; set; }
        public decimal? TestNullableDecimal { get; set; }
        public DateTime? TestNullableDateTime { get; set; }
        public DateTimeOffset? TestNullableDateTimeOffset { get; set; }
        public float? TestNullableSingle { get; set; }
        public bool? TestNullableBoolean { get; set; }
        public byte? TestNullableByte { get; set; }
        public ushort? TestNullableUnsignedInt16 { get; set; }
        public uint? TestNullableUnsignedInt32 { get; set; }
        public ulong? TestNullableUnsignedInt64 { get; set; }
        public char? TestNullableCharacter { get; set; }
        public sbyte? TestNullableSignedByte { get; set; }
    }
}
