// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A factory creating managers for SQL aliases, capable of generate uniquified table aliases.
/// </summary>
public interface ISqlAliasManagerFactory
{
    /// <summary>
    ///     Creates a new <see cref="SqlAliasManager" />.
    /// </summary>
    SqlAliasManager Create();
}
