// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;

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
    public interface IRelationalValueBufferFactory
    {
        /// <summary>
        ///     Creates a value buffer for the given <see cref="DbDataReader" />.
        /// </summary>
        /// <param name="dataReader"> The reader to create a value buffer for. </param>
        /// <returns> The newly created value buffer. </returns>
        ValueBuffer Create([NotNull] DbDataReader dataReader);
    }
}
