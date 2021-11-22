// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Used to generate C# code for creating <see cref="MigrationOperation" /> objects.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface ICSharpMigrationOperationGenerator
{
    /// <summary>
    ///     Generates code for creating <see cref="MigrationOperation" /> objects.
    /// </summary>
    /// <param name="builderName">The <see cref="MigrationOperation" /> variable name.</param>
    /// <param name="operations">The operations.</param>
    /// <param name="builder">The builder code is added to.</param>
    void Generate(
        string builderName,
        IReadOnlyList<MigrationOperation> operations,
        IndentedStringBuilder builder);
}
