// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Indicates what kind of impact on the result set a given command will have.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
[Flags]
public enum ResultSetMapping
{
    /// <summary>
    ///     The command does not have any results, neither as rows nor as output parameters.
    /// </summary>
    NoResults = 0,

    /// <summary>
    ///     The command maps to a row in the result set.
    /// </summary>
    HasResultRow = 1,

    /// <summary>
    ///     The command maps to a non-last row in the result set.
    /// </summary>
    NotLastInResultSet = 3,

    /// <summary>
    ///     The command maps to the last result in the result set.
    /// </summary>
    LastInResultSet = 5,

    /// <summary>
    ///     The command maps to a result set which contains only a single rows affected value.
    /// </summary>
    ResultSetWithRowsAffectedOnly = 9,

    /// <summary>
    ///     When rows with database-generated values are returned in non-deterministic ordering, it is necessary to project out a synthetic
    ///     position value, in order to look up the correct <see cref="ModificationCommand" /> and propagate the values. When this bit is
    ///     enabled, the current result row contains such a position value.
    /// </summary>
    IsPositionalResultMappingEnabled = 17,

    /// <summary>
    ///     The command has output parameters.
    /// </summary>
    HasOutputParameters = 32
}
