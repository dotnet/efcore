// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Base class for SQL Server, Azure SQL, Azure Synapse, etc. specific builders.
/// </summary>
public abstract class SqlEngineDbContextOptionsBuilderBase<TSelf>(DbContextOptionsBuilder optionsBuilder)
    : RelationalDbContextOptionsBuilder<TSelf, SqlServerOptionsExtension>(optionsBuilder)
    where TSelf : SqlEngineDbContextOptionsBuilderBase<TSelf>
{
}
