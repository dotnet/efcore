// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        protected ModelCodeGenerator(ModelCodeGeneratorDependencies dependencies)
            => Dependencies = Check.NotNull(dependencies, nameof(dependencies));

        /// <summary>
        ///     Gets the programming language supported by this service.
        /// </summary>
        /// <value> The language. </value>
        public abstract string Language { get; }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ModelCodeGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Generates code for a model.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="options"> The options to use during generation. </param>
        /// <returns> The generated model. </returns>
        public abstract ScaffoldedModel GenerateModel(
            IModel model,
            ModelCodeGenerationOptions options);
    }
}
