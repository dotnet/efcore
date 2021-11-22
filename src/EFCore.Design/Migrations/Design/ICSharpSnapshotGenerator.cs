// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Used to generate C# code for creating an <see cref="IModel" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface ICSharpSnapshotGenerator
{
    /// <summary>
    ///     Generates code for creating an <see cref="IModel" />.
    /// </summary>
    /// <param name="builderName">The <see cref="ModelBuilder" /> variable name.</param>
    /// <param name="model">The model.</param>
    /// <param name="stringBuilder">The builder code is added to.</param>
    void Generate(
        string builderName,
        IModel model,
        IndentedStringBuilder stringBuilder);
}
