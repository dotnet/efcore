// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A ColumnModification factory.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IColumnModificationFactory
    {
        /// <summary>
        ///     Creates a new object with <see cref="IColumnModification" /> interface.
        /// </summary>
        /// <param name="columnModificationParameters"> Creation parameters. </param>
        /// <returns> The new instance with IColumnModification interface. </returns>
        IColumnModification CreateColumnModification(ColumnModificationParameters columnModificationParameters);
    }
}
