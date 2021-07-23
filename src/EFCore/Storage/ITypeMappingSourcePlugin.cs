// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents a plugin type mapping source.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface ITypeMappingSourcePlugin
    {
        /// <summary>
        ///     Finds a type mapping for the given info.
        /// </summary>
        /// <param name="mappingInfo"> The mapping info to use to create the mapping. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none could be found. </returns>
        CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo);
    }
}
