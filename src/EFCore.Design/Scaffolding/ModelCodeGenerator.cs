// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Used to generate code for a model.
    /// </summary>
    public abstract class ModelCodeGenerator : IModelCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelCodeGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        protected ModelCodeGenerator([NotNull] ModelCodeGeneratorDependencies dependencies)
            => Dependencies = Check.NotNull(dependencies, nameof(dependencies));

        /// <summary>
        ///     Gets the programming language supported by this service.
        /// </summary>
        /// <value> The language. </value>
        public abstract string Language { get; }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual ModelCodeGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Generates code for a model.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="namespace"> The namespace. </param>
        /// <param name="contextDir"> The directory of the <see cref="DbContext" />. </param>
        /// <param name="contextName"> The name of the <see cref="DbContext" />. </param>
        /// <param name="connectionString"> The connection string. </param>
        /// <param name="options"> The options to use during generation. </param>
        /// <returns> The generated model. </returns>
        public abstract ScaffoldedModel GenerateModel(
            IModel model,
            string @namespace,
            string contextDir,
            string contextName,
            string connectionString,
            ModelCodeGenerationOptions options);
    }
}
