// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Base type for model code generators that use templates.
/// </summary>
public abstract class TemplatedModelGenerator : ModelCodeGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplatedModelGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    protected TemplatedModelGenerator(ModelCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Gets the subdirectory under the project to look for templates in.
    /// </summary>
    /// <value>The subdirectory.</value>
    protected static string TemplatesDirectory { get; } = Path.Combine("CodeTemplates", "EFCore");

    /// <inheritdoc />
    public override string? Language
        => null;

    /// <summary>
    ///     Checks whether the templates required for this generator are present.
    /// </summary>
    /// <param name="projectDir">The root project directory.</param>
    /// <returns><see langword="true" /> if the templates are present; otherwise, <see langword="false" />.</returns>
    public abstract bool HasTemplates(string projectDir);
}
