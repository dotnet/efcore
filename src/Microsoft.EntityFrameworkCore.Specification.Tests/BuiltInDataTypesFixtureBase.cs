// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class BuiltInDataTypesFixtureBase : IDisposable
    {
        public abstract DbContext CreateContext();

        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BuiltInDataTypes>();
            modelBuilder.Entity<BuiltInNullableDataTypes>();
            modelBuilder.Entity<BinaryKeyDataType>();
            modelBuilder.Entity<BinaryForeignKeyDataType>();
            modelBuilder.Entity<StringKeyDataType>();
            modelBuilder.Entity<StringForeignKeyDataType>();
            modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<BuiltInNullableDataTypes>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<BinaryForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<StringForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
            MakeRequired<BuiltInDataTypes>(modelBuilder);

            modelBuilder.Entity<MaxLengthDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.ByteArray5).HasMaxLength(5);
                    b.Property(e => e.String3).HasMaxLength(3);
                    b.Property(e => e.ByteArray9000).HasMaxLength(9000);
                    b.Property(e => e.String9000).HasMaxLength(9000);
                });

            modelBuilder.Entity<UnicodeDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.StringAnsi).IsUnicode(false);
                    b.Property(e => e.StringAnsi3).HasMaxLength(3).IsUnicode(false);
                    b.Property(e => e.StringAnsi9000).IsUnicode(false).HasMaxLength(9000);
                    b.Property(e => e.StringUnicode).IsUnicode();
                });
        }

        protected static void MakeRequired<TEntity>(ModelBuilder modelBuilder) where TEntity : class
        {
            var entityType = modelBuilder.Entity<TEntity>().Metadata;

            foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties)
            {
                entityType.GetOrAddProperty(propertyInfo).IsNullable = false;
            }
        }

        public abstract void Dispose();

        public abstract bool SupportsBinaryKeys { get; }

        public abstract DateTime DefaultDateTime { get; }
    }

    public class BuiltInDataTypes
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
        public TimeSpan TestTimeSpan { get; set; }
        public float TestSingle { get; set; }
        public bool TestBoolean { get; set; }
        public byte TestByte { get; set; }
        public ushort TestUnsignedInt16 { get; set; }
        public uint TestUnsignedInt32 { get; set; }
        public ulong TestUnsignedInt64 { get; set; }
        public char TestCharacter { get; set; }
        public sbyte TestSignedByte { get; set; }
        public Enum64 Enum64 { get; set; }
        public Enum32 Enum32 { get; set; }
        public Enum16 Enum16 { get; set; }
        public Enum8 Enum8 { get; set; }
    }

    public enum Enum64 : long
    {
        SomeValue = 1
    }

    public enum Enum32
    {
        SomeValue = 1
    }

    public enum Enum16 : short
    {
        SomeValue = 1
    }

    public enum Enum8 : byte
    {
        SomeValue = 1
    }

    public class MaxLengthDataTypes
    {
        public int Id { get; set; }
        public string String3 { get; set; }
        public byte[] ByteArray5 { get; set; }
        public string String9000 { get; set; }
        public byte[] ByteArray9000 { get; set; }
    }

    public class UnicodeDataTypes
    {
        public int Id { get; set; }
        public string StringDefault { get; set; }
        public string StringAnsi { get; set; }
        public string StringAnsi3 { get; set; }
        public string StringAnsi9000 { get; set; }
        public string StringUnicode { get; set; }
    }

    public class BinaryKeyDataType
    {
        public byte[] Id { get; set; }

        public ICollection<BinaryForeignKeyDataType> Dependents { get; set; }
    }

    public class BinaryForeignKeyDataType
    {
        public int Id { get; set; }
        public byte[] BinaryKeyDataTypeId { get; set; }

        public BinaryKeyDataType Principal { get; set; }
    }

    public class StringKeyDataType
    {
        public string Id { get; set; }

        public ICollection<StringForeignKeyDataType> Dependents { get; set; }
    }

    public class StringForeignKeyDataType
    {
        public int Id { get; set; }
        public string StringKeyDataTypeId { get; set; }

        public StringKeyDataType Principal { get; set; }
    }

    public class BuiltInNullableDataTypes
    {
        public int Id { get; set; }
        public int PartitionId { get; set; }
        public string TestString { get; set; }
        public byte[] TestByteArray { get; set; }
        public short? TestNullableInt16 { get; set; }
        public int? TestNullableInt32 { get; set; }
        public long? TestNullableInt64 { get; set; }
        public double? TestNullableDouble { get; set; }
        public decimal? TestNullableDecimal { get; set; }
        public DateTime? TestNullableDateTime { get; set; }
        public DateTimeOffset? TestNullableDateTimeOffset { get; set; }
        public TimeSpan? TestNullableTimeSpan { get; set; }
        public float? TestNullableSingle { get; set; }
        public bool? TestNullableBoolean { get; set; }
        public byte? TestNullableByte { get; set; }
        public ushort? TestNullableUnsignedInt16 { get; set; }
        public uint? TestNullableUnsignedInt32 { get; set; }
        public ulong? TestNullableUnsignedInt64 { get; set; }
        public char? TestNullableCharacter { get; set; }
        public sbyte? TestNullableSignedByte { get; set; }
        public Enum64? Enum64 { get; set; }
        public Enum32? Enum32 { get; set; }
        public Enum16? Enum16 { get; set; }
        public Enum8? Enum8 { get; set; }
    }
}
