// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         A parameter object for the execution methods on <see cref="RelationalCommand" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public readonly struct RelationalCommandParameterObject
    {
        /// <summary>
        ///     <para>
        ///         Creates a new parameter object for the given parameters.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection on which the command will execute. </param>
        /// <param name="parameterValues"> The SQL parameter values to use, or null if none. </param>
        /// <param name="readerColumns"> The expected columns if the reader needs to be buffered, or null otherwise. </param>
        /// <param name="context"> The current <see cref="DbContext" /> instance, or null if it is not known. </param>
        /// <param name="logger"> A logger, or null if no logger is available. </param>
        public RelationalCommandParameterObject(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            [CanBeNull] IReadOnlyList<ReaderColumn> readerColumns,
            [CanBeNull] DbContext context,
            [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            : this(connection, parameterValues, readerColumns, context, logger, detailedErrorsEnabled: false)
        {
        }

        /// <summary>
        ///     <para>
        ///         Creates a new parameter object for the given parameters.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection on which the command will execute. </param>
        /// <param name="parameterValues"> The SQL parameter values to use, or null if none. </param>
        /// <param name="readerColumns"> The expected columns if the reader needs to be buffered, or null otherwise. </param>
        /// <param name="context"> The current <see cref="DbContext" /> instance, or null if it is not known. </param>
        /// <param name="logger"> A logger, or null if no logger is available. </param>
        /// <param name="detailedErrorsEnabled"> A value indicating if detailed errors are enabled. </param>
        public RelationalCommandParameterObject(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            [CanBeNull] IReadOnlyList<ReaderColumn> readerColumns,
            [CanBeNull] DbContext context,
            [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            bool detailedErrorsEnabled)
        {
            Check.NotNull(connection, nameof(connection));

            Connection = connection;
            ParameterValues = parameterValues;
            ReaderColumns = readerColumns;
            Context = context;
            Logger = logger;
            DetailedErrorsEnabled = detailedErrorsEnabled;
        }

        /// <summary>
        ///     The connection on which the command will execute.
        /// </summary>
        public IRelationalConnection Connection { get; }

        /// <summary>
        ///     The SQL parameter values to use, or null if none.
        /// </summary>
        public IReadOnlyDictionary<string, object> ParameterValues { get; }

        /// <summary>
        ///     The expected columns if the reader needs to be buffered, or null otherwise.
        /// </summary>
        public IReadOnlyList<ReaderColumn> ReaderColumns { get; }

        /// <summary>
        ///     The current <see cref="DbContext" /> instance, or null if it is not known.
        /// </summary>
        public DbContext Context { get; }

        /// <summary>
        ///     A logger, or null if no logger is available.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Database.Command> Logger { get; }

        /// <summary>
        ///     A value indicating if detailed errors are enabled.
        /// </summary>
        public bool DetailedErrorsEnabled { get; }
    }
}
