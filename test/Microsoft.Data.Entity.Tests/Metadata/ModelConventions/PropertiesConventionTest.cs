using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class PropertiesConventionTest
    {
        private class EntityWithInvalidProperties
        {
            public static int Static { get; set; }
            public int WriteOnly { set { } }
            public int ReadOnly { get { return 0; } }
            public int this[int index] { get { return 0; } set { } }
        }

        [Fact]
        public void IsValidProperty_returns_false_when_invalid()
        {
            var entityType = new EntityType(typeof(EntityWithInvalidProperties));

            new PropertiesConvention().Apply(entityType);

            Assert.Empty(entityType.Properties);
        }

        private class EntityWithEveryPrimitive
        {
            public bool Boolean { get; set; }
            public byte Byte { get; set; }
            public byte[] ByteArray { get; set; }
            public char Char { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public decimal Decimal { get; set; }
            public double Double { get; set; }
            public Enum1 Enum { get; set; }
            public Guid Guid { get; set; }
            public short Int16 { get; set; }
            public int Int32 { get; set; }
            public long Int64 { get; set; }
            public bool? NullableBoolean { get; set; }
            public byte? NullableByte { get; set; }
            public char? NullableChar { get; set; }
            public DateTime? NullableDateTime { get; set; }
            public DateTimeOffset? NullableDateTimeOffset { get; set; }
            public decimal? NullableDecimal { get; set; }
            public double? NullableDouble { get; set; }
            public Enum1? NullableEnum { get; set; }
            public Guid? NullableGuid { get; set; }
            public short? NullableInt16 { get; set; }
            public int? NullableInt32 { get; set; }
            public long? NullableInt64 { get; set; }
            public sbyte? NullableSByte { get; set; }
            public float? NullableSingle { get; set; }
            public TimeSpan? NullableTimeSpan { get; set; }
            public ushort? NullableUInt16 { get; set; }
            public uint? NullableUInt32 { get; set; }
            public ulong? NullableUInt64 { get; set; }
            public sbyte SByte { get; set; }
            public float Single { get; set; }
            public string String { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public ushort UInt16 { get; set; }
            public uint UInt32 { get; set; }
            public ulong UInt64 { get; set; }
        }

        private enum Enum1
        {
            Default
        }

        [Fact]
        public void IsPrimitiveProperty_returns_true_when_supported_type()
        {
            var entityType = new EntityType(typeof(EntityWithEveryPrimitive));

            new PropertiesConvention().Apply(entityType);

            Assert.Equal(
                typeof(EntityWithEveryPrimitive).GetProperties().Select(p => p.Name),
                entityType.Properties.Select(p => p.Name));
        }

        private class EntityWithNoPrimitives
        {
            public object Object { get; set; }
        }

        [Fact]
        public void IsPrimitiveProperty_returns_false_when_unsupported_type()
        {
            var entityType = new EntityType(typeof(EntityWithNoPrimitives));

            new PropertiesConvention().Apply(entityType);

            Assert.Empty(entityType.Properties);
        }
    }
}
