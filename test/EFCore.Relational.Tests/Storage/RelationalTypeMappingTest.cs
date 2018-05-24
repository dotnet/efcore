// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalTypeMappingTest
    {
        protected class FakeValueConverter : ValueConverter<object, object>
        {
            public FakeValueConverter()
                : base(_ => _, _ => _)
            {
            }

            public override Type ModelClrType { get; } = typeof(object);
            public override Type ProviderClrType { get; } = typeof(object);
        }

        protected class FakeValueComparer : ValueComparer<object>
        {
            public FakeValueComparer()
                : base(false)
            {
            }

            public override Type Type { get; } = typeof(object);
        }

        [Theory]
        [InlineData(typeof(BoolTypeMapping), typeof(bool))]
        [InlineData(typeof(ByteTypeMapping), typeof(byte))]
        [InlineData(typeof(CharTypeMapping), typeof(char))]
        [InlineData(typeof(DateTimeOffsetTypeMapping), typeof(DateTimeOffset))]
        [InlineData(typeof(DateTimeTypeMapping), typeof(DateTime))]
        [InlineData(typeof(DecimalTypeMapping), typeof(decimal))]
        [InlineData(typeof(DoubleTypeMapping), typeof(double))]
        [InlineData(typeof(FloatTypeMapping), typeof(float))]
        [InlineData(typeof(GuidTypeMapping), typeof(Guid))]
        [InlineData(typeof(IntTypeMapping), typeof(int))]
        [InlineData(typeof(LongTypeMapping), typeof(long))]
        [InlineData(typeof(SByteTypeMapping), typeof(sbyte))]
        [InlineData(typeof(ShortTypeMapping), typeof(short))]
        [InlineData(typeof(TimeSpanTypeMapping), typeof(TimeSpan))]
        [InlineData(typeof(UIntTypeMapping), typeof(uint))]
        [InlineData(typeof(ULongTypeMapping), typeof(ulong))]
        [InlineData(typeof(UShortTypeMapping), typeof(ushort))]
        public virtual void Create_and_clone_with_converter(Type mappingType, Type clrType)
        {
            var mapping = (RelationalTypeMapping)Activator.CreateInstance(
                mappingType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null,
                new[] { FakeTypeMapping.CreateParameters(clrType) },
                null,
                null);

            var clone = mapping.Clone("<clone>", null);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("<clone>", clone.StoreType);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Null(clone.Size);
            Assert.NotNull(mapping.Converter);
            Assert.Same(mapping.Converter, clone.Converter);
            Assert.Same(mapping.Comparer, clone.Comparer);
            Assert.Same(mapping.KeyComparer, clone.KeyComparer);
            Assert.Same(typeof(object), clone.ClrType);
            Assert.Equal(StoreTypePostfix.PrecisionAndScale, clone.StoreTypePostfix);

            var newConverter = new FakeValueConverter();
            clone = (RelationalTypeMapping)mapping.Clone(newConverter);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("<original>", clone.StoreType);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Null(clone.Size);
            Assert.NotSame(mapping.Converter, clone.Converter);
            Assert.Same(mapping.Comparer, clone.Comparer);
            Assert.Same(mapping.KeyComparer, clone.KeyComparer);
            Assert.Same(typeof(object), clone.ClrType);
            Assert.Equal(StoreTypePostfix.PrecisionAndScale, clone.StoreTypePostfix);
        }

        [Theory]
        [InlineData(typeof(ByteArrayTypeMapping), typeof(byte[]))]
        public virtual void Create_and_clone_sized_mappings_with_converter(Type mappingType, Type clrType)
        {
            var mapping = (RelationalTypeMapping)Activator.CreateInstance(
                mappingType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null,
                new[]
                {
                    FakeTypeMapping.CreateParameters(
                        clrType,
                        size: 33,
                        fixedLength: true,
                        storeTypePostfix: StoreTypePostfix.Size)
                },
                null,
                null);

            var clone = mapping.Clone("<clone>", 66);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("<original>(33)", mapping.StoreType);
            Assert.Equal("<clone>(66)", clone.StoreType);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Equal(33, mapping.Size);
            Assert.Equal(66, clone.Size);
            Assert.NotNull(mapping.Converter);
            Assert.Same(mapping.Converter, clone.Converter);
            Assert.Same(mapping.Comparer, clone.Comparer);
            Assert.Same(mapping.KeyComparer, clone.KeyComparer);
            Assert.Same(typeof(object), clone.ClrType);
            Assert.True(mapping.IsFixedLength);
            Assert.True(clone.IsFixedLength);
            Assert.Equal(StoreTypePostfix.Size, clone.StoreTypePostfix);

            var newConverter = new FakeValueConverter();
            clone = (RelationalTypeMapping)mapping.Clone(newConverter);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("<original>(33)", mapping.StoreType);
            Assert.Equal("<original>(33)", clone.StoreType);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Equal(33, mapping.Size);
            Assert.Equal(33, clone.Size);
            Assert.NotSame(mapping.Converter, clone.Converter);
            Assert.Same(mapping.Comparer, clone.Comparer);
            Assert.Same(mapping.KeyComparer, clone.KeyComparer);
            Assert.Same(typeof(object), clone.ClrType);
            Assert.True(mapping.IsFixedLength);
            Assert.True(clone.IsFixedLength);
            Assert.Equal(StoreTypePostfix.Size, clone.StoreTypePostfix);
        }

        [Theory]
        [InlineData(typeof(StringTypeMapping), typeof(string))]
        public virtual void Create_and_clone_unicode_sized_mappings_with_converter(Type mappingType, Type clrType)
        {
            var mapping = (RelationalTypeMapping)Activator.CreateInstance(
                mappingType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null,
                new[]
                {
                    FakeTypeMapping.CreateParameters(
                        clrType,
                        size: 33,
                        unicide: false,
                        fixedLength: true,
                        storeTypePostfix: StoreTypePostfix.Size)
                },
                null,
                null);

            var clone = mapping.Clone("<clone>", 66);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("<original>(33)", mapping.StoreType);
            Assert.Equal("<clone>(66)", clone.StoreType);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Equal(33, mapping.Size);
            Assert.Equal(66, clone.Size);
            Assert.False(mapping.IsUnicode);
            Assert.False(clone.IsUnicode);
            Assert.NotNull(mapping.Converter);
            Assert.Same(mapping.Converter, clone.Converter);
            Assert.Same(mapping.Comparer, clone.Comparer);
            Assert.Same(mapping.KeyComparer, clone.KeyComparer);
            Assert.Same(typeof(object), clone.ClrType);
            Assert.True(mapping.IsFixedLength);
            Assert.True(clone.IsFixedLength);
            Assert.Equal(StoreTypePostfix.Size, clone.StoreTypePostfix);

            var newConverter = new FakeValueConverter();
            clone = (RelationalTypeMapping)mapping.Clone(newConverter);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("<original>(33)", mapping.StoreType);
            Assert.Equal("<original>(33)", clone.StoreType);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Equal(33, mapping.Size);
            Assert.Equal(33, clone.Size);
            Assert.False(mapping.IsUnicode);
            Assert.False(clone.IsUnicode);
            Assert.NotSame(mapping.Converter, clone.Converter);
            Assert.Same(mapping.Comparer, clone.Comparer);
            Assert.Same(mapping.KeyComparer, clone.KeyComparer);
            Assert.Same(typeof(object), clone.ClrType);
            Assert.True(mapping.IsFixedLength);
            Assert.True(clone.IsFixedLength);
            Assert.Equal(StoreTypePostfix.Size, clone.StoreTypePostfix);
        }

        private class FakeTypeMapping : RelationalTypeMapping
        {
            private FakeTypeMapping(RelationalTypeMappingParameters parameters)
                : base(parameters)
            {
            }

            public FakeTypeMapping()
                : base("storeType", typeof(object))
            {
            }

            public static object CreateParameters(
                Type clrType,
                int? size = null,
                bool unicide = false,
                bool fixedLength = false,
                StoreTypePostfix storeTypePostfix = StoreTypePostfix.PrecisionAndScale)
            {
                return new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        clrType,
                        new FakeValueConverter(),
                        new FakeValueComparer(),
                        new FakeValueComparer()),
                    "<original>",
                    storeTypePostfix,
                    System.Data.DbType.VarNumeric,
                    size: size,
                    unicode: unicide,
                    fixedLength: fixedLength);
            }
        }

        [Fact]
        public void Can_create_simple_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int")
                    .CreateParameter(command, "Name", 17, nullable: false);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DefaultParameterType, parameter.DbType);
                Assert.False(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_simple_nullable_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int")
                    .CreateParameter(command, "Name", 17, nullable: true);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DefaultParameterType, parameter.DbType);
                Assert.True(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_simple_parameter_with_DbType()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int", DbType.Int32)
                    .CreateParameter(command, "Name", 17, nullable: false);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DbType.Int32, parameter.DbType);
                Assert.False(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_simple_nullable_parameter_with_DbType()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int", DbType.Int32)
                    .CreateParameter(command, "Name", 17, nullable: true);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DbType.Int32, parameter.DbType);
                Assert.True(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_required_string_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new StringTypeMapping("nvarchar(23)", DbType.String, unicode: true, size: 23)
                    .CreateParameter(command, "Name", "Value", nullable: false);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal("Value", parameter.Value);
                Assert.Equal(DbType.String, parameter.DbType);
                Assert.False(parameter.IsNullable);
                Assert.Equal(23, parameter.Size);
            }
        }

        [Fact]
        public void Can_create_string_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new StringTypeMapping("nvarchar(23)", DbType.String, unicode: true, size: 23)
                    .CreateParameter(command, "Name", "Value", nullable: true);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal("Value", parameter.Value);
                Assert.Equal(DbType.String, parameter.DbType);
                Assert.True(parameter.IsNullable);
                Assert.Equal(23, parameter.Size);
            }
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_bool_literal_when_true()
        {
            var literal = new BoolTypeMapping("bool").GenerateSqlLiteral(true);
            Assert.Equal("1", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_bool_literal_when_false()
        {
            var literal = new BoolTypeMapping("bool").GenerateSqlLiteral(false);
            Assert.Equal("0", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_string_literal()
        {
            var literal = new StringTypeMapping("string").GenerateSqlLiteral("Text");
            Assert.Equal("'Text'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_char_literal()
        {
            var literal = new CharTypeMapping("char").GenerateSqlLiteral('A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_ByteArray_literal()
        {
            var literal = new ByteArrayTypeMapping("byte[]").GenerateSqlLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("X'DA7A'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = new GuidTypeMapping("guid").GenerateSqlLiteral(value);
            Assert.Equal("'c6f43a9e-91e1-45ef-a320-832ea23b7292'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = new DateTimeTypeMapping("DateTime").GenerateSqlLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = new DateTimeOffsetTypeMapping("DateTimeOffset").GenerateSqlLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_NullableInt_literal_when_null()
        {
#pragma warning disable IDE0034 // Simplify 'default' expression - Causes inference of default(object) due to parameter being object type
            var literal = new IntTypeMapping("int?", DbType.Int32).GenerateSqlLiteral(default(int?));
#pragma warning restore IDE0034 // Simplify 'default' expression
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_NullableInt_literal_when_not_null()
        {
            var literal = new IntTypeMapping("int?", DbType.Int32).GenerateSqlLiteral((int?)123);
            Assert.Equal("123", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Byte_works_for_range_limits()
        {
            var typeMapping = new ByteTypeMapping("byte", DbType.Byte);
            var literal = typeMapping.GenerateSqlLiteral(byte.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(byte.MaxValue);
            Assert.Equal("255", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_SByte_works_for_range_limits()
        {
            var typeMapping = new SByteTypeMapping("sbyte", DbType.SByte);
            var literal = typeMapping.GenerateSqlLiteral(sbyte.MinValue);
            Assert.Equal("-128", literal);

            literal = typeMapping.GenerateSqlLiteral(sbyte.MaxValue);
            Assert.Equal("127", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Short_works_for_range_limits()
        {
            var typeMapping = new ShortTypeMapping("short", DbType.Int16);
            var literal = typeMapping.GenerateSqlLiteral(short.MinValue);
            Assert.Equal("-32768", literal);

            literal = typeMapping.GenerateSqlLiteral(short.MaxValue);
            Assert.Equal("32767", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_UShort_works_for_range_limits()
        {
            var typeMapping = new UShortTypeMapping("ushort", DbType.UInt16);
            var literal = typeMapping.GenerateSqlLiteral(ushort.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(ushort.MaxValue);
            Assert.Equal("65535", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Int_works_for_range_limits()
        {
            var typeMapping = new IntTypeMapping("int", DbType.Int32);
            var literal = typeMapping.GenerateSqlLiteral(int.MinValue);
            Assert.Equal("-2147483648", literal);

            literal = typeMapping.GenerateSqlLiteral(int.MaxValue);
            Assert.Equal("2147483647", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_UInt_works_for_range_limits()
        {
            var typeMapping = new UIntTypeMapping("uint", DbType.UInt32);
            var literal = typeMapping.GenerateSqlLiteral(uint.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(uint.MaxValue);
            Assert.Equal("4294967295", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Long_works_for_range_limits()
        {
            var typeMapping = new LongTypeMapping("long", DbType.Int64);
            var literal = typeMapping.GenerateSqlLiteral(long.MinValue);
            Assert.Equal("-9223372036854775808", literal);

            literal = typeMapping.GenerateSqlLiteral(long.MaxValue);
            Assert.Equal("9223372036854775807", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_ULong_works_for_range_limits()
        {
            var typeMapping = new ULongTypeMapping("ulong", DbType.UInt64);
            var literal = typeMapping.GenerateSqlLiteral(ulong.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(ulong.MaxValue);
            Assert.Equal("18446744073709551615", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Float_works_for_range_limits()
        {
            var typeMapping = new FloatTypeMapping("float", DbType.Single);
            var literal = typeMapping.GenerateSqlLiteral(float.MinValue);
            Assert.Equal("-3.40282347E+38", literal);

            literal = typeMapping.GenerateSqlLiteral(float.MaxValue);
            Assert.Equal("3.40282347E+38", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Float_works_for_special_values()
        {
            var typeMapping = new FloatTypeMapping("float", DbType.Single);
            var literal = typeMapping.GenerateSqlLiteral(float.NaN);
            Assert.Equal("NaN", literal);

            literal = typeMapping.GenerateSqlLiteral(float.PositiveInfinity);
            Assert.Equal("Infinity", literal);

            literal = typeMapping.GenerateSqlLiteral(float.NegativeInfinity);
            Assert.Equal("-Infinity", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Double_works_for_range_limits()
        {
            var typeMapping = new DoubleTypeMapping("double", DbType.Double);
            var literal = typeMapping.GenerateSqlLiteral(double.MinValue);
            Assert.Equal("-1.7976931348623157E+308", literal);

            literal = typeMapping.GenerateSqlLiteral(double.MaxValue);
            Assert.Equal("1.7976931348623157E+308", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Double_works_for_special_values()
        {
            var typeMapping = new DoubleTypeMapping("double", DbType.Double);
            var literal = typeMapping.GenerateSqlLiteral(double.NaN);
            Assert.Equal("NaN", literal);

            literal = typeMapping.GenerateSqlLiteral(double.PositiveInfinity);
            Assert.Equal("Infinity", literal);

            literal = typeMapping.GenerateSqlLiteral(double.NegativeInfinity);
            Assert.Equal("-Infinity", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Decimal_works_for_range_limits()
        {
            var typeMapping = new DecimalTypeMapping("decimal", DbType.Decimal);
            var literal = typeMapping.GenerateSqlLiteral(decimal.MinValue);
            Assert.Equal("-79228162514264337593543950335.0", literal);

            literal = typeMapping.GenerateSqlLiteral(decimal.MaxValue);
            Assert.Equal("79228162514264337593543950335.0", literal);
        }

        [Fact]
        public virtual void Primary_key_type_mapping_is_picked_up_by_FK_without_going_through_store_type()
        {
            using (var context = new FruityContext(ContextOptions))
            {
                Assert.Same(
                    context.Model.FindEntityType(typeof(Banana)).FindProperty("Id").FindMapping(),
                    context.Model.FindEntityType(typeof(Kiwi)).FindProperty("BananaId").FindMapping());
            }
        }

        private class FruityContext : DbContext
        {
            public FruityContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Banana> Bananas { get; set; }
            public DbSet<Kiwi> Kiwi { get; set; }
        }

        [Fact]
        public virtual void Primary_key_type_mapping_can_differ_from_FK()
        {
            using (var context = new MismatchedFruityContext(ContextOptions))
            {
                Assert.Equal(
                    typeof(short),
                    context.Model.FindEntityType(typeof(Banana)).FindProperty("Id").FindMapping().Converter.ProviderClrType);
                Assert.Null(context.Model.FindEntityType(typeof(Kiwi)).FindProperty("Id").FindMapping().Converter);
            }
        }

        private class MismatchedFruityContext : FruityContext
        {
            public MismatchedFruityContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Banana>().Property(e => e.Id).HasConversion<short>();
                modelBuilder.Entity<Kiwi>().Property(e => e.Id).HasConversion<int>();
                modelBuilder.Entity<Kiwi>().HasOne(e => e.Banana).WithMany(e => e.Kiwis).HasForeignKey(e => e.Id);
            }
        }

        private class Banana
        {
            public int Id { get; set; }

            public ICollection<Kiwi> Kiwis { get; set; }
        }

        private class Kiwi
        {
            public int Id { get; set; }

            public int BananaId { get; set; }
            public Banana Banana { get; set; }
        }

        protected abstract DbContextOptions ContextOptions { get; }

        protected abstract DbCommand CreateTestCommand();

        protected abstract DbType DefaultParameterType { get; }
    }
}
