// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalCommandBuilder" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed class RelationalCommandBuilderDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalCommandBuilder" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="typeMappingSource"> The source for <see cref="RelationalTypeMapping"/>s to use. </param>
        /// <param name="logger"> The command logger. </param>
        public RelationalCommandBuilderDependencies(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(logger, nameof(logger));

            TypeMappingSource = typeMappingSource;
            Logger = logger;
        }

        /// <summary>
        ///     The command logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Database.Command> Logger { get; }

        /// <summary>
        ///     The source for <see cref="RelationalTypeMapping"/>s to use.
        /// </summary>
        public IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMappingSource">A replacement for the current dependency of this type.</param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalCommandBuilderDependencies With([NotNull] IRelationalTypeMappingSource typeMappingSource)
            => new RelationalCommandBuilderDependencies(typeMappingSource, Logger);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger">A replacement for the current dependency of this type.</param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalCommandBuilderDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            => new RelationalCommandBuilderDependencies(TypeMappingSource, logger);
    }
}
