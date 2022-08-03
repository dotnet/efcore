// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Used to generate code for compiled model metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-compiled-models">EF Core compiled models</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface ICompiledModelCodeGenerator : ILanguageBasedService
{
    /// <summary>
    ///     Generates code for compiled model metadata.
    /// </summary>
    /// <param name="model">The source model.</param>
    /// <param name="options">The options to use during generation.</param>
    /// <returns>The generated model metadata files.</returns>
    IReadOnlyCollection<ScaffoldedFile> GenerateModel(
        IModel model,
        CompiledModelCodeGenerationOptions options);
}
