// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Used to scaffold a compiled model from a model.
    /// </summary>
    public interface ICompiledModelScaffolder
    {
        /// <summary>
        ///     Scaffolds a compiled model from a model and saves it to disk.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="outputDir"> The output directory. </param>
        /// <param name="options"> The options to use when generating code for the model. </param>
        /// <returns> The scaffolded model files. </returns>
        IReadOnlyList<string> ScaffoldModel(
            IModel model,
            string outputDir,
            CompiledModelCodeGenerationOptions options);
    }
}
