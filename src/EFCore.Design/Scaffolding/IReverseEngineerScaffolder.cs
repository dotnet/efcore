// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Used to scaffold a model from a database schema.
    /// </summary>
    public interface IReverseEngineerScaffolder
    {
        /// <summary>
        ///     Scaffolds a model from a database schema.
        /// </summary>
        /// <param name="connectionString"> A connection string to the database. </param>
        /// <param name="databaseOptions"> The options specifying which metadata to read from the database. </param>
        /// <param name="modelOptions"> The options to use when reverse engineering a model from the database. </param>
        /// <param name="codeOptions"> The options to use when generating code for the model. </param>
        /// <returns> The scaffolded model. </returns>
        ScaffoldedModel ScaffoldModel(
            [NotNull] string connectionString,
            [NotNull] DatabaseModelFactoryOptions databaseOptions,
            [NotNull] ModelReverseEngineerOptions modelOptions,
            [NotNull] ModelCodeGenerationOptions codeOptions);

        /// <summary>
        ///     Saves a scaffolded model to disk.
        /// </summary>
        /// <param name="scaffoldedModel"> The scaffolded model. </param>
        /// <param name="outputDir"> The output directory. </param>
        /// <param name="overwriteFiles"> True to overwrite any existing files. </param>
        /// <returns> The model files. </returns>
        SavedModelFiles Save(
            [NotNull] ScaffoldedModel scaffoldedModel,
            [CanBeNull] string outputDir,
            bool overwriteFiles);
    }
}
