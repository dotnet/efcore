// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="CommandBatchPreparer" />
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
    public sealed class CommandBatchPreparerDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="CommandBatchPreparer" />.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public CommandBatchPreparerDependencies(
            [NotNull] IModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IComparer<ModificationCommand> modificationCommandComparer,
            [NotNull] IKeyValueIndexFactorySource keyValueIndexFactorySource,
            [NotNull] ILoggingOptions loggingOptions,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger,
            [NotNull] IDbContextOptions options)
        {
            ModificationCommandBatchFactory = modificationCommandBatchFactory;
            ParameterNameGeneratorFactory = parameterNameGeneratorFactory;
            ModificationCommandComparer = modificationCommandComparer;
            KeyValueIndexFactorySource = keyValueIndexFactorySource;
            LoggingOptions = loggingOptions;
            UpdateLogger = updateLogger;
            Options = options;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IComparer<ModificationCommand> ModificationCommandComparer { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IKeyValueIndexFactorySource KeyValueIndexFactorySource { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ILoggingOptions LoggingOptions { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IDbContextOptions Options { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="modificationCommandBatchFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] IModificationCommandBatchFactory modificationCommandBatchFactory)
            => new CommandBatchPreparerDependencies(
                modificationCommandBatchFactory,
                ParameterNameGeneratorFactory,
                ModificationCommandComparer,
                KeyValueIndexFactorySource,
                LoggingOptions,
                UpdateLogger,
                Options);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="parameterNameGeneratorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory)
            => new CommandBatchPreparerDependencies(
                ModificationCommandBatchFactory,
                parameterNameGeneratorFactory,
                ModificationCommandComparer,
                KeyValueIndexFactorySource,
                LoggingOptions,
                UpdateLogger,
                Options);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="modificationCommandComparer"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] IComparer<ModificationCommand> modificationCommandComparer)
            => new CommandBatchPreparerDependencies(
                ModificationCommandBatchFactory,
                ParameterNameGeneratorFactory,
                modificationCommandComparer,
                KeyValueIndexFactorySource,
                LoggingOptions,
                UpdateLogger,
                Options);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="keyValueIndexFactorySource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] IKeyValueIndexFactorySource keyValueIndexFactorySource)
            => new CommandBatchPreparerDependencies(
                ModificationCommandBatchFactory,
                ParameterNameGeneratorFactory,
                ModificationCommandComparer,
                keyValueIndexFactorySource,
                LoggingOptions,
                UpdateLogger,
                Options);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="loggingOptions"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] ILoggingOptions loggingOptions)
            => new CommandBatchPreparerDependencies(
                ModificationCommandBatchFactory,
                ParameterNameGeneratorFactory,
                ModificationCommandComparer,
                KeyValueIndexFactorySource,
                loggingOptions,
                UpdateLogger,
                Options);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="updateLogger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
            => new CommandBatchPreparerDependencies(
                ModificationCommandBatchFactory,
                ParameterNameGeneratorFactory,
                ModificationCommandComparer,
                KeyValueIndexFactorySource,
                LoggingOptions,
                updateLogger,
                Options);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="options"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] IDbContextOptions options)
            => new CommandBatchPreparerDependencies(
                ModificationCommandBatchFactory,
                ParameterNameGeneratorFactory,
                ModificationCommandComparer,
                KeyValueIndexFactorySource,
                LoggingOptions,
                UpdateLogger,
                options);
    }
}
