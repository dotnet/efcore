// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

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
    /// </summary>
    public sealed class CommandBatchPreparerDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="CommandBatchPreparer" />.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
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
        public CommandBatchPreparerDependencies(
            [NotNull] IModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IComparer<ModificationCommand> modificationCommandComparer,
            [NotNull] IKeyValueIndexFactorySource keyValueIndexFactorySource,
            [NotNull] Func<IStateManager> stateManager,
            [NotNull] ILoggingOptions loggingOptions,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger,
            [NotNull] IDbContextOptions options)
        {
            ModificationCommandBatchFactory = modificationCommandBatchFactory;
            ParameterNameGeneratorFactory = parameterNameGeneratorFactory;
            ModificationCommandComparer = modificationCommandComparer;
            KeyValueIndexFactorySource = keyValueIndexFactorySource;
            StateManager = stateManager;
            LoggingOptions = loggingOptions;
            UpdateLogger = updateLogger;
            Options = options;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IComparer<ModificationCommand> ModificationCommandComparer { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IKeyValueIndexFactorySource KeyValueIndexFactorySource { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Func<IStateManager> StateManager { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ILoggingOptions LoggingOptions { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                StateManager,
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
                StateManager,
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
                StateManager,
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
                StateManager,
                LoggingOptions,
                UpdateLogger,
                Options);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="stateManager"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public CommandBatchPreparerDependencies With([NotNull] Func<IStateManager> stateManager)
            => new CommandBatchPreparerDependencies(
                ModificationCommandBatchFactory,
                ParameterNameGeneratorFactory,
                ModificationCommandComparer,
                KeyValueIndexFactorySource,
                stateManager,
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
                StateManager,
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
                StateManager,
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
                StateManager,
                LoggingOptions,
                UpdateLogger,
                options);
    }
}
