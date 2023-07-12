// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Selects an <see cref="IModelCodeGenerator" /> service for a given programming language.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IModelCodeGeneratorSelector
{
    /// <summary>
    ///     Selects an <see cref="IModelCodeGenerator" /> service for a given programming language.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <returns>The <see cref="IModelCodeGenerator" />.</returns>
    [Obsolete("Use the overload that takes ModelCodeGenerationOptions instead.")]
    IModelCodeGenerator Select(string? language);

    /// <summary>
    ///     Selects an <see cref="IModelCodeGenerator" /> service for a given set of options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The <see cref="IModelCodeGenerator" />.</returns>
    IModelCodeGenerator Select(ModelCodeGenerationOptions options)
#pragma warning disable CS0618 // Type or member is obsolete
        => Select(options.Language);
#pragma warning restore CS0618
}
