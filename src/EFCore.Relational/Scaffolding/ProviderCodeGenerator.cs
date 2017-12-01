// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Generates provider-specific code framgents.
    /// </summary>
    public abstract class ProviderCodeGenerator : IProviderCodeGenerator
    {
        /// <summary>
        ///     The name of the extension method on <see cref="DbContextOptionsBuilder" /> to use the provider.
        /// </summary>
        public abstract string UseProviderMethod { get; }
    }
}
