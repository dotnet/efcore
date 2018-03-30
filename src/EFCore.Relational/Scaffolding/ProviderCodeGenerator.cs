// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Generates provider-specific code fragments.
    /// </summary>
    public abstract class ProviderCodeGenerator : IProviderConfigurationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProviderCodeGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        protected ProviderCodeGenerator([NotNull] ProviderCodeGeneratorDependencies dependencies)
            => Dependencies = Check.NotNull(dependencies, nameof(dependencies));

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual ProviderCodeGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Generates a code fragment like <c>.UseSqlServer("Database=Foo")</c> which can be used in
        ///     the <see cref="DbContext.OnConfiguring" /> method of the generated DbContext.
        /// </summary>
        /// <param name="connectionString"> The connection string to include in the code fragment. </param>
        /// <returns> The code fragment. </returns>
        public abstract MethodCallCodeFragment GenerateUseProvider(string connectionString);
    }
}
