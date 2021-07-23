// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates commands based on raw SQL command text.
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
    public interface IRawSqlCommandBuilder
    {
        /// <summary>
        ///     Creates a new command based on SQL command text.
        /// </summary>
        /// <param name="sql"> The command text. </param>
        /// <returns> The newly created command. </returns>
        IRelationalCommand Build(string sql);

        /// <summary>
        ///     Creates a new command based on SQL command text.
        /// </summary>
        /// <param name="sql"> The command text. </param>
        /// <param name="parameters"> Parameters for the command. </param>
        /// <returns> The newly created command. </returns>
        RawSqlCommand Build(
            string sql,
            IEnumerable<object> parameters);
    }
}
