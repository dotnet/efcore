// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IRawSqlCommandBuilder
    {
        /// <summary>
        ///     Creates a new command based on SQL command text.
        /// </summary>
        /// <param name="sql"> The command text. </param>
        /// <returns> The newly created command. </returns>
        IRelationalCommand Build([NotNull] string sql);

        /// <summary>
        ///     Creates a new command based on SQL command text.
        /// </summary>
        /// <param name="sql"> The command text. </param>
        /// <param name="parameters"> Parameters for the command. </param>
        /// <returns> The newly created command. </returns>
        RawSqlCommand Build(
            [NotNull] string sql,
            [NotNull] IEnumerable<object> parameters);
    }
}
