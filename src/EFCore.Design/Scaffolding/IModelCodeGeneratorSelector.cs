// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Selects an <see cref="IModelCodeGenerator" /> service for a given programming language.
    /// </summary>
    public interface IModelCodeGeneratorSelector
    {
        /// <summary>
        ///     Selects an <see cref="IModelCodeGenerator" /> service for a given programming language.
        /// </summary>
        /// <param name="language"> The programming language. </param>
        /// <returns> The <see cref="IModelCodeGenerator" />. </returns>
        IModelCodeGenerator Select(string? language);
    }
}
