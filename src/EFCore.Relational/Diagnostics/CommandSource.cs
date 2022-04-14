// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Enum used by <see cref="CommandEventData" />, and subclasses to indicate the
///     source of the <see cref="DbCommand" /> being used to execute the command.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public enum CommandSource
{
    /// <summary>
    ///     Unknown
    /// </summary>
    Unknown,

    /// <summary>
    ///     Linq Query
    /// </summary>
    LinqQuery,

    /// <summary>
    ///     Save Changes
    /// </summary>
    SaveChanges,

    /// <summary>
    ///     Migrations
    /// </summary>
    Migrations,

    /// <summary>
    ///     FromSqlQuery
    /// </summary>
    FromSqlQuery,

    /// <summary>
    ///     ExecuteSqlRaw
    /// </summary>
    ExecuteSqlRaw,

    /// <summary>
    ///     ValueGenerator
    /// </summary>
    ValueGenerator,

    /// <summary>
    ///     Scaffolding
    /// </summary>
    Scaffolding,

    /// <summary>
    ///     BulkUpdate
    /// </summary>
    BulkUpdate
}
