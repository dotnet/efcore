// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
