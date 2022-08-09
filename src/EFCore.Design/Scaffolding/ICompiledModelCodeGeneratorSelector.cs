// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Selects an <see cref="ICompiledModelCodeGenerator" /> service for given generation options.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-compiled-models">EF Core compiled models</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface ICompiledModelCodeGeneratorSelector
{
    /// <summary>
    ///     Selects an <see cref="ICompiledModelCodeGenerator" /> service for given generation options.
    /// </summary>
    /// <param name="options">The generation options.</param>
    /// <returns>The <see cref="ICompiledModelCodeGenerator" />.</returns>
    ICompiledModelCodeGenerator Select(CompiledModelCodeGenerationOptions options);
}
