// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET <see cref="decimal" /> type and a database type.
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
public class DecimalTypeMapping : RelationalTypeMapping
{
    private const string DecimalFormatConst = "{0:0.0###########################}";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static DecimalTypeMapping Default { get; } = new("decimal");

    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
    /// <param name="precision">The precision of data the property is configured to store, or null if the default precision is required.</param>
    /// <param name="scale">The scale of data the property is configured to store, or null if the default scale is required.</param>
    public DecimalTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Decimal,
        int? precision = null,
        int? scale = null)
        : base(
            storeType, typeof(decimal), dbType, precision: precision, scale: scale, jsonValueReaderWriter: JsonDecimalReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
    protected DecimalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DecimalTypeMapping(parameters);

    /// <summary>
    ///     Gets the string format to be used to generate SQL literals of this type.
    /// </summary>
    protected override string SqlLiteralFormatString
        => DecimalFormatConst;
}
