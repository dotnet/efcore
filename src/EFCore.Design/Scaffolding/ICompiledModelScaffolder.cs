// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Used to scaffold a compiled model from a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-compiled-models">EF Core compiled models</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface ICompiledModelScaffolder
{
    /// <summary>
    ///     Scaffolds a compiled model from a model and saves it to disk.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="outputDir">The output directory.</param>
    /// <param name="options">The options to use when generating code for the model.</param>
    /// <returns>The scaffolded model files.</returns>
    IReadOnlyList<string> ScaffoldModel(
        IModel model,
        string outputDir,
        CompiledModelCodeGenerationOptions options);
}
