// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Used to scaffold a model from a database schema.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IReverseEngineerScaffolder
{
    /// <summary>
    ///     Scaffolds a model from a database schema.
    /// </summary>
    /// <param name="connectionString">A connection string to the database.</param>
    /// <param name="databaseOptions">The options specifying which metadata to read from the database.</param>
    /// <param name="modelOptions">The options to use when reverse engineering a model from the database.</param>
    /// <param name="codeOptions">The options to use when generating code for the model.</param>
    /// <returns>The scaffolded model.</returns>
    ScaffoldedModel ScaffoldModel(
        string connectionString,
        DatabaseModelFactoryOptions databaseOptions,
        ModelReverseEngineerOptions modelOptions,
        ModelCodeGenerationOptions codeOptions);

    /// <summary>
    ///     Saves a scaffolded model to disk.
    /// </summary>
    /// <param name="scaffoldedModel">The scaffolded model.</param>
    /// <param name="outputDir">The output directory.</param>
    /// <param name="overwriteFiles">True to overwrite any existing files.</param>
    /// <returns>The model files.</returns>
    SavedModelFiles Save(
        ScaffoldedModel scaffoldedModel,
        string outputDir,
        bool overwriteFiles);
}
