// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A service for creating <see cref="IMutableModificationCommand" /> instances.
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
    public interface IMutableModificationCommandFactory
    {
        /// <summary>
        ///     Creates a new database CUD command.
        /// </summary>
        /// <param name="modificationCommandParameters"> The creation parameters. </param>
        /// <returns> A new <see cref="IMutableModificationCommand" /> instance. </returns>
        IMutableModificationCommand CreateModificationCommand(
            ModificationCommandParameters modificationCommandParameters);
    }
}
