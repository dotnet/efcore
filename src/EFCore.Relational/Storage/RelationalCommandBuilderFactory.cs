// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Factory for <see cref="RelationalCommandBuilder" /> instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class RelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        /// <summary>
        ///     <para>
        ///         Constructs a new <see cref="RelationalCommand" />.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalCommandBuilderFactory(
            RelationalCommandBuilderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        public virtual RelationalCommandBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a new <see cref="IRelationalCommandBuilder" />.
        /// </summary>
        /// <returns> The newly created builder. </returns>
        public virtual IRelationalCommandBuilder Create()
            => new RelationalCommandBuilder(Dependencies);
    }
}
