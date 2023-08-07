// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET <see cref="byte" /> array type and a database type.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class ByteArrayTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ByteArrayTypeMapping Default { get; } = new("binary");

    /// <summary>
    ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
    /// <param name="size">The size of data the property is configured to store, or null if no size is configured.</param>
    public ByteArrayTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Binary,
        int? size = null)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(byte[]), jsonValueReaderWriter: JsonByteArrayReaderWriter.Instance), storeType, StoreTypePostfix.None, dbType,
                unicode: false, size))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
    protected ByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new ByteArrayTypeMapping(parameters);

    /// <summary>
    ///     Generates the SQL representation of a literal value.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    ///     The generated string.
    /// </returns>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("X'");

        foreach (var @byte in (byte[])value)
        {
            stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
        }

        stringBuilder.Append('\'');
        return stringBuilder.ToString();
    }
}
