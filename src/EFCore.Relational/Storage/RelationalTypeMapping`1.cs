// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET type and a database type, where the .NET type used in the EF model
///         is statically known.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Knowing the model CLR type statically allows the default <see cref="ValueComparer" /> instances to be created
///         without reflection, which is required for NativeAOT compatibility.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="T">The .NET type used in the EF model.</typeparam>
public abstract class RelationalTypeMapping<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties)]
    T> : RelationalTypeMapping
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalTypeMapping{T}" /> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    protected RelationalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalTypeMapping{T}" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="clrType">The .NET type.</param>
    /// <param name="dbType">The <see cref="System.Data.DbType" /> to be used.</param>
    /// <param name="unicode">A value indicating whether the type should handle Unicode data or not.</param>
    /// <param name="size">The size of data the property is configured to store, or null if no size is configured.</param>
    /// <param name="fixedLength">A value indicating whether the type has fixed length data or not.</param>
    /// <param name="precision">The precision of data the property is configured to store, or null if no precision is configured.</param>
    /// <param name="scale">The scale of data the property is configured to store, or null if no scale is configured.</param>
    /// <param name="jsonValueReaderWriter">Handles reading and writing JSON values for instances of the mapped type.</param>
    protected RelationalTypeMapping(
        string storeType,
        Type clrType,
        DbType? dbType = null,
        bool unicode = false,
        int? size = null,
        bool fixedLength = false,
        int? precision = null,
        int? scale = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        : base(storeType, clrType, dbType, unicode, size, fixedLength, precision, scale, jsonValueReaderWriter)
    {
    }

    /// <inheritdoc />
    protected override ValueComparer CreateDefaultComparer(bool favorStructuralComparisons)
        => ClrType == typeof(T)
            ? ValueComparer.CreateDefault<T>(favorStructuralComparisons)
            : base.CreateDefaultComparer(favorStructuralComparisons);
}
