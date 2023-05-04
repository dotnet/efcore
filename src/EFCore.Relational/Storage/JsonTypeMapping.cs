// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a <see cref="JsonElement" /> type and a database type.
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
public abstract class JsonTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="clrType">The .NET type.</param>
    /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
    protected JsonTypeMapping(string storeType, Type clrType, DbType? dbType)
        : base(storeType, clrType, dbType)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
    protected JsonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => throw new InvalidOperationException(
            RelationalStrings.MethodNeedsToBeImplementedInTheProvider);
}
