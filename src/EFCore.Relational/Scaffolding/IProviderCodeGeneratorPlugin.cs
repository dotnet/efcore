// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        MethodCallCodeFragment GenerateProviderOptions();

        /// <summary>
        ///     Generates a method chain to configure additional context options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        MethodCallCodeFragment GenerateContextOptions();
    }
}
