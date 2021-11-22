// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Selects an <see cref="IMigrationsCodeGenerator" /> service for a given programming language.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IMigrationsCodeGeneratorSelector
{
    /// <summary>
    ///     Selects an <see cref="IMigrationsCodeGenerator" /> service for a given programming language.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <returns>The <see cref="IMigrationsCodeGenerator" />.</returns>
    IMigrationsCodeGenerator Select(string? language);
}
