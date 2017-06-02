// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The primary point where a database provider can tell EF that it has been selected for the current context
    ///         and provide the services required for it to function.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        ///     The unique name used to identify the database provider. This should be the same as the NuGet package name
        ///     for the providers runtime.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets a value indicating whether this database provider has been configured for a given context.
        /// </summary>
        /// <param name="options"> The options for the context. </param>
        /// <returns> True if the database provider has been configured, otherwise false. </returns>
        bool IsConfigured([NotNull] IDbContextOptions options);
    }
}
