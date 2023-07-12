// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Used to generate code for a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IModelCodeGenerator : ILanguageBasedService
{
    /// <summary>
    ///     Generates code for a model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="options">The options to use during generation.</param>
    /// <returns>The generated model.</returns>
    ScaffoldedModel GenerateModel(
        IModel model,
        ModelCodeGenerationOptions options);
}
