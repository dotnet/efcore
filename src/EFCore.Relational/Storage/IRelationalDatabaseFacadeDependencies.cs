// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Exposes dependencies needed by <see cref="DatabaseFacade" /> and its relational extension methods.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
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
