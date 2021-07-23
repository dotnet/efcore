// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Implemented by plugins to generate code fragments for reverse engineering.
    /// </summary>
    public interface IProviderCodeGeneratorPlugin
    {
        /// <summary>
        ///     Generates a method chain used to configure provider-specific options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        MethodCallCodeFragment? GenerateProviderOptions();

        /// <summary>
        ///     Generates a method chain to configure additional context options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        MethodCallCodeFragment? GenerateContextOptions();
    }
}
