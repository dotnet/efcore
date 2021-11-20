// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates instances of the <see cref="ValueBuffer" /> type. An <see cref="IRelationalValueBufferFactory" />
    ///         is tied to a particular result shape and will only create value buffers for that result shape. Instances
    ///         for different result shapes are created by <see cref="IRelationalValueBufferFactoryFactory" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    public interface IRelationalValueBufferFactory
    {
        /// <summary>
        ///     Creates a value buffer for the given <see cref="DbDataReader" />.
        /// </summary>
        /// <param name="dataReader">The reader to create a value buffer for.</param>
        /// <returns>The newly created value buffer.</returns>
        ValueBuffer Create(DbDataReader dataReader);
    }
}
