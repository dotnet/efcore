// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A service for creating <see cref="IModificationCommand" /> instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IModificationCommandFactory
    {
        /// <summary>
        ///     Creates a new object with <see cref="IModificationCommand" /> interface.
        /// </summary>
        /// <param name="modificationCommandParameters"> Creation parameters. </param>
        /// <returns> The new instance. </returns>
        IModificationCommand CreateModificationCommand(
            ModificationCommandParameters modificationCommandParameters);
    }
}
