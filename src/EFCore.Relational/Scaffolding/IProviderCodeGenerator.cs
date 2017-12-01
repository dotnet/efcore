// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     A service typically implemented by database providers to generate code fragments
    ///     for reverse engineering.
    /// </summary>
    public interface IProviderCodeGenerator
    {
        /// <summary>
        ///     The name of the extension method on <see cref="DbContextOptionsBuilder" /> to use the provider.
        /// </summary>
        string UseProviderMethod { get; }
    }
}
