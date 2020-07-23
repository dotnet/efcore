// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Used to generate code for a model.
    /// </summary>
    public interface IModelCodeGenerator : ILanguageBasedService
    {
        /// <summary>
        ///     Generates code for a model.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="options"> The options to use during generation. </param>
        /// <returns> The generated model. </returns>
        ScaffoldedModel GenerateModel(
            [NotNull] IModel model,
            [NotNull] ModelCodeGenerationOptions options);
    }
}
