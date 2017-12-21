// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalConventionSetBuilder" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    /// </summary>
    public sealed class RelationalConventionSetBuilderDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalConventionSetBuilder" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        /// </summary>
        [Obsolete("Use the constructor with most parameters")]
        public RelationalConventionSetBuilderDependencies(
            [NotNull] IRelationalTypeMapper typeMapper,
            [CanBeNull] ICurrentDbContext currentContext,
            [CanBeNull] IDbSetFinder setFinder)
            : this(setFinder, currentContext, null, typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
        }

        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalConventionSetBuilder" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        /// </summary>
        public RelationalConventionSetBuilderDependencies(
            [NotNull] IRelationalTypeMapper typeMapper,
            [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger,
            [CanBeNull] ICurrentDbContext currentContext,
            [CanBeNull] IDbSetFinder setFinder)
            : this(setFinder, currentContext, logger, typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
        }

        private RelationalConventionSetBuilderDependencies(
            IDbSetFinder setFinder,
            ICurrentDbContext currentContext,
            IDiagnosticsLogger<DbLoggerCategory.Model> logger,
            IRelationalTypeMapper typeMapper)
        {
            TypeMapper = typeMapper;
            Logger = logger
                     ?? new DiagnosticsLogger<DbLoggerCategory.Model>(new LoggerFactory(), new LoggingOptions(), new DiagnosticListener(""));
            Context = currentContext;
            SetFinder = setFinder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IRelationalTypeMapper TypeMapper { get; }

        /// <summary>
        ///     The logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ICurrentDbContext Context { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDbSetFinder SetFinder { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMapper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalConventionSetBuilderDependencies With([NotNull] IRelationalTypeMapper typeMapper)
            => new RelationalConventionSetBuilderDependencies(typeMapper, Logger, Context, SetFinder);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalConventionSetBuilderDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            => new RelationalConventionSetBuilderDependencies(TypeMapper, logger, Context, SetFinder);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalConventionSetBuilderDependencies With([NotNull] ICurrentDbContext currentContext)
            => new RelationalConventionSetBuilderDependencies(TypeMapper, Logger, currentContext, SetFinder);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="setFinder"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalConventionSetBuilderDependencies With([NotNull] IDbSetFinder setFinder)
            => new RelationalConventionSetBuilderDependencies(TypeMapper, Logger, Context, setFinder);
    }
}
