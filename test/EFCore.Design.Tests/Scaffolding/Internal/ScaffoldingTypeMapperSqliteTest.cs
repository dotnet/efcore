// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class ScaffoldingTypeMapperSqliteTest
{
    // Native type cases...

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_text_column_with_abnormal_casing(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("text", keyOrIndex, rowVersion);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_integer_column_with_abnormal_casing(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("integer", keyOrIndex, rowVersion);

        AssertMapping<long>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_blob_column_with_abnormal_casing(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("blob", keyOrIndex, rowVersion);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_real_column_with_abnormal_casing(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("real", keyOrIndex, rowVersion);

        AssertMapping<double>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_text_column(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("TEXT", keyOrIndex, rowVersion);

        AssertMapping<string>(mapping, inferred: true, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_integer_column(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("INTEGER", keyOrIndex, rowVersion);

        AssertMapping<long>(mapping, inferred: true, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_blob_column(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("BLOB", keyOrIndex, rowVersion);

        AssertMapping<byte[]>(mapping, inferred: true, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_real_column(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("REAL", keyOrIndex, rowVersion);

        AssertMapping<double>(mapping, inferred: true, maxLength: null, unicode: null, fixedLength: null);
    }

    // Type affinity cases...

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Maps_int_column(bool isKeyOrIndex)
    {
        var mapping = CreateMapper().FindMapping("int", isKeyOrIndex, rowVersion: false);

        AssertMapping<long>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Maps_bigint_column(bool isKeyOrIndex)
    {
        var mapping = CreateMapper().FindMapping("bigint", isKeyOrIndex, rowVersion: false);

        AssertMapping<long>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_varbinary_max_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(max)", keyOrIndex: false, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_varbinary_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(200)", keyOrIndex: false, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_binary_sized_column()
    {
        var mapping = CreateMapper().FindMapping("binary(200)", keyOrIndex: false, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_varbinary_max_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(max)", keyOrIndex: true, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_varbinary_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(200)", keyOrIndex: true, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_varbinary_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(900)", keyOrIndex: true, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_binary_sized_column()
    {
        var mapping = CreateMapper().FindMapping("binary(200)", keyOrIndex: true, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_binary_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("binary(900)", keyOrIndex: true, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_rowversion_varbinary_max_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(max)", keyOrIndex: false, rowVersion: true);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_rowversion_varbinary_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(200)", keyOrIndex: false, rowVersion: true);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_rowversion_varbinary_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varbinary(8)", keyOrIndex: false, rowVersion: true);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_rowversion_binary_max_column()
    {
        var mapping = CreateMapper().FindMapping("binary(max)", keyOrIndex: false, rowVersion: true);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_rowversion_binary_sized_column()
    {
        var mapping = CreateMapper().FindMapping("binary(200)", keyOrIndex: false, rowVersion: true);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_rowversion_binary_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("binary(8)", keyOrIndex: false, rowVersion: true);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_nvarchar_max_column()
    {
        var mapping = CreateMapper().FindMapping("nvarchar(max)", keyOrIndex: false, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_nvarchar_sized_column()
    {
        var mapping = CreateMapper().FindMapping("nvarchar(200)", keyOrIndex: false, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_varchar_max_column()
    {
        var mapping = CreateMapper().FindMapping("varchar(max)", keyOrIndex: false, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_varchar_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varchar(200)", keyOrIndex: false, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_nvarchar_max_column()
    {
        var mapping = CreateMapper().FindMapping("nvarchar(max)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_nvarchar_sized_column()
    {
        var mapping = CreateMapper().FindMapping("nvarchar(200)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_varchar_max_column()
    {
        var mapping = CreateMapper().FindMapping("varchar(max)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_varchar_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varchar(200)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_nvarchar_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("nvarchar(450)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_varchar_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("varchar(900)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_nchar_sized_column()
    {
        var mapping = CreateMapper().FindMapping("nchar(200)", keyOrIndex: false, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_normal_char_sized_column()
    {
        var mapping = CreateMapper().FindMapping("char(200)", keyOrIndex: false, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_nchar_max_column()
    {
        var mapping = CreateMapper().FindMapping("nchar(max)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_nchar_sized_column()
    {
        var mapping = CreateMapper().FindMapping("nchar(200)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_char_max_column()
    {
        var mapping = CreateMapper().FindMapping("char(max)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_char_sized_column()
    {
        var mapping = CreateMapper().FindMapping("char(200)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_nchar_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("nchar(450)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_key_char_default_sized_column()
    {
        var mapping = CreateMapper().FindMapping("char(900)", keyOrIndex: true, rowVersion: false);

        AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    // Unknown type cases...

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Maps_empty_type_column(bool keyOrIndex, bool rowVersion)
    {
        var mapping = CreateMapper().FindMapping("", keyOrIndex, rowVersion);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalFact]
    public void Maps_rowversion_rowversion_column()
    {
        var mapping = CreateMapper().FindMapping("rowversion", keyOrIndex: false, rowVersion: true);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Maps_decimal_column(bool isKeyOrIndex)
    {
        var mapping = CreateMapper().FindMapping("decimal(18, 2)", isKeyOrIndex, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Maps_bit_column(bool isKeyOrIndex)
    {
        var mapping = CreateMapper().FindMapping("bit", isKeyOrIndex, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Maps_datetime_column(bool isKeyOrIndex)
    {
        var mapping = CreateMapper().FindMapping("datetime", isKeyOrIndex, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Maps_datetime2_column(bool isKeyOrIndex)
    {
        var mapping = CreateMapper().FindMapping("datetime2", isKeyOrIndex, rowVersion: false);

        AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null, fixedLength: null);
    }

    private static void AssertMapping<T>(TypeScaffoldingInfo mapping, bool inferred, int? maxLength, bool? unicode, bool? fixedLength)
    {
        Assert.Same(typeof(T), mapping.ClrType);
        Assert.Equal(inferred, mapping.IsInferred);
        Assert.Equal(maxLength, mapping.ScaffoldMaxLength);
        Assert.Equal(unicode, mapping.ScaffoldUnicode);
        Assert.Equal(fixedLength, mapping.ScaffoldFixedLength);
    }

    private static ScaffoldingTypeMapper CreateMapper()
        => new(
            new SqliteTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));
}
