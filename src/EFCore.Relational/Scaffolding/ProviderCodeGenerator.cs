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
        ///     Generates a method chain used to configure provider-specific options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        public virtual MethodCallCodeFragment GenerateProviderOptions()
        {
            MethodCallCodeFragment providerOptions = null;

            foreach (var plugin in Dependencies.Plugins)
            {
                var chainedCall = plugin.GenerateProviderOptions();
                if (chainedCall == null)
                {
                    continue;
                }

                providerOptions = providerOptions?.Chain(chainedCall) ?? chainedCall;
            }

            return providerOptions;
        }

        /// <summary>
        ///     Generates a code fragment like <c>.UseSqlServer("Database=Foo")</c> which can be used in
        ///     the <see cref="DbContext.OnConfiguring" /> method of the generated DbContext.
        /// </summary>
        /// <param name="connectionString"> The connection string to include in the code fragment. </param>
        /// <param name="providerOptions"> The method chain used to configure provider options. </param>
        /// <returns> The code fragment. </returns>
        public abstract MethodCallCodeFragment GenerateUseProvider(
            string connectionString,
            MethodCallCodeFragment providerOptions);

        /// <summary>
        ///     Generates a method chain to configure additional context options.
        /// </summary>
        /// <returns> The method chain. May be null. </returns>
        public virtual MethodCallCodeFragment GenerateContextOptions()
        {
            MethodCallCodeFragment contextOptions = null;

            foreach (var plugin in Dependencies.Plugins)
            {
                var chainedCall = plugin.GenerateContextOptions();
                if (chainedCall == null)
                {
                    continue;
                }

                contextOptions = contextOptions?.Chain(chainedCall) ?? chainedCall;
            }

            return contextOptions;
        }
    }
}
