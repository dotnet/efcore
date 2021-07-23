// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Exposes dependencies needed by <see cref="DatabaseFacade" /> and its relational extension methods.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IRelationalDatabaseFacadeDependencies : IDatabaseFacadeDependencies
    {
        /// <summary>
        ///     The relational connection.
        /// </summary>
        IRelationalConnection RelationalConnection { get; }

        /// <summary>
        ///     The raw SQL command builder.
        /// </summary>
        IRawSqlCommandBuilder RawSqlCommandBuilder { get; }

        /// <summary>
        ///     A command logger.
        /// </summary>
        new IRelationalCommandDiagnosticsLogger CommandLogger { get; }
    }
}
