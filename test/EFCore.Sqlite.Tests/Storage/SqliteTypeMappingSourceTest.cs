// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class SqliteTypeMappingSourceTest : RelationalTypeMappingSourceTestBase
{
    [ConditionalTheory]
    [InlineData("INTEGER", typeof(byte), DbType.Byte)]
    [InlineData("INTEGER", typeof(short), DbType.Int16)]
    [InlineData("INTEGER", typeof(int), DbType.Int32)]
    [InlineData("INTEGER", typeof(long), DbType.Int64)]
    [InlineData("INTEGER", typeof(sbyte), DbType.SByte)]
    [InlineData("INTEGER", typeof(ushort), DbType.UInt16)]
    [InlineData("INTEGER", typeof(uint), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ulong), DbType.UInt64)]
    [InlineData("TEXT", typeof(string), null)]
    [InlineData("TEXT", typeof(Guid), DbType.Guid)]
    [InlineData("BLOB", typeof(byte[]), DbType.Binary)]
    [InlineData("TEXT", typeof(DateTime), DbType.DateTime)]
    [InlineData("TEXT", typeof(DateTimeOffset), DbType.DateTimeOffset)]
    [InlineData("TEXT", typeof(TimeSpan), DbType.Time)]
    [InlineData("TEXT", typeof(decimal), DbType.Decimal)]
    [InlineData("REAL", typeof(float), DbType.Single)]
    [InlineData("REAL", typeof(double), DbType.Double)]
    [InlineData("INTEGER", typeof(ByteEnum), DbType.Byte)]
    [InlineData("INTEGER", typeof(ShortEnum), DbType.Int16)]
    [InlineData("INTEGER", typeof(IntEnum), DbType.Int32)]
    [InlineData("INTEGER", typeof(LongEnum), DbType.Int64)]
    [InlineData("INTEGER", typeof(SByteEnum), DbType.SByte)]
    [InlineData("INTEGER", typeof(UShortEnum), DbType.UInt16)]
    [InlineData("INTEGER", typeof(UIntEnum), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ULongEnum), DbType.UInt64)]
    [InlineData("INTEGER", typeof(byte?), DbType.Byte)]
    [InlineData("INTEGER", typeof(short?), DbType.Int16)]
    [InlineData("INTEGER", typeof(int?), DbType.Int32)]
    [InlineData("INTEGER", typeof(long?), DbType.Int64)]
    [InlineData("INTEGER", typeof(sbyte?), DbType.SByte)]
    [InlineData("INTEGER", typeof(ushort?), DbType.UInt16)]
    [InlineData("INTEGER", typeof(uint?), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ulong?), DbType.UInt64)]
    [InlineData("TEXT", typeof(Guid?), DbType.Guid)]
    [InlineData("TEXT", typeof(DateTime?), DbType.DateTime)]
    [InlineData("TEXT", typeof(DateTimeOffset?), DbType.DateTimeOffset)]
    [InlineData("TEXT", typeof(TimeSpan?), DbType.Time)]
    [InlineData("TEXT", typeof(decimal?), DbType.Decimal)]
    [InlineData("REAL", typeof(float?), DbType.Single)]
    [InlineData("REAL", typeof(double?), DbType.Double)]
    [InlineData("INTEGER", typeof(ByteEnum?), DbType.Byte)]
    [InlineData("INTEGER", typeof(ShortEnum?), DbType.Int16)]
    [InlineData("INTEGER", typeof(IntEnum?), DbType.Int32)]
    [InlineData("INTEGER", typeof(LongEnum?), DbType.Int64)]
    [InlineData("INTEGER", typeof(SByteEnum?), DbType.SByte)]
    [InlineData("INTEGER", typeof(UShortEnum?), DbType.UInt16)]
    [InlineData("INTEGER", typeof(UIntEnum?), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ULongEnum?), DbType.UInt64)]
    public void Does_mappings_for_CLR_type(string storeType, Type clrType, DbType? dbType)
    {
        var mapping = GetTypeMapping(clrType);
        Assert.Equal(storeType, mapping.StoreType);
        Assert.Equal(Nullable.GetUnderlyingType(clrType) ?? clrType, mapping.ClrType);
        Assert.Equal(dbType, mapping.DbType);
        Assert.Null(mapping.Size);
        Assert.False(mapping.IsUnicode);
        Assert.False(mapping.IsFixedLength);
    }

    [ConditionalTheory]
    [InlineData("INTEGER", typeof(long), DbType.Int64)]
    [InlineData("INT", typeof(long), DbType.Int64)]
    [InlineData("LINT", typeof(long), DbType.Int64)]
    [InlineData("MINTED", typeof(long), DbType.Int64)]
    [InlineData("TEXT", typeof(string), null)]
    [InlineData("PRETEXT", typeof(string), null)]
    [InlineData("TEXTURAL", typeof(string), null)]
    [InlineData("CONTEXTUALIZE", typeof(string), null)]
    [InlineData("CHAR", typeof(string), null)]
    [InlineData("CHARACTER", typeof(string), null)]
    [InlineData("CHARLES", typeof(string), null)]
    [InlineData("RECHAR", typeof(string), null)]
    [InlineData("DISCHARGE", typeof(string), null)]
    [InlineData("CLOB", typeof(string), null)]
    [InlineData("CLOBBER", typeof(string), null)]
    [InlineData("RECLOB", typeof(string), null)]
    [InlineData("RECLOBBERED", typeof(string), null)]
    [InlineData("BLOB", typeof(byte[]), DbType.Binary)]
    [InlineData("BLOBBED", typeof(byte[]), DbType.Binary)]
    [InlineData("REBLOB", typeof(byte[]), DbType.Binary)]
    [InlineData("REBLOBBING", typeof(byte[]), DbType.Binary)]
    [InlineData("BIN", typeof(byte[]), DbType.Binary)]
    [InlineData("BINARY", typeof(byte[]), DbType.Binary)]
    [InlineData("BINGO", typeof(byte[]), DbType.Binary)]
    [InlineData("MYOGLOBIN", typeof(byte[]), DbType.Binary)]
    [InlineData("COMBINATORIAL", typeof(byte[]), DbType.Binary)]
    [InlineData("REAL", typeof(double), DbType.Double)]
    [InlineData("REALIZATION", typeof(double), DbType.Double)]
    [InlineData("CORPOREAL", typeof(double), DbType.Double)]
    [InlineData("UNREALISTIC", typeof(double), DbType.Double)]
    [InlineData("FLOA", typeof(double), DbType.Double)]
    [InlineData("FLOAT", typeof(double), DbType.Double)]
    [InlineData("FLOATATION", typeof(double), DbType.Double)]
    [InlineData("REFLOA", typeof(double), DbType.Double)]
    [InlineData("OFFLOADED", typeof(double), DbType.Double)]
    [InlineData("DOUB", typeof(double), DbType.Double)]
    [InlineData("DOUBLE", typeof(double), DbType.Double)]
    [InlineData("DOUBLET", typeof(double), DbType.Double)]
    [InlineData("REDOUB", typeof(double), DbType.Double)]
    [InlineData("REDOUBLES", typeof(double), DbType.Double)]
    [InlineData("OBJECT", typeof(byte[]), DbType.Binary)]
    [InlineData("DISCOMBOBULATED", typeof(byte[]), DbType.Binary)]
    public void Does_mappings_for_store_type(string storeType, Type clrType, DbType? dbType)
    {
        foreach (var type in new[] { storeType, storeType.ToLower(), storeType.Substring(0, 1) + storeType.Substring(1).ToLower() })
        {
            var mapping = CreateRelationalTypeMappingSource(CreateModel()).FindMapping(type)!;
            Assert.Equal(storeType.ToLower(), mapping.StoreType.ToLower());
            Assert.Equal(Nullable.GetUnderlyingType(clrType) ?? clrType, mapping.ClrType);
            Assert.Equal(dbType, mapping.DbType);
            Assert.Null(mapping.Size);
            Assert.False(mapping.IsUnicode);
            Assert.False(mapping.IsFixedLength);
        }
    }

    [ConditionalTheory]
    [InlineData("INTEGER", typeof(byte), DbType.Byte)]
    [InlineData("MINTED", typeof(byte), DbType.Byte)]
    [InlineData("RUBBISH", typeof(byte), DbType.Byte)]
    [InlineData("INTEGER", typeof(short), DbType.Int16)]
    [InlineData("MINTED", typeof(short), DbType.Int16)]
    [InlineData("RUBBISH", typeof(short), DbType.Int16)]
    [InlineData("INTEGER", typeof(int), DbType.Int32)]
    [InlineData("MINTED", typeof(int), DbType.Int32)]
    [InlineData("RUBBISH", typeof(int), DbType.Int32)]
    [InlineData("INTEGER", typeof(long), DbType.Int64)]
    [InlineData("MINTED", typeof(long), DbType.Int64)]
    [InlineData("RUBBISH", typeof(long), DbType.Int64)]
    [InlineData("INTEGER", typeof(sbyte), DbType.SByte)]
    [InlineData("MINTED", typeof(sbyte), DbType.SByte)]
    [InlineData("RUBBISH", typeof(sbyte), DbType.SByte)]
    [InlineData("INTEGER", typeof(ushort), DbType.UInt16)]
    [InlineData("MINTED", typeof(ushort), DbType.UInt16)]
    [InlineData("RUBBISH", typeof(ushort), DbType.UInt16)]
    [InlineData("INTEGER", typeof(uint), DbType.UInt32)]
    [InlineData("MINTED", typeof(uint), DbType.UInt32)]
    [InlineData("RUBBISH", typeof(uint), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ulong), DbType.UInt64)]
    [InlineData("MINTED", typeof(ulong), DbType.UInt64)]
    [InlineData("RUBBISH", typeof(ulong), DbType.UInt64)]
    [InlineData("TEXT", typeof(string), null)]
    [InlineData("CONTEXTUALIZE", typeof(string), null)]
    [InlineData("RUBBISH", typeof(string), null)]
    [InlineData("TEXT", typeof(Guid), DbType.Guid)]
    [InlineData("CONTEXTUALIZE", typeof(Guid), DbType.Guid)]
    [InlineData("RUBBISH", typeof(Guid), DbType.Guid)]
    [InlineData("BLOB", typeof(byte[]), DbType.Binary)]
    [InlineData("BLOBBED", typeof(byte[]), DbType.Binary)]
    [InlineData("RUBBISH", typeof(byte[]), DbType.Binary)]
    [InlineData("TEXT", typeof(DateTime), DbType.DateTime)]
    [InlineData("CONTEXTUALIZE", typeof(DateTime), DbType.DateTime)]
    [InlineData("RUBBISH", typeof(DateTime), DbType.DateTime)]
    [InlineData("TEXT", typeof(DateTimeOffset), DbType.DateTimeOffset)]
    [InlineData("CONTEXTUALIZE", typeof(DateTimeOffset), DbType.DateTimeOffset)]
    [InlineData("RUBBISH", typeof(DateTimeOffset), DbType.DateTimeOffset)]
    [InlineData("TEXT", typeof(TimeSpan), DbType.Time)]
    [InlineData("CONTEXTUALIZE", typeof(TimeSpan), DbType.Time)]
    [InlineData("RUBBISH", typeof(TimeSpan), DbType.Time)]
    [InlineData("TEXT", typeof(decimal), DbType.Decimal)]
    [InlineData("CONTEXTUALIZE", typeof(decimal), DbType.Decimal)]
    [InlineData("RUBBISH", typeof(decimal), DbType.Decimal)]
    [InlineData("REAL", typeof(float), DbType.Single)]
    [InlineData("UNREALISTIC", typeof(float), DbType.Single)]
    [InlineData("RUBBISH", typeof(float), DbType.Single)]
    [InlineData("REAL", typeof(double), DbType.Double)]
    [InlineData("UNREALISTIC", typeof(double), DbType.Double)]
    [InlineData("RUBBISH", typeof(double), DbType.Double)]
    [InlineData("INTEGER", typeof(ByteEnum), DbType.Byte)]
    [InlineData("MINTED", typeof(ByteEnum), DbType.Byte)]
    [InlineData("RUBBISH", typeof(ByteEnum), DbType.Byte)]
    [InlineData("INTEGER", typeof(ShortEnum), DbType.Int16)]
    [InlineData("MINTED", typeof(ShortEnum), DbType.Int16)]
    [InlineData("RUBBISH", typeof(ShortEnum), DbType.Int16)]
    [InlineData("INTEGER", typeof(IntEnum), DbType.Int32)]
    [InlineData("MINTED", typeof(IntEnum), DbType.Int32)]
    [InlineData("RUBBISH", typeof(IntEnum), DbType.Int32)]
    [InlineData("INTEGER", typeof(LongEnum), DbType.Int64)]
    [InlineData("MINTED", typeof(LongEnum), DbType.Int64)]
    [InlineData("RUBBISH", typeof(LongEnum), DbType.Int64)]
    [InlineData("INTEGER", typeof(SByteEnum), DbType.SByte)]
    [InlineData("MINTED", typeof(SByteEnum), DbType.SByte)]
    [InlineData("RUBBISH", typeof(SByteEnum), DbType.SByte)]
    [InlineData("INTEGER", typeof(UShortEnum), DbType.UInt16)]
    [InlineData("MINTED", typeof(UShortEnum), DbType.UInt16)]
    [InlineData("RUBBISH", typeof(UShortEnum), DbType.UInt16)]
    [InlineData("INTEGER", typeof(UIntEnum), DbType.UInt32)]
    [InlineData("MINTED", typeof(UIntEnum), DbType.UInt32)]
    [InlineData("RUBBISH", typeof(UIntEnum), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ULongEnum), DbType.UInt64)]
    [InlineData("MINTED", typeof(ULongEnum), DbType.UInt64)]
    [InlineData("RUBBISH", typeof(ULongEnum), DbType.UInt64)]
    [InlineData("INTEGER", typeof(byte?), DbType.Byte)]
    [InlineData("MINTED", typeof(byte?), DbType.Byte)]
    [InlineData("RUBBISH", typeof(byte?), DbType.Byte)]
    [InlineData("INTEGER", typeof(short?), DbType.Int16)]
    [InlineData("MINTED", typeof(short?), DbType.Int16)]
    [InlineData("RUBBISH", typeof(short?), DbType.Int16)]
    [InlineData("INTEGER", typeof(int?), DbType.Int32)]
    [InlineData("MINTED", typeof(int?), DbType.Int32)]
    [InlineData("RUBBISH", typeof(int?), DbType.Int32)]
    [InlineData("INTEGER", typeof(long?), DbType.Int64)]
    [InlineData("MINTED", typeof(long?), DbType.Int64)]
    [InlineData("RUBBISH", typeof(long?), DbType.Int64)]
    [InlineData("INTEGER", typeof(sbyte?), DbType.SByte)]
    [InlineData("MINTED", typeof(sbyte?), DbType.SByte)]
    [InlineData("RUBBISH", typeof(sbyte?), DbType.SByte)]
    [InlineData("INTEGER", typeof(ushort?), DbType.UInt16)]
    [InlineData("MINTED", typeof(ushort?), DbType.UInt16)]
    [InlineData("RUBBISH", typeof(ushort?), DbType.UInt16)]
    [InlineData("INTEGER", typeof(uint?), DbType.UInt32)]
    [InlineData("MINTED", typeof(uint?), DbType.UInt32)]
    [InlineData("RUBBISH", typeof(uint?), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ulong?), DbType.UInt64)]
    [InlineData("MINTED", typeof(ulong?), DbType.UInt64)]
    [InlineData("RUBBISH", typeof(ulong?), DbType.UInt64)]
    [InlineData("TEXT", typeof(Guid?), DbType.Guid)]
    [InlineData("CONTEXTUALIZE", typeof(Guid?), DbType.Guid)]
    [InlineData("RUBBISH", typeof(Guid?), DbType.Guid)]
    [InlineData("TEXT", typeof(DateTime?), DbType.DateTime)]
    [InlineData("CONTEXTUALIZE", typeof(DateTime?), DbType.DateTime)]
    [InlineData("RUBBISH", typeof(DateTime?), DbType.DateTime)]
    [InlineData("TEXT", typeof(DateTimeOffset?), DbType.DateTimeOffset)]
    [InlineData("CONTEXTUALIZE", typeof(DateTimeOffset?), DbType.DateTimeOffset)]
    [InlineData("RUBBISH", typeof(DateTimeOffset?), DbType.DateTimeOffset)]
    [InlineData("TEXT", typeof(TimeSpan?), DbType.Time)]
    [InlineData("CONTEXTUALIZE", typeof(TimeSpan?), DbType.Time)]
    [InlineData("RUBBISH", typeof(TimeSpan?), DbType.Time)]
    [InlineData("TEXT", typeof(decimal?), DbType.Decimal)]
    [InlineData("CONTEXTUALIZE", typeof(decimal?), DbType.Decimal)]
    [InlineData("RUBBISH", typeof(decimal?), DbType.Decimal)]
    [InlineData("REAL", typeof(float?), DbType.Single)]
    [InlineData("UNREALISTIC", typeof(float?), DbType.Single)]
    [InlineData("RUBBISH", typeof(float?), DbType.Single)]
    [InlineData("REAL", typeof(double?), DbType.Double)]
    [InlineData("UNREALISTIC", typeof(double?), DbType.Double)]
    [InlineData("RUBBISH", typeof(double?), DbType.Double)]
    [InlineData("INTEGER", typeof(ByteEnum?), DbType.Byte)]
    [InlineData("MINTED", typeof(ByteEnum?), DbType.Byte)]
    [InlineData("RUBBISH", typeof(ByteEnum?), DbType.Byte)]
    [InlineData("INTEGER", typeof(ShortEnum?), DbType.Int16)]
    [InlineData("MINTED", typeof(ShortEnum?), DbType.Int16)]
    [InlineData("RUBBISH", typeof(ShortEnum?), DbType.Int16)]
    [InlineData("INTEGER", typeof(IntEnum?), DbType.Int32)]
    [InlineData("MINTED", typeof(IntEnum?), DbType.Int32)]
    [InlineData("RUBBISH", typeof(IntEnum?), DbType.Int32)]
    [InlineData("INTEGER", typeof(LongEnum?), DbType.Int64)]
    [InlineData("MINTED", typeof(LongEnum?), DbType.Int64)]
    [InlineData("RUBBISH", typeof(LongEnum?), DbType.Int64)]
    [InlineData("INTEGER", typeof(SByteEnum?), DbType.SByte)]
    [InlineData("MINTED", typeof(SByteEnum?), DbType.SByte)]
    [InlineData("RUBBISH", typeof(SByteEnum?), DbType.SByte)]
    [InlineData("INTEGER", typeof(UShortEnum?), DbType.UInt16)]
    [InlineData("MINTED", typeof(UShortEnum?), DbType.UInt16)]
    [InlineData("RUBBISH", typeof(UShortEnum?), DbType.UInt16)]
    [InlineData("INTEGER", typeof(UIntEnum?), DbType.UInt32)]
    [InlineData("MINTED", typeof(UIntEnum?), DbType.UInt32)]
    [InlineData("RUBBISH", typeof(UIntEnum?), DbType.UInt32)]
    [InlineData("INTEGER", typeof(ULongEnum?), DbType.UInt64)]
    [InlineData("MINTED", typeof(ULongEnum?), DbType.UInt64)]
    [InlineData("RUBBISH", typeof(ULongEnum?), DbType.UInt64)]
    public void Does_mappings_for_both_store_and_CLR_type(string storeType, Type clrType, DbType? dbType)
    {
        foreach (var type in new[] { storeType, storeType.ToLower(), storeType.Substring(0, 1) + storeType.Substring(1).ToLower() })
        {
            var mapping = GetTypeMapping(clrType, storeTypeName: type);
            Assert.Equal(storeType.ToLower(), mapping.StoreType.ToLower());
            Assert.Equal(Nullable.GetUnderlyingType(clrType) ?? clrType, mapping.ClrType);
            Assert.Equal(dbType, mapping.DbType);
            Assert.Null(mapping.Size);
            Assert.False(mapping.IsUnicode);
            Assert.False(mapping.IsFixedLength);
        }
    }

    [ConditionalFact]
    public void Does_default_mappings_for_values()
    {
        var model = CreateModel();
        Assert.Equal("TEXT", CreateRelationalTypeMappingSource(model).GetMappingForValue("Cheese").StoreType);
        Assert.Equal("BLOB", CreateRelationalTypeMappingSource(model).GetMappingForValue(new byte[1]).StoreType);
        Assert.Equal("TEXT", CreateRelationalTypeMappingSource(model).GetMappingForValue(new DateTime()).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue(1).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue(1L).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue((byte)1).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue((short)1).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue((uint)1).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue((ulong)1).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue((sbyte)1).StoreType);
        Assert.Equal("INTEGER", CreateRelationalTypeMappingSource(model).GetMappingForValue((ushort)1).StoreType);
        Assert.Equal("TEXT", CreateRelationalTypeMappingSource(model).GetMappingForValue(1.0m).StoreType);
        Assert.Equal("REAL", CreateRelationalTypeMappingSource(model).GetMappingForValue(1.0).StoreType);
        Assert.Equal("REAL", CreateRelationalTypeMappingSource(model).GetMappingForValue(1.0f).StoreType);
    }

    [ConditionalFact]
    public void Does_default_mappings_for_null_values()
    {
        var model = CreateModel();
        Assert.Equal("NULL", CreateRelationalTypeMappingSource(model).GetMappingForValue(null).StoreType);
        Assert.Equal("NULL", CreateRelationalTypeMappingSource(model).GetMappingForValue(DBNull.Value).StoreType);
    }

    [ConditionalFact]
    public void Throws_for_unrecognized_property_types()
    {
        var property = ((IMutableModel)new Model()).AddEntityType("Entity1")
            .AddProperty("Strange", typeof(object));
        var ex = Assert.Throws<InvalidOperationException>(
            () => CreateRelationalTypeMappingSource(CreateModel()).GetMapping((IProperty)property));
        Assert.Equal(
            RelationalStrings.UnsupportedPropertyType("Entity1 (Dictionary<string, object>)", "Strange", "object"), ex.Message);

        Assert.Equal(
            RelationalStrings.UnsupportedType("object"),
            Assert.Throws<InvalidOperationException>(() => CreateRelationalTypeMappingSource(CreateModel()).GetMapping(typeof(object)))
                .Message);
    }

    [ConditionalFact]
    public void Plugins_can_override_builtin_mappings()
    {
        var typeMappingSource = new SqliteTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>() with
            {
                Plugins = new[] { new FakeTypeMappingSourcePlugin() }
            });

        Assert.Equal("String", typeMappingSource.GetMapping("datetime2").ClrType.Name);
    }

    private class FakeTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => new StringTypeMapping("datetime2", null);
    }

    protected override IRelationalTypeMappingSource CreateRelationalTypeMappingSource(IModel model)
    {
        var typeMappingSource = new SqliteTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        model.ModelDependencies = new RuntimeModelDependencies(typeMappingSource, null!, null!);

        return typeMappingSource;
    }

    private enum LongEnum : long;

    private enum IntEnum;

    private enum ShortEnum : short;

    private enum ByteEnum : byte;

    protected enum ULongEnum : ulong;

    protected enum UIntEnum : uint;

    protected enum UShortEnum : ushort;

    protected enum SByteEnum : sbyte;

    protected override ModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configureConventions = null)
        => SqliteTestHelpers.Instance.CreateConventionBuilder(configureConventions: configureConventions);
}
