// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Generates provider-specific code fragments.
    /// </summary>
    public abstract class ProviderCodeGenerator : IProviderCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProviderCodeGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        protected ProviderCodeGenerator([NotNull] ProviderCodeGeneratorDependencies dependencies)
            => Dependencies = Check.NotNull(dependencies, nameof(dependencies));

        /// <summary>
        ///     The name of the extension method on <see cref="DbContextOptionsBuilder" /> to use the provider.
        /// </summary>
        public abstract string UseProviderMethod { get; }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual ProviderCodeGeneratorDependencies Dependencies { get; }
    }
}
