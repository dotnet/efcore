// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Selects an <see cref="ICompiledModelCodeGenerator" /> service for given generation options.
    /// </summary>
    public interface ICompiledModelCodeGeneratorSelector
    {
        /// <summary>
        ///     Selects an <see cref="ICompiledModelCodeGenerator" /> service for given generation options.
        /// </summary>
        /// <param name="options"> The generation options. </param>
        /// <returns> The <see cref="ICompiledModelCodeGenerator" />. </returns>
        ICompiledModelCodeGenerator Select(CompiledModelCodeGenerationOptions options);
    }
}
