// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class BuiltInDataTypesFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract DbContext CreateContext(TTestStore testStore);

        public Model CreateModel()
        {
            var modelBuilder = new BasicModelBuilder(new Model());
            OnModelCreating(modelBuilder);
            return modelBuilder.Model;
        }

        // TODO: Use ModelBuilder when Ignore is available
        public virtual void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BuiltInNonNullableDataTypes>(b =>
                {
                    b.Key(dt => dt.Id0);
                    // having 2 non-null properies is needed for Azure Table Storage (and should be supported by all other providers)
                    b.Property(dt => dt.Id0);
                    b.Property(dt => dt.Id1);
                    b.Property(dt => dt.TestInt16);
                    b.Property(dt => dt.TestInt32);
                    b.Property(dt => dt.TestInt64);
                    b.Property(dt => dt.TestDouble);
                    b.Property(dt => dt.TestDecimal);
                    b.Property(dt => dt.TestDateTime);
                    b.Property(dt => dt.TestDateTimeOffset);
                    b.Property(dt => dt.TestSingle);
                    b.Property(dt => dt.TestBoolean);
                    b.Property(dt => dt.TestByte);
                });

            modelBuilder.Entity<BuiltInNullableDataTypes>(b =>
                {
                    b.Key(dt => dt.Id0);
                    // having 2 non-null properies is needed for Azure Table Storage (and should be supported by all other providers)
                    b.Property(dt => dt.Id0);
                    b.Property(dt => dt.Id1);
                    b.Property(dt => dt.TestString);
                    b.Property(dt => dt.TestNullableInt16);
                    b.Property(dt => dt.TestNullableInt32);
                    b.Property(dt => dt.TestNullableInt64);
                    b.Property(dt => dt.TestNullableDouble);
                    b.Property(dt => dt.TestNullableDecimal);
                    b.Property(dt => dt.TestNullableDateTime);
                    b.Property(dt => dt.TestNullableDateTimeOffset);
                    b.Property(dt => dt.TestNullableSingle);
                    b.Property(dt => dt.TestNullableBoolean);
                    b.Property(dt => dt.TestNullableByte);
                });
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
        public int Id0 { get; set; }
        public int Id1 { get; set; }
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
        public int Id0 { get; set; }
        public int Id1 { get; set; }
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
