// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A service for generating <see cref="MigrationCommand" /> objects that can
///     then be executed or scripted from a list of <see cref="MigrationOperation" />s.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface IMigrationsSqlGenerator
{
    /// <summary>
    ///     Generates commands from a list of operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="options">The options to use when generating commands.</param>
    /// <returns>The list of commands to be executed or scripted.</returns>
    IReadOnlyList<MigrationCommand> Generate(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default);
}
