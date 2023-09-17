// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET <see cref="UInt128" /> type and a database type.
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
public class UInt128TypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///  Column width in bytes
    /// </summary>
    public const int ColumnWidth = 40;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static UInt128TypeMapping Default { get; } = new("char(40)");

    /// <summary>
    ///     Initializes a new instance of the <see cref="UIntTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
    /// <param name="unicode">A value indicating whether the type should handle Unicode data or not.</param>
    /// <param name="size">The size of data the property is configured to store, or null if no size is configured.</param>
    /// <param name="fixedLength">A value indicating whether the type has fixed length data or not.</param>
    public UInt128TypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.String,
        bool unicode = false,
        int? size = ColumnWidth,
        bool fixedLength = true)
        : base(storeType, typeof(UInt128), dbType, unicode, size, fixedLength, jsonValueReaderWriter: JsonUInt128ReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UInt128TypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
    protected UInt128TypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new UInt128TypeMapping(parameters);

    /// <summary>
    ///     Gets the string format to be used to generate SQL literals of this type.
    /// </summary>
    protected override string SqlLiteralFormatString
        => "'{0:R}'";
}
