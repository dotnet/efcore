// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates instances of the <see cref="IRelationalCommandBuilder" /> class.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalCommandBuilderFactory
    {
        /// <summary>
        ///     Creates a new <see cref="IRelationalCommandBuilder" />.
        /// </summary>
        /// <returns> The newly created builder. </returns>
        IRelationalCommandBuilder Create();
    }
}
