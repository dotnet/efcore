// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Selects an <see cref="IMigrationsCodeGenerator" /> service for a given programming language.
    /// </summary>
    public interface IMigrationsCodeGeneratorSelector
    {
        /// <summary>
        ///     Selects an <see cref="IMigrationsCodeGenerator" /> service for a given programming language.
        /// </summary>
        /// <param name="language"> The programming language. </param>
        /// <returns> The <see cref="IMigrationsCodeGenerator" />. </returns>
        IMigrationsCodeGenerator Select([CanBeNull] string language);
    }
}
