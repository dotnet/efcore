// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public enum SqlServerEngineType
{
    /// <summary>
    ///     Unknown SQL engine type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     SQL Server.
    /// </summary>
    SqlServer = 1,

    /// <summary>
    ///     Azure SQL.
    /// </summary>
    AzureSql = 2,

    /// <summary>
    ///     Azure Synapse.
    /// </summary>
    AzureSynapse = 3,
}
