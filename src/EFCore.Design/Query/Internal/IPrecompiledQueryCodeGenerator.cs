// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Analyzes a user project, locates EF LINQ queries, and generates interceptors to run those queries
///     as precompiled queries.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public interface IPrecompiledQueryCodeGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    Task GeneratePrecompiledQueries(
        string projectDir,
        DbContext context,
        string outputDir,
        List<PrecompiledQueryCodeGenerator.QueryPrecompilationError> precompilationErrors,
        CancellationToken cancellationToken = default);
}
