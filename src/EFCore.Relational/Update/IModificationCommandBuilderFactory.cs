// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A service for creating <see cref="IModificationCommandBuilder" /> instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IModificationCommandBuilderFactory
    {
        /// <summary>
        ///     Creates a new object with <see cref="ModificationCommandBatch" /> interface.
        /// </summary>
        /// <param name="tableName"> Name of table. </param>
        /// <param name="schemaName"> Name of schema. </param>
        /// <param name="generateParameterName"> Service for generation parameter names. </param>
        /// <param name="sensitiveLoggingEnabled"> Indicates whether or not potentially sensitive data (e.g. database values) can be logged. </param>
        /// <param name="comparer">  A <see cref="IComparer{T}" /> for <see cref="IUpdateEntry" />s. </param>
        /// <param name="logger">A <see cref="IDiagnosticsLogger{T}" /> for <see cref="DbLoggerCategory.Update" />s.</param>
        /// <returns>
        ///     New object with <see cref="ModificationCommandBatch" /> interface.
        /// </returns>
        IModificationCommandBuilder CreateModificationCommandBuilder(
            string tableName,
            string? schemaName,
            Func<string> generateParameterName,
            bool sensitiveLoggingEnabled,
            IComparer<IUpdateEntry>? comparer,
            IDiagnosticsLogger<DbLoggerCategory.Update>? logger);
    }
}
