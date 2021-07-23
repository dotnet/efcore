// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Parameters for creating a <see cref="IMutableModificationCommand" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public sealed record ModificationCommandParameters
    {
        /// <summary>
        ///     Creates a new <see cref="ModificationCommandParameters" /> instance.
        /// </summary>
        /// <param name="tableName"> The name of the table containing the data to be modified. </param>
        /// <param name="schemaName"> The schema containing the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether potentially sensitive data (e.g. database values) can be logged. </param>
        /// <param name="comparer"> An <see cref="IComparer{T}" /> for <see cref="IUpdateEntry" />. </param>
        /// <param name="generateParameterName"> A delegate to generate parameter names. </param>
        /// <param name="logger"> An <see cref="IDiagnosticsLogger{T}" /> for <see cref="DbLoggerCategory.Update" />. </param>
        public ModificationCommandParameters(
            string tableName,
            string? schemaName,
            bool sensitiveLoggingEnabled,
            IComparer<IUpdateEntry>? comparer = null,
            Func<string>? generateParameterName = null,
            IDiagnosticsLogger<DbLoggerCategory.Update>? logger = null)
        {
            TableName = tableName;
            Schema = schemaName;
            GenerateParameterName = generateParameterName;
            SensitiveLoggingEnabled = sensitiveLoggingEnabled;
            Comparer = comparer;
            Logger = logger;
        }

        /// <summary>
        ///     The name of the table containing the data to be modified.
        /// </summary>
        public string TableName { get; init; }

        /// <summary>
        ///     The schema containing the table, or <see langword="null" /> to use the default schema.
        /// </summary>
        public string? Schema { get; init; }

        /// <summary>
        ///     A delegate to generate parameter names.
        /// </summary>
        public Func<string>? GenerateParameterName { get; init; }

        /// <summary>
        ///     Indicates whether potentially sensitive data (e.g. database values) can be logged.
        /// </summary>
        public bool SensitiveLoggingEnabled { get; init; }

        /// <summary>
        ///     An <see cref="IComparer{T}" /> for <see cref="IUpdateEntry" />.
        /// </summary>
        public IComparer<IUpdateEntry>? Comparer { get; init; }

        /// <summary>
        ///     A <see cref="IDiagnosticsLogger{T}" /> for <see cref="DbLoggerCategory.Update" />.
        /// </summary>
        public IDiagnosticsLogger<DbLoggerCategory.Update>? Logger { get; init; }
    }
}
