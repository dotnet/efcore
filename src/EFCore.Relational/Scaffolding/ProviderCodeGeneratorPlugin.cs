// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        public virtual MethodCallCodeFragment? GenerateContextOptions()
            => null;

        /// <summary>
        ///     Generates a method chain to configure additional context options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        public virtual MethodCallCodeFragment? GenerateProviderOptions()
            => null;
    }
}
