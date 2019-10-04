// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Base class used by plugins to generate code fragments for reverse engineering.
    /// </summary>
    public class ProviderCodeGeneratorPlugin : IProviderCodeGeneratorPlugin
    {
        /// <summary>
        ///     Generates a method chain used to configure provider-specific options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        public virtual MethodCallCodeFragment GenerateContextOptions()
            => null;

        /// <summary>
        ///     Generates a method chain to configure additional context options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        public virtual MethodCallCodeFragment GenerateProviderOptions()
            => null;
    }
}
