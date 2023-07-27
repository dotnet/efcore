// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET <see cref="char" /> type and a database type.
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
public class CharTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CharTypeMapping Default { get; } = new("char");

    /// <summary>
    ///     Initializes a new instance of the <see cref="CharTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
    public CharTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.String)
        : base(storeType, typeof(char), dbType, jsonValueReaderWriter: JsonCharReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CharTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
    protected CharTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new CharTypeMapping(parameters);

    /// <summary>
    ///     Generates the SQL representation of a non-null literal value.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    ///     The generated string.
    /// </returns>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        // NB: We can get Int32 values here too due to compiler-introduced convert nodes
        var charValue = Convert.ToChar(value);
        if (charValue == '\'')
        {
            return "''''";
        }

        return "'" + charValue + "'";
    }
}
