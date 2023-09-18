// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Contains extension methods on <see cref="DbFunctions" /> for the Microsoft.EntityFrameworkCore.Sqlite provider.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqliteDbFunctionsExtensions
{
    /// <summary>
    ///     Maps to the SQLite <c>glob</c> function which is similar to
    ///     <see cref="DbFunctionsExtensions.Like(DbFunctions, string, string)" /> but uses the file system globbing
    ///     syntax instead.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="matchExpression">The string that is to be matched.</param>
    /// <param name="pattern">The pattern which may involve wildcards <c>*,?,[,^,-,]</c>.</param>
    /// <returns><see langword="true" /> if there is a match.</returns>
    public static bool Glob(this DbFunctions _, string matchExpression, string pattern)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Glob)));

    /// <summary>
    ///     Maps to the SQLite <c>hex</c> function which returns a hexadecimal string representing the specified value.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="bytes">The binary value.</param>
    /// <returns>A hexadecimal string.</returns>
    public static string Hex(this DbFunctions _, byte[] bytes)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Hex)));

    /// <summary>
    ///     Maps to the SQLite <c>unhex</c> function which returns a BLOB representing decoding of the hexadecimal string.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="value">The hexadecimal string.</param>
    /// <returns>Decoded hexadecimal string as binary value.</returns>
    public static byte[] Unhex(this DbFunctions _, string value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unhex)));

    /// <summary>
    ///     Maps to the SQLite <c>unhex</c> function which returns a BLOB representing decoding of the hexadecimal string.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="value">The hexadecimal string.</param>
    /// <param name="ignoreChars">Characters that are ignored in <paramref name="value" />.</param>
    /// <returns>Decoded hexadecimal string as binary value.</returns>
    public static byte[] Unhex(this DbFunctions _, string value, string ignoreChars)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unhex)));

    /// <summary>
    ///     Maps to the SQLite <c>substr</c> function which returns a subarray of the specified value. The subarray starts
    ///     at <paramref name="startIndex" /> and continues to the end of the value.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="bytes">The binary value.</param>
    /// <param name="startIndex">The 1-based starting index. If negative, the index is relative to the end of the value.</param>
    /// <returns>The subarray.</returns>
    /// <remarks>
    ///     Use <see cref="string.Substring(int)" /> for string values.
    /// </remarks>
    public static byte[] Substr(this DbFunctions _, byte[] bytes, int startIndex)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Substr)));

    /// <summary>
    ///     Maps to the SQLite substr function which returns a subarray of the specified value. The subarray starts
    ///     at <paramref name="startIndex" /> and has the specified <paramref name="length" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="bytes">The binary value.</param>
    /// <param name="startIndex">The 1-based starting index. If negative, the index is relative to the end of the value.</param>
    /// <param name="length">
    ///     The length of the subarray. If negative, bytes preceding <paramref name="startIndex" /> are returned.
    /// </param>
    /// <returns>The subarray.</returns>
    /// <remarks>
    ///     Use <see cref="string.Substring(int, int)" /> for string values.
    /// </remarks>
    public static byte[] Substr(this DbFunctions _, byte[] bytes, int startIndex, int length)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Substr)));
}
