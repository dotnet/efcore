// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

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
        IModelCodeGenerator Select([CanBeNull] string language);
    }
}
