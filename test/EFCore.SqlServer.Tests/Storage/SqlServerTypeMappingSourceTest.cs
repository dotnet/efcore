// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class SqlServerTypeMappingSourceTest : RelationalTypeMappingSourceTestBase
{
    [ConditionalTheory]
    [InlineData(typeof(int), "int", DbType.Int32)]
    [InlineData(typeof(byte), "tinyint", DbType.Byte)]
    [InlineData(typeof(double), "float", DbType.Double)]
    [InlineData(typeof(bool), "bit", DbType.Boolean)]
    [InlineData(typeof(short), "smallint", DbType.Int16)]
    [InlineData(typeof(long), "bigint", DbType.Int64)]
    [InlineData(typeof(float), "real", DbType.Single)]
    [InlineData(typeof(string), "nvarchar(max)", DbType.String)]
    [InlineData(typeof(byte[]), "varbinary(max)", DbType.Binary)]
    [InlineData(typeof(DateTime), "datetime2", DbType.DateTime2)]
    [InlineData(typeof(DateOnly), "date", DbType.Date)]
    [InlineData(typeof(TimeOnly), "time", DbType.Time)]
    [InlineData(typeof(TimeSpan), "time", DbType.Time)]
    [InlineData(typeof(DateTimeOffset), "datetimeoffset", DbType.DateTimeOffset)]
    [InlineData(typeof(Guid), "uniqueidentifier", DbType.Guid)]
    [InlineData(typeof(IntEnum), "int", DbType.Int32)]
    [InlineData(typeof(ByteEnum), "tinyint", DbType.Byte)]
    [InlineData(typeof(ShortEnum), "smallint", DbType.Int16)]
    [InlineData(typeof(LongEnum), "bigint", DbType.Int64)]
    public void Can_map_by_clr_type(Type clrType, string expectedStoreType, DbType expectedDbType)
    {
        var mapping = GetTypeMapping(clrType);
        Assert.Equal(expectedStoreType, mapping.StoreType);
        Assert.Equal(expectedDbType, mapping.DbType);

        if (clrType.IsValueType)
        {
            mapping = GetTypeMapping(typeof(Nullable<>).MakeGenericType(clrType));

            Assert.Equal(expectedStoreType, mapping.StoreType);
            Assert.Equal(expectedDbType, mapping.DbType);
        }
    }

    [ConditionalFact]
    public void Does_decimal_mapping()
    {
        var typeMapping = GetTypeMapping(typeof(decimal));

        Assert.Equal(DbType.Decimal, typeMapping.DbType);
        Assert.Equal("decimal(18,2)", typeMapping.StoreType);
    }

    [ConditionalFact]
    public void Does_decimal_mapping_for_nullable_CLR_types()
    {
        var typeMapping = GetTypeMapping(typeof(decimal?));

        Assert.Equal(DbType.Decimal, typeMapping.DbType);
        Assert.Equal("decimal(18,2)", typeMapping.StoreType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_non_key_SQL_Server_string_mapping(Type type, bool? unicode, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, unicode: unicode, fixedLength: fixedLength);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_non_key_SQL_Server_string_mapping_with_value_that_fits_max_length(Type type, bool? unicode, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: unicode, fixedLength: fixedLength);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string) ? "Va" : new List<int>();
        Assert.Equal(3, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_non_key_SQL_Server_string_mapping_with_max_length(Type type, bool? unicode, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: unicode, fixedLength: fixedLength);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), true)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_fixed_string_mapping_with_max_length_large_value(Type type, bool? unicode)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: unicode, fixedLength: true);

        Assert.Equal(DbType.StringFixedLength, typeMapping.DbType);
        Assert.Equal("nchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.True(typeMapping.IsFixedLength);

        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", value);
        Assert.Equal(DbType.String, parameter.DbType);
        Assert.Equal(4000, parameter.Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), true)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_fixed_string_mapping_with_max_length_small_value(Type type, bool? unicode)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: unicode, fixedLength: true);

        Assert.Equal(DbType.StringFixedLength, typeMapping.DbType);
        Assert.Equal("nchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.True(typeMapping.IsFixedLength);

        object value = type == typeof(string) ? "Va" : new List<int>();
        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", value);
        Assert.Equal(DbType.String, parameter.DbType);
        Assert.Equal(3, parameter.Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), true)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_fixed_string_mapping_with_max_length_exact_value(Type type, bool? unicode)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: unicode, fixedLength: true);

        Assert.Equal(DbType.StringFixedLength, typeMapping.DbType);
        Assert.Equal("nchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.True(typeMapping.IsFixedLength);

        object value = type == typeof(string) ? "Val" : new List<int> { 1 };
        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", value);
        Assert.Equal(DbType.StringFixedLength, parameter.DbType);
        Assert.Equal(3, parameter.Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_non_key_SQL_Server_string_mapping_with_long_string(Type type, bool? unicode, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, unicode: unicode, fixedLength: fixedLength);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string) ? new string('X', 4001) : Enumerable.Range(1, 2000).ToList();
        Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_non_key_SQL_Server_string_mapping_with_max_length_with_long_string(Type type, bool? unicode, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: unicode, fixedLength: fixedLength);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string) ? new string('X', 4001) : Enumerable.Range(1, 2000).ToList();
        Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_non_key_SQL_Server_required_string_mapping(Type type, bool? unicode, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, nullable: false, unicode: unicode, fixedLength: fixedLength);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_key_SQL_Server_string_mapping(Type type, bool? unicode, bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", type);
        property.IsNullable = false;
        property.SetIsUnicode(unicode);
        property.SetIsFixedLength(fixedLength);
        ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(450)", typeMapping.StoreType);
        Assert.Equal(450, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_foreign_key_SQL_Server_string_mapping(Type type, bool? unicode, bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", type);
        property.IsNullable = false;
        property.SetIsUnicode(unicode);
        property.SetIsFixedLength(fixedLength);
        var fkProperty = ((IMutableEntityType)property.DeclaringType).AddProperty("FK", type);
        var pk = ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);
        ((IMutableEntityType)property.DeclaringType).AddForeignKey(fkProperty, pk, ((IMutableEntityType)property.DeclaringType));

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(450)", typeMapping.StoreType);
        Assert.Equal(450, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_required_foreign_key_SQL_Server_string_mapping(Type type, bool? unicode, bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", type);
        property.IsNullable = false;
        property.SetIsUnicode(unicode);
        property.SetIsFixedLength(fixedLength);
        var fkProperty = ((IMutableEntityType)property.DeclaringType).AddProperty("FK", type);
        var pk = ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);
        ((IMutableEntityType)property.DeclaringType).AddForeignKey(fkProperty, pk, ((IMutableEntityType)property.DeclaringType));
        fkProperty.IsNullable = false;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(450)", typeMapping.StoreType);
        Assert.Equal(450, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), true, false)]
    [InlineData(typeof(string), null, false)]
    [InlineData(typeof(string), true, null)]
    [InlineData(typeof(string), null, null)]
    [InlineData(typeof(IEnumerable<int>), true, false)]
    [InlineData(typeof(IEnumerable<int>), null, false)]
    [InlineData(typeof(IEnumerable<int>), true, null)]
    [InlineData(typeof(IEnumerable<int>), null, null)]
    public void Does_indexed_column_SQL_Server_string_mapping(Type type, bool? unicode, bool? fixedLength)
    {
        var entityType = CreateEntityType<MyType>();
        var property = entityType.AddProperty("MyProp", type);
        property.SetIsUnicode(unicode);
        property.SetIsFixedLength(fixedLength);
        entityType.AddIndex(property);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(450)", typeMapping.StoreType);
        Assert.Equal(450, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(null, false)]
    [InlineData(true, null)]
    [InlineData(null, null)]
    public void Does_IndexAttribute_column_SQL_Server_string_mapping(bool? unicode, bool? fixedLength)
    {
        var entityType = CreateEntityType<MyTypeWithIndexAttribute>();
        var property = entityType.FindProperty("Name");
        property.SetIsUnicode(unicode);
        property.SetIsFixedLength(fixedLength);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyTypeWithIndexAttribute))!.FindProperty("Name")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(450)", typeMapping.StoreType);
        Assert.Equal(450, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(null, false)]
    [InlineData(true, null)]
    [InlineData(null, null)]
    public void Does_IndexAttribute_column_SQL_Server_primitive_collection_mapping(bool? unicode, bool? fixedLength)
    {
        var entityType = CreateEntityType<MyTypeWithIndexAttributeOnCollection>();
        var property = entityType.FindProperty("Ints")!;
        property.SetIsUnicode(unicode);
        property.SetIsFixedLength(fixedLength);

        var model = entityType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyTypeWithIndexAttributeOnCollection))!.FindProperty("Ints")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.String, typeMapping.DbType);
        Assert.Equal("nvarchar(450)", typeMapping.StoreType);
        Assert.Equal(450, typeMapping.Size);
        Assert.True(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(
            450, typeMapping.CreateParameter(
                new TestCommand(), "Ints", new List<int>
                {
                    1,
                    2,
                    3
                }).Size);
        Assert.Equal(typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_string_mapping_ansi(Type type, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, unicode: false, fixedLength: fixedLength);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_string_mapping_for_value_that_fits_with_max_length_ansi(Type type, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: false, fixedLength: fixedLength);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string) ? "Val" : new List<int> { 1 };
        Assert.Equal(3, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_string_mapping_with_max_length_ansi(Type type, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: false, fixedLength: fixedLength);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string))]
    [InlineData(typeof(IEnumerable<int>))]
    public void Does_non_key_SQL_Server_fixed_string_mapping_with_max_length_ansi_large_value(Type type)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: false, fixedLength: true);

        Assert.Equal(DbType.AnsiStringFixedLength, typeMapping.DbType);
        Assert.Equal("char(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.True(typeMapping.IsFixedLength);

        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", value);
        Assert.Equal(DbType.AnsiString, parameter.DbType);
        Assert.Equal(8000, parameter.Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string))]
    [InlineData(typeof(IEnumerable<int>))]
    public void Does_non_key_SQL_Server_fixed_string_mapping_with_max_length_ansi_small_value(Type type)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: false, fixedLength: true);

        Assert.Equal(DbType.AnsiStringFixedLength, typeMapping.DbType);
        Assert.Equal("char(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.True(typeMapping.IsFixedLength);

        object value = type == typeof(string) ? "Va" : new List<int>();
        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", value);
        Assert.Equal(DbType.AnsiString, parameter.DbType);
        Assert.Equal(3, parameter.Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string))]
    [InlineData(typeof(IEnumerable<int>))]
    public void Does_non_key_SQL_Server_fixed_string_mapping_with_max_length_ansi_exact_value(Type type)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: false, fixedLength: true);

        Assert.Equal(DbType.AnsiStringFixedLength, typeMapping.DbType);
        Assert.Equal("char(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.True(typeMapping.IsFixedLength);

        object value = type == typeof(string) ? "Val" : new List<int> { 1 };
        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", value);
        Assert.Equal(DbType.AnsiStringFixedLength, parameter.DbType);
        Assert.Equal(3, parameter.Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_string_mapping_with_long_string_ansi(Type type, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, unicode: false, fixedLength: fixedLength);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string) ? new string('X', 8001) : Enumerable.Range(1, 6000).ToList();
        Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_string_mapping_with_max_length_with_long_string_ansi(Type type, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, null, 3, unicode: false, fixedLength: fixedLength);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string) ? new string('X', 8001) : Enumerable.Range(1, 6000).ToList();
        Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_non_key_SQL_Server_required_string_mapping_ansi(Type type, bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(type, nullable: false, unicode: false, fixedLength: fixedLength);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_key_SQL_Server_string_mapping_ansi(Type type, bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", type);
        property.IsNullable = false;
        property.SetIsUnicode(false);
        property.SetIsFixedLength(fixedLength);
        ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_foreign_key_SQL_Server_string_mapping_ansi(Type type, bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", type);
        property.SetIsUnicode(false);
        property.SetIsFixedLength(fixedLength);
        property.IsNullable = false;
        var fkProperty = ((IMutableEntityType)property.DeclaringType).AddProperty("FK", type);
        var pk = ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);
        ((IMutableEntityType)property.DeclaringType).AddForeignKey(fkProperty, pk, ((IMutableEntityType)property.DeclaringType));

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_required_foreign_key_SQL_Server_string_mapping_ansi(Type type, bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", type);
        property.SetIsUnicode(false);
        property.SetIsFixedLength(fixedLength);
        property.IsNullable = false;
        var fkProperty = ((IMutableEntityType)property.DeclaringType).AddProperty("FK", type);
        var pk = ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);
        ((IMutableEntityType)property.DeclaringType).AddForeignKey(fkProperty, pk, ((IMutableEntityType)property.DeclaringType));
        fkProperty.IsNullable = false;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(IEnumerable<int>), false)]
    [InlineData(typeof(IEnumerable<int>), null)]
    public void Does_indexed_column_SQL_Server_string_mapping_ansi(Type type, bool? fixedLength)
    {
        var entityType = CreateEntityType<MyType>();
        var property = entityType.AddProperty("MyProp", type);
        property.SetIsUnicode(false);
        property.SetIsFixedLength(fixedLength);
        entityType.AddIndex(property);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        object value = type == typeof(string)
            ? "Value"
            : new List<int>
            {
                1,
                2,
                3
            };
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", value).Size);
        Assert.Equal(type == typeof(string) ? null : typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_IndexAttribute_column_SQL_Server_string_mapping_ansi(bool? fixedLength)
    {
        var entityType = CreateEntityType<MyTypeWithIndexAttribute>();
        var property = entityType.FindProperty("Name");
        property.SetIsUnicode(false);
        property.SetIsFixedLength(fixedLength);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyTypeWithIndexAttribute))!.FindProperty("Name")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_IndexAttribute_column_SQL_Server_primitive_collection_mapping_ansi(bool? fixedLength)
    {
        var entityType = CreateEntityType<MyTypeWithIndexAttributeOnCollection>();
        var property = entityType.FindProperty("Ints")!;
        property.SetIsUnicode(false);
        property.SetIsFixedLength(fixedLength);

        var model = entityType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyTypeWithIndexAttributeOnCollection))!.FindProperty("Ints")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.AnsiString, typeMapping.DbType);
        Assert.Equal("varchar(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.Size);
        Assert.False(typeMapping.IsUnicode);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(
            900, typeMapping.CreateParameter(
                new TestCommand(), "Ints", new List<int>
                {
                    1,
                    2,
                    3
                }).Size);
        Assert.Equal(typeof(int), typeMapping.ElementTypeMapping?.ClrType);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_non_key_SQL_Server_binary_mapping(bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(typeof(byte[]), fixedLength: fixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_non_key_SQL_Server_binary_mapping_with_max_length(bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(typeof(byte[]), null, 3, fixedLength: fixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(3, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_non_key_SQL_Server_binary_mapping_with_long_array(bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(typeof(byte[]), fixedLength: fixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8001]).Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_non_key_SQL_Server_binary_mapping_with_max_length_with_long_array(bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(typeof(byte[]), null, 3, fixedLength: fixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(3)", typeMapping.StoreType);
        Assert.Equal(3, typeMapping.Size);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8001]).Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_non_key_SQL_Server_required_binary_mapping(bool? fixedLength)
    {
        var typeMapping = GetTypeMapping(typeof(byte[]), nullable: false, fixedLength: fixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(max)", typeMapping.StoreType);
        Assert.Null(typeMapping.Size);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
    }

    [ConditionalTheory]
    [InlineData("binary(100)", null)]
    [InlineData("binary(100)", 100)]
    [InlineData("binary", 100)]
    [InlineData(null, 100)]
    public void Does_non_key_SQL_Server_fixed_length_binary_mapping_with_small_value(string typeName, int? maxLength)
    {
        var typeMapping = CreateBinaryMapping(typeName, maxLength);

        Assert.True(typeMapping.IsFixedLength);
        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("binary(100)", typeMapping.StoreType);

        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", new byte[10]);
        Assert.Equal(DbType.Binary, parameter.DbType);
        Assert.Equal(10, parameter.Size);
    }

    [ConditionalTheory]
    [InlineData("binary(100)", null)]
    [InlineData("binary(100)", 100)]
    [InlineData("binary", 100)]
    [InlineData(null, 100)]
    public void Does_non_key_SQL_Server_fixed_length_binary_mapping_with_exact_value(string typeName, int? maxLength)
    {
        var typeMapping = CreateBinaryMapping(typeName, maxLength);

        Assert.True(typeMapping.IsFixedLength);
        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("binary(100)", typeMapping.StoreType);

        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", new byte[100]);
        Assert.Equal(DbType.Binary, parameter.DbType);
        Assert.Equal(100, parameter.Size);
    }

    [ConditionalTheory]
    [InlineData("binary(100)", null)]
    [InlineData("binary(100)", 100)]
    [InlineData("binary", 100)]
    [InlineData(null, 100)]
    public void Does_non_key_SQL_Server_fixed_length_binary_mapping_with_large_value(string typeName, int? maxLength)
    {
        var typeMapping = CreateBinaryMapping(typeName, maxLength);

        Assert.True(typeMapping.IsFixedLength);
        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("binary(100)", typeMapping.StoreType);

        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", new byte[101]);
        Assert.Equal(DbType.Binary, parameter.DbType);
        Assert.Equal(101, parameter.Size);
    }

    [ConditionalTheory]
    [InlineData("binary(100)", null)]
    [InlineData("binary(100)", 100)]
    [InlineData("binary", 100)]
    [InlineData(null, 100)]
    public void Does_non_key_SQL_Server_fixed_length_binary_mapping_with_extreme_value(string typeName, int? maxLength)
    {
        var typeMapping = CreateBinaryMapping(typeName, maxLength);

        Assert.True(typeMapping.IsFixedLength);
        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("binary(100)", typeMapping.StoreType);

        var parameter = typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8001]);
        Assert.Equal(DbType.Binary, parameter.DbType);
        Assert.Equal(-1, parameter.Size);
    }

    private RelationalTypeMapping CreateBinaryMapping(string typeName, int? maxLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyBinaryProp", typeof(byte[]));

        if (typeName != null)
        {
            property.SetColumnType("binary(100)");
        }
        else
        {
            property.SetIsFixedLength(true);
        }

        if (maxLength != null)
        {
            property.SetMaxLength(maxLength);
        }

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyBinaryProp")!;
        return typeMappingSource.GetMapping(runtimeProperty);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_key_SQL_Server_binary_mapping(bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", typeof(byte[]));
        property.IsNullable = false;
        property.SetIsFixedLength(fixedLength);
        ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(900)", typeMapping.StoreType);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_foreign_key_SQL_Server_binary_mapping(bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", typeof(byte[]));
        property.IsNullable = false;
        property.SetIsFixedLength(fixedLength);
        var fkProperty = ((IMutableEntityType)property.DeclaringType).AddProperty("FK", typeof(byte[]));
        var pk = ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);
        ((IMutableEntityType)property.DeclaringType).AddForeignKey(fkProperty, pk, ((IMutableEntityType)property.DeclaringType));

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);
        Assert.False(typeMapping.IsFixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_required_foreign_key_SQL_Server_binary_mapping(bool? fixedLength)
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", typeof(byte[]));
        property.IsNullable = false;
        property.SetIsFixedLength(fixedLength);
        var fkProperty = ((IMutableEntityType)property.DeclaringType).AddProperty("FK", typeof(byte[]));
        var pk = ((IMutableEntityType)property.DeclaringType).SetPrimaryKey(property);
        ((IMutableEntityType)property.DeclaringType).AddForeignKey(fkProperty, pk, ((IMutableEntityType)property.DeclaringType));
        fkProperty.IsNullable = false;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);
        Assert.False(typeMapping.IsFixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(null)]
    public void Does_indexed_column_SQL_Server_binary_mapping(bool? fixedLength)
    {
        var entityType = CreateEntityType<MyType>();
        var property = entityType.AddProperty("MyProp", typeof(byte[]));
        property.SetIsFixedLength(fixedLength);
        entityType.AddIndex(property);

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);
        Assert.False(typeMapping.IsFixedLength);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("varbinary(900)", typeMapping.StoreType);
        Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[] { 0, 1, 2, 3 }).Size);
    }

    [ConditionalFact]
    public void Does_non_key_SQL_Server_rowversion_mapping()
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", typeof(byte[]));
        property.IsConcurrencyToken = true;
        property.ValueGenerated = ValueGenerated.OnAddOrUpdate;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("rowversion", typeMapping.StoreType);
        Assert.Equal(8, typeMapping.Size);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(8, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8]).Size);
    }

    [ConditionalFact]
    public void Does_non_key_SQL_Server_required_rowversion_mapping()
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", typeof(byte[]));
        property.IsConcurrencyToken = true;
        property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
        property.IsNullable = false;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.Equal("rowversion", typeMapping.StoreType);
        Assert.Equal(8, typeMapping.Size);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal(8, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8]).Size);
    }

    [ConditionalFact]
    public void Does_not_do_rowversion_mapping_for_non_computed_concurrency_tokens()
    {
        var property = CreateEntityType<MyType>().AddProperty("MyProp", typeof(byte[]));
        property.IsConcurrencyToken = true;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(MyType))!.FindProperty("MyProp")!;
        var typeMapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Equal(DbType.Binary, typeMapping.DbType);
        Assert.False(typeMapping.IsFixedLength);
        Assert.Equal("varbinary(max)", typeMapping.StoreType);
    }

    [ConditionalFact]
    public void Does_default_mappings_for_sequence_types()
    {
        var model = CreateModel();
        Assert.Equal("int", CreateRelationalTypeMappingSource(model).GetMapping(typeof(int)).StoreType);
        Assert.Equal("smallint", CreateRelationalTypeMappingSource(model).GetMapping(typeof(short)).StoreType);
        Assert.Equal("bigint", CreateRelationalTypeMappingSource(model).GetMapping(typeof(long)).StoreType);
        Assert.Equal("tinyint", CreateRelationalTypeMappingSource(model).GetMapping(typeof(byte)).StoreType);
    }

    [ConditionalFact]
    public void Does_default_mappings_for_strings_and_byte_arrays()
    {
        var model = CreateModel();
        Assert.Equal("nvarchar(max)", CreateRelationalTypeMappingSource(model).GetMapping(typeof(string)).StoreType);
        Assert.Equal("varbinary(max)", CreateRelationalTypeMappingSource(model).GetMapping(typeof(byte[])).StoreType);
    }

    [ConditionalFact]
    public void Does_default_mappings_for_values()
    {
        var model = CreateModel();
        Assert.Equal("nvarchar(max)", CreateRelationalTypeMappingSource(model).GetMappingForValue("Cheese").StoreType);
        Assert.Equal("varbinary(max)", CreateRelationalTypeMappingSource(model).GetMappingForValue(new byte[1]).StoreType);
        Assert.Equal("datetime2", CreateRelationalTypeMappingSource(model).GetMappingForValue(new DateTime()).StoreType);
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
        var model = CreateModel();

        var ex = Assert.Throws<InvalidOperationException>(() => CreateRelationalTypeMappingSource(model).GetMapping((IProperty)property));
        Assert.Equal(
            RelationalStrings.UnsupportedPropertyType("Entity1 (Dictionary<string, object>)", "Strange", "object"), ex.Message);

        Assert.Equal(
            RelationalStrings.UnsupportedType("object"),
            Assert.Throws<InvalidOperationException>(() => CreateRelationalTypeMappingSource(model).GetMapping(typeof(object))).Message);

        Assert.Equal(
            RelationalStrings.UnsupportedStoreType("object"),
            Assert.Throws<InvalidOperationException>(() => CreateRelationalTypeMappingSource(model).GetMapping("object")).Message);
    }

    [ConditionalTheory]
    [InlineData("bigint", typeof(long), null, false, false)]
    [InlineData("binary varying(333)", typeof(byte[]), 333, false, false)]
    [InlineData("binary varying(max)", typeof(byte[]), -1, false, false)]
    [InlineData("binary(333)", typeof(byte[]), 333, false, true)]
    [InlineData("bit", typeof(bool), null, false, false)]
    [InlineData("char varying(333)", typeof(string), 333, false, false)]
    [InlineData("char varying(max)", typeof(string), -1, false, false)]
    [InlineData("char(333)", typeof(string), 333, false, true)]
    [InlineData("character varying(333)", typeof(string), 333, false, false)]
    [InlineData("character varying(max)", typeof(string), -1, false, false)]
    [InlineData("character(333)", typeof(string), 333, false, true)]
    [InlineData("date", typeof(DateOnly), null, false, false)]
    [InlineData("datetime", typeof(DateTime), null, false, false)]
    [InlineData("datetime2", typeof(DateTime), null, false, false)]
    [InlineData("datetimeoffset", typeof(DateTimeOffset), null, false, false)]
    [InlineData("dec", typeof(decimal), null, false, false, "dec(18,0)")]
    [InlineData("decimal", typeof(decimal), null, false, false, "decimal(18,0)")]
    [InlineData("float", typeof(double), null, false, false)] // This is correct. SQL Server 'float' type maps to C# double
    [InlineData("float(10)", typeof(double), null, false, false)]
    [InlineData("image", typeof(byte[]), null, false, false)]
    [InlineData("int", typeof(int), null, false, false)]
    [InlineData("money", typeof(decimal), null, false, false)]
    [InlineData("national char varying(333)", typeof(string), 333, true, false)]
    [InlineData("national char varying(max)", typeof(string), -1, true, false)]
    [InlineData("national character varying(333)", typeof(string), 333, true, false)]
    [InlineData("national character varying(max)", typeof(string), -1, true, false)]
    [InlineData("national character(333)", typeof(string), 333, true, true)]
    [InlineData("nchar(333)", typeof(string), 333, true, true)]
    [InlineData("ntext", typeof(string), null, true, false)]
    [InlineData("numeric", typeof(decimal), null, false, false, "numeric(18,0)")]
    [InlineData("nvarchar(333)", typeof(string), 333, true, false)]
    [InlineData("nvarchar(max)", typeof(string), -1, true, false)]
    [InlineData("real", typeof(float), null, false, false)]
    [InlineData("rowversion", typeof(byte[]), 8, false, false)]
    [InlineData("smalldatetime", typeof(DateTime), null, false, false)]
    [InlineData("smallint", typeof(short), null, false, false)]
    [InlineData("smallmoney", typeof(decimal), null, false, false)]
    [InlineData("text", typeof(string), null, false, false)]
    [InlineData("time", typeof(TimeOnly), null, false, false)]
    [InlineData("timestamp", typeof(byte[]), 8, false, false)] // note: rowversion is a synonym stored the data type as 'timestamp'
    [InlineData("tinyint", typeof(byte), null, false, false)]
    [InlineData("uniqueidentifier", typeof(Guid), null, false, false)]
    [InlineData("varbinary(333)", typeof(byte[]), 333, false, false)]
    [InlineData("varbinary(max)", typeof(byte[]), -1, false, false)]
    [InlineData("VarCHaR(333)", typeof(string), 333, false, false)] // case-insensitive
    [InlineData("varchar(333)", typeof(string), 333, false, false)]
    [InlineData("varchar(max)", typeof(string), -1, false, false)]
    [InlineData("VARCHAR(max)", typeof(string), -1, false, false, "VARCHAR(max)")]
    public void Can_map_by_store_type(string storeType, Type type, int? size, bool unicode, bool fixedLength, string expectedType = null)
    {
        var mapping = CreateRelationalTypeMappingSource(CreateModel()).FindMapping(storeType);

        Assert.Same(type, mapping.ClrType);
        Assert.Equal(size, mapping.Size);
        Assert.Equal(unicode, mapping.IsUnicode);
        Assert.Equal(fixedLength, mapping.IsFixedLength);
        Assert.Equal(expectedType ?? storeType, mapping.StoreType);
    }

    [ConditionalTheory]
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(DateOnly), "date")]
    [InlineData(typeof(DateTime), "date")]
    [InlineData(typeof(TimeOnly), "time")]
    [InlineData(typeof(TimeSpan), "time")]
    public void Can_map_by_clr_and_store_types(Type clrType, string storeType)
    {
        var mapping = CreateRelationalTypeMappingSource(CreateModel()).FindMapping(clrType, storeType);

        Assert.Equal(storeType, mapping.StoreType);
        Assert.Same(clrType, mapping.ClrType);
    }

    [ConditionalTheory]
    [InlineData("char varying")]
    [InlineData("char")]
    [InlineData("character varying")]
    [InlineData("character")]
    [InlineData("national char varying")]
    [InlineData("national character varying")]
    [InlineData("national character")]
    [InlineData("nchar")]
    [InlineData("nvarchar")]
    [InlineData("varchar")]
    [InlineData("VarCHaR")]
    [InlineData("VARCHAR")]
    public void Can_map_string_base_type_name_and_size(string typeName)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<StringCheese>()
            .Property(e => e.StringWithSize)
            .HasColumnType(typeName)
            .HasMaxLength(2018)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(StringCheese))!.FindProperty("StringWithSize")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(string), mapping.ClrType);
        Assert.Equal(2018, mapping.Size);
        Assert.Equal(typeName.StartsWith("n", StringComparison.OrdinalIgnoreCase), mapping.IsUnicode);
        Assert.Equal(typeName.Contains("var", StringComparison.OrdinalIgnoreCase), !mapping.IsFixedLength);
        Assert.Equal(typeName + "(2018)", mapping.StoreType);
    }

    [ConditionalTheory]
    [InlineData("char varying")]
    [InlineData("char")]
    [InlineData("character varying")]
    [InlineData("character")]
    [InlineData("national char varying")]
    [InlineData("national character varying")]
    [InlineData("national character")]
    [InlineData("nchar")]
    [InlineData("nvarchar")]
    [InlineData("varchar")]
    [InlineData("VarCHaR")]
    [InlineData("VARCHAR")]
    public void Can_map_collection_base_type_name_and_size(string typeName)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<StringCheese>()
            .Property(e => e.CollectionWithSize)
            .HasColumnType(typeName)
            .HasMaxLength(2018)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(StringCheese))!.FindProperty("CollectionWithSize")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(IEnumerable<int>), mapping.ClrType);
        Assert.Equal(2018, mapping.Size);
        Assert.Equal(typeName.StartsWith("n", StringComparison.OrdinalIgnoreCase), mapping.IsUnicode);
        Assert.Equal(typeName.Contains("var", StringComparison.OrdinalIgnoreCase), !mapping.IsFixedLength);
        Assert.Equal(typeName + "(2018)", mapping.StoreType);
    }

    [ConditionalTheory]
    [InlineData("datetime2(0)", 0)]
    [InlineData("datetime2(1)", 1)]
    [InlineData("datetime2(2)", 2)]
    [InlineData("datetime2(3)", 3)]
    [InlineData("datetime2(4)", 4)]
    [InlineData("datetime2(5)", 5)]
    [InlineData("datetime2(6)", 6)]
    [InlineData("datetime2(7)", 7)]
    [InlineData("datetime2", null)]
    public void Can_map_datetime_base_type_columnType_with_precision(string typeName, int? precision)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<VarTimeEntity>()
            .Property(e => e.DateTimeWithPrecision)
            .HasColumnType(typeName)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(VarTimeEntity))!.FindProperty("DateTimeWithPrecision")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(DateTime), mapping.ClrType);
        Assert.Equal(precision, mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.Equal(typeName, mapping.StoreType, true);
    }

    [ConditionalTheory]
    [InlineData("datetime2(0)", 0)]
    [InlineData("datetime2(1)", 1)]
    [InlineData("datetime2(2)", 2)]
    [InlineData("datetime2(3)", 3)]
    [InlineData("datetime2(4)", 4)]
    [InlineData("datetime2(5)", 5)]
    [InlineData("datetime2(6)", 6)]
    [InlineData("datetime2(7)", 7)]
    public void Can_map_datetime_base_type_precision(string typeName, int precision)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<VarTimeEntity>()
            .Property(e => e.DateTimeWithPrecision)
            .HasPrecision(precision)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(VarTimeEntity))!.FindProperty("DateTimeWithPrecision")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(DateTime), mapping.ClrType);
        Assert.Equal(precision, mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.Equal(typeName, mapping.StoreType, true);
    }

    [ConditionalTheory]
    [InlineData("datetimeoffset(0)", 0)]
    [InlineData("datetimeoffset(1)", 1)]
    [InlineData("datetimeoffset(2)", 2)]
    [InlineData("datetimeoffset(3)", 3)]
    [InlineData("datetimeoffset(4)", 4)]
    [InlineData("datetimeoffset(5)", 5)]
    [InlineData("datetimeoffset(6)", 6)]
    [InlineData("datetimeoffset(7)", 7)]
    [InlineData("datetimeoffset", null)]
    public void Can_map_datetimeoffset_base_type_columnType_with_precision(string typeName, int? precision)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<VarTimeEntity>()
            .Property(e => e.DateTimeOffsetWithPrecision)
            .HasColumnType(typeName)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(VarTimeEntity))!.FindProperty("DateTimeOffsetWithPrecision")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(DateTimeOffset), mapping.ClrType);
        Assert.Equal(precision, mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.Equal(typeName, mapping.StoreType, true);
    }

    [ConditionalTheory]
    [InlineData("datetimeoffset(0)", 0)]
    [InlineData("datetimeoffset(1)", 1)]
    [InlineData("datetimeoffset(2)", 2)]
    [InlineData("datetimeoffset(3)", 3)]
    [InlineData("datetimeoffset(4)", 4)]
    [InlineData("datetimeoffset(5)", 5)]
    [InlineData("datetimeoffset(6)", 6)]
    [InlineData("datetimeoffset(7)", 7)]
    public void Can_map_datetimeoffset_base_type_precision(string typeName, int precision)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<VarTimeEntity>()
            .Property(e => e.DateTimeOffsetWithPrecision)
            .HasPrecision(precision)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(VarTimeEntity))!.FindProperty("DateTimeOffsetWithPrecision")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(DateTimeOffset), mapping.ClrType);
        Assert.Equal(precision, mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.Equal(typeName, mapping.StoreType, true);
    }

    [ConditionalTheory]
    [InlineData("time(0)", 0)]
    [InlineData("time(1)", 1)]
    [InlineData("time(2)", 2)]
    [InlineData("time(3)", 3)]
    [InlineData("time(4)", 4)]
    [InlineData("time(5)", 5)]
    [InlineData("time(6)", 6)]
    [InlineData("time(7)", 7)]
    [InlineData("time", null)]
    public void Can_map_time_base_type_columnType_with_precision(string typeName, int? precision)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<VarTimeEntity>()
            .Property(e => e.TimeSpanWithPrecision)
            .HasColumnType(typeName)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(VarTimeEntity))!.FindProperty("TimeSpanWithPrecision")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(TimeSpan), mapping.ClrType);
        Assert.Equal(precision, mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.Equal(typeName, mapping.StoreType, true);
    }

    [ConditionalTheory]
    [InlineData("time(0)", 0)]
    [InlineData("time(1)", 1)]
    [InlineData("time(2)", 2)]
    [InlineData("time(3)", 3)]
    [InlineData("time(4)", 4)]
    [InlineData("time(5)", 5)]
    [InlineData("time(6)", 6)]
    [InlineData("time(7)", 7)]
    public void Can_map_time_base_type_precision(string typeName, int precision)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<VarTimeEntity>()
            .Property(e => e.TimeSpanWithPrecision)
            .HasPrecision(precision)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(VarTimeEntity))!.FindProperty("TimeSpanWithPrecision")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(TimeSpan), mapping.ClrType);
        Assert.Equal(precision, mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.Equal(typeName, mapping.StoreType, true);
    }

    private class VarTimeEntity
    {
        public int Id { get; set; }
        public DateTime DateTimeWithPrecision { get; set; }
        public DateTimeOffset DateTimeOffsetWithPrecision { get; set; }
        public TimeSpan TimeSpanWithPrecision { get; set; }
    }

    [ConditionalTheory]
    [InlineData("binary varying")]
    [InlineData("binary")]
    [InlineData("varbinary")]
    public void Can_map_binary_base_type_name_and_size(string typeName)
    {
        var builder = CreateModelBuilder();

        var property = builder.Entity<StringCheese>()
            .Property(e => e.BinaryWithSize)
            .HasColumnType(typeName)
            .HasMaxLength(2018)
            .Metadata;

        var model = property.DeclaringType.Model.FinalizeModel();
        var typeMappingSource = CreateRelationalTypeMappingSource(model);
        var runtimeProperty = model.FindEntityType(typeof(StringCheese))!.FindProperty("BinaryWithSize")!;
        var mapping = typeMappingSource.GetMapping(runtimeProperty);

        Assert.Same(typeof(byte[]), mapping.ClrType);
        Assert.Equal(2018, mapping.Size);
        Assert.Equal(typeName.Contains("var", StringComparison.OrdinalIgnoreCase), !mapping.IsFixedLength);
        Assert.Equal(typeName + "(2018)", mapping.StoreType);
    }

    private class StringCheese
    {
        public int Id { get; set; }
        public string StringWithSize { get; set; }
        public List<int> CollectionWithSize { get; set; }
        public byte[] BinaryWithSize { get; set; }
    }

    [ConditionalFact]
    public void Key_with_store_type_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "money",
            mapper.GetMapping(model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "money",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void String_key_with_max_fixed_length_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "nchar(200)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "nchar(200)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void Binary_key_with_max_fixed_length_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "binary(100)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "binary(100)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void String_key_with_unicode_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "varchar(900)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "varchar(900)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void Key_store_type_if_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "money",
            mapper.GetMapping(model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "dec(6,1)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship2Id")).StoreType);
    }

    [ConditionalFact]
    public void String_FK_max_length_is_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "nchar(200)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "nchar(787)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship2Id")).StoreType);
    }

    [ConditionalFact]
    public void Binary_FK_max_length_is_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "binary(100)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "binary(767)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship2Id")).StoreType);
    }

    [ConditionalFact]
    public void String_FK_unicode_is_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "varchar(900)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "nvarchar(450)",
            mapper.GetMapping(model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship2Id")).StoreType);
    }

    [ConditionalFact]
    public void Plugins_can_override_builtin_mappings()
    {
        var typeMappingSource = new SqlServerTypeMappingSource(
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
        var typeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        model.ModelDependencies = new RuntimeModelDependencies(typeMappingSource, null!, null!);

        return typeMappingSource;
    }

    private enum LongEnum : long;

    private enum IntEnum;

    private enum ShortEnum : short;

    private enum ByteEnum : byte;

    protected override ModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configureConventions = null)
        => SqlServerTestHelpers.Instance.CreateConventionBuilder(configureConventions: configureConventions);

    private class TestParameter : DbParameter
    {
        public override void ResetDbType()
        {
        }

        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; }
        public override string SourceColumn { get; set; }
        public override object Value { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public override int Size { get; set; }
    }

    private class TestCommand : DbCommand
    {
        public override void Prepare()
        {
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; }
        protected override DbTransaction DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }

        public override void Cancel()
        {
        }

        protected override DbParameter CreateDbParameter()
            => new TestParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => throw new NotImplementedException();

        public override int ExecuteNonQuery()
            => throw new NotImplementedException();

        public override object ExecuteScalar()
            => throw new NotImplementedException();
    }
}
