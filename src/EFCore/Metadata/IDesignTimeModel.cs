// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     The metadata about the shape of entities, the relationships between them, and how they map to the database.
///     Also includes all the information necessary to initialize the database.
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
public interface IDesignTimeModel
{
    /// <summary>
    ///     Gets the metadata about the shape of entities, the relationships between them, and how they map to the database.
    ///     Also includes all the information necessary to initialize the database.
    /// </summary>
    public IModel Model { get; }
}
