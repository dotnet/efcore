// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Used to generate code for compiled model metadata.
    /// </summary>
    public interface ICompiledModelCodeGenerator : ILanguageBasedService
    {
        /// <summary>
        ///     Generates code for compiled model metadata.
        /// </summary>
        /// <param name="model"> The source model. </param>
        /// <param name="options"> The options to use during generation. </param>
        /// <returns> The generated model metadata files. </returns>
        IReadOnlyCollection<ScaffoldedFile> GenerateModel(
            IModel model,
            CompiledModelCodeGenerationOptions options);
    }
}
