// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates instances of the <see cref="IRelationalValueBufferFactory" /> type. <see cref="IRelationalValueBufferFactory" />
    ///         instances are tied to a specific result shape. This factory is responsible for creating the
    ///         <see cref="IRelationalValueBufferFactory" /> for a given result shape.
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information.
    /// </remarks>
    public interface IRelationalValueBufferFactoryFactory
    {
        /// <summary>
        ///     Creates a new <see cref="IRelationalValueBufferFactory" />.
        /// </summary>
        /// <param name="types"> Types and mapping for the values to be read. </param>
        /// <returns> The newly created <see cref="IRelationalValueBufferFactoryFactory" />. </returns>
        IRelationalValueBufferFactory Create(IReadOnlyList<TypeMaterializationInfo> types);
    }
}
