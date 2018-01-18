// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
        /// <param name="tables"> A list of tables to include. Empty to include all tables. </param>
        /// <param name="schemas"> A list of schemas to include. Empty to include all schemas. </param>
        /// <param name="namespace"> The namespace of the model. </param>
        /// <param name="language"> The programming language to scaffold for. </param>
        /// <param name="outputDbContextDir"> The DbContext output dirctory. </param>
        /// <param name="contextName"> The <see cref="DbContext"/> name. </param>
        /// <param name="useDataAnnotations"> True to scaffold data annotations. </param>
        /// <param name="useDatabaseNames"> True to use the database schema names directly. </param>
        /// <returns> The scaffolded model. </returns>
        ScaffoldedModel ScaffoldModel(
            [NotNull] string connectionString,
            [NotNull] IEnumerable<string> tables,
            [NotNull] IEnumerable<string> schemas,
            [NotNull] string @namespace,
            [NotNull] string language,
            [CanBeNull] string outputDbContextDir,
            [CanBeNull] string contextName,
            bool useDataAnnotations,
            bool useDatabaseNames);

        /// <summary>
        ///     Saves a scaffolded model to disk.
        /// </summary>
        /// <param name="scaffoldedModel"> The scaffolded model. </param>
        /// <param name="projectDir"> The project directory. </param>
        /// <param name="outputDir"> The output dirctory. </param>
        /// <param name="overwriteFiles"> True to overwrite any existing files. </param>
        /// <returns> The model files. </returns>
        ModelFiles Save(
            [NotNull] ScaffoldedModel scaffoldedModel,
            [NotNull] string projectDir,
            [CanBeNull] string outputDir,
            bool overwriteFiles);
    }
}
