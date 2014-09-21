// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class DataTypesFixture
    {
        public DbContext CreateContext()
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            return new DbContext(options);
        }

        public IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<AllDataTypes>(b =>
                {
                    b.Key(adt => adt.TestInt32);
                    b.Property(adt => adt.TestInt32);
                    b.Property(adt => adt.TestNullableInt32);
                    b.Property(adt => adt.TestString);
                    b.Property(adt => adt.TestInt64);
                    b.Property(adt => adt.TestNullableInt64);
                    b.Property(adt => adt.TestDouble);
                    b.Property(adt => adt.TestNullableDouble);
                    b.Property(adt => adt.TestDecimal);
                    b.Property(adt => adt.TestNullableDecimal);
                    b.Property(adt => adt.TestDateTime);
                    b.Property(adt => adt.TestNullableDateTime);
                    b.Property(adt => adt.TestDateTimeOffset);
                    b.Property(adt => adt.TestNullableDateTimeOffset);
                    b.Property(adt => adt.TestSingle);
                    b.Property(adt => adt.TestNullableSingle);
                    b.Property(adt => adt.TestBoolean);
                    b.Property(adt => adt.TestNullableBoolean);
                    b.Property(adt => adt.TestByte);
                    b.Property(adt => adt.TestNullableByte);
                    b.Property(adt => adt.TestUnsignedInt32);
                    b.Property(adt => adt.TestNullableUnsignedInt32);
                    b.Property(adt => adt.TestUnsignedInt64);
                    b.Property(adt => adt.TestNullableUnsignedInt64);
                    b.Property(adt => adt.TestInt16);
                    b.Property(adt => adt.TestNullableInt16);
                    b.Property(adt => adt.TestUnsignedInt16);
                    b.Property(adt => adt.TestNullableUnsignedInt16);
                    b.Property(adt => adt.TestCharacter);
                    b.Property(adt => adt.TestNullableCharacter);
                    b.Property(adt => adt.TestSignedByte);
                    b.Property(adt => adt.TestNullableSignedByte);
                });

            return model;
        }
    }

    public class AllDataTypes
    {
        public int TestInt32 { get; set; }
        public int? TestNullableInt32 { get; set; }
        public string TestString { get; set; }
        public long TestInt64 { get; set; }
        public long? TestNullableInt64 { get; set; }
        public double TestDouble { get; set; }
        public double? TestNullableDouble { get; set; }
        public decimal TestDecimal { get; set; }
        public decimal? TestNullableDecimal { get; set; }
        public DateTime TestDateTime { get; set; }
        public DateTime? TestNullableDateTime { get; set; }
        public DateTimeOffset TestDateTimeOffset { get; set; }
        public DateTimeOffset? TestNullableDateTimeOffset { get; set; }
        public float TestSingle { get; set; }
        public float? TestNullableSingle { get; set; }
        public bool TestBoolean { get; set; }
        public bool? TestNullableBoolean { get; set; }
        public byte TestByte { get; set; }
        public byte? TestNullableByte { get; set; }
        public uint TestUnsignedInt32 { get; set; }
        public uint? TestNullableUnsignedInt32 { get; set; }
        public ulong TestUnsignedInt64 { get; set; }
        public ulong? TestNullableUnsignedInt64 { get; set; }
        public short TestInt16 { get; set; }
        public short? TestNullableInt16 { get; set; }
        public ushort TestUnsignedInt16 { get; set; }
        public ushort? TestNullableUnsignedInt16 { get; set; }
        public char TestCharacter { get; set; }
        public char? TestNullableCharacter { get; set; }
        public sbyte TestSignedByte { get; set; }
        public sbyte? TestNullableSignedByte { get; set; }
    }
}
