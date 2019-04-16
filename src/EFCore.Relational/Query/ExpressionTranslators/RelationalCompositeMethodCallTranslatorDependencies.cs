// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalCompositeMethodCallTranslator" />
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
    public sealed class RelationalCompositeMethodCallTranslatorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalCompositeMethodCallTranslator" />.
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
        /// <param name="logger"> A logger. </param>
        /// <param name="plugins"> The plugins. </param>
        public RelationalCompositeMethodCallTranslatorDependencies(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            [NotNull] IEnumerable<IMethodCallTranslatorPlugin> plugins)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(plugins, nameof(plugins));

            Logger = logger;
            Plugins = plugins;
        }

        /// <summary>
        ///     The logger.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }

        /// <summary>
        ///     Gets the plugins.
        /// </summary>
        public IEnumerable<IMethodCallTranslatorPlugin> Plugins { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="logger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalCompositeMethodCallTranslatorDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => new RelationalCompositeMethodCallTranslatorDependencies(logger, Plugins);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="plugins"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalCompositeMethodCallTranslatorDependencies With(
                [NotNull] IEnumerable<IMethodCallTranslatorPlugin> plugins)
            => new RelationalCompositeMethodCallTranslatorDependencies(Logger, plugins);
    }
}
