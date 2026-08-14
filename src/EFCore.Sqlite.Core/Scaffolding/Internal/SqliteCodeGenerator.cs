// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteCodeGenerator : ProviderCodeGenerator
{
    private static readonly MethodInfo UseSqliteMethodInfo
        = typeof(SqliteDbContextOptionsBuilderExtensions).GetRuntimeMethod(
            nameof(SqliteDbContextOptionsBuilderExtensions.UseSqlite),
            [typeof(DbContextOptionsBuilder), typeof(string), typeof(Action<SqliteDbContextOptionsBuilder>)])!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteCodeGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    public SqliteCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions)
        => new(
            UseSqliteMethodInfo,
            providerOptions == null
                ? [connectionString]
                : [connectionString, new NestedClosureCodeFragment("x", providerOptions)]);
}
