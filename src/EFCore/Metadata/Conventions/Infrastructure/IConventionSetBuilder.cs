// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A service on the EF internal service provider that creates the <see cref="ConventionSet" />
    ///         that will be used by the current <see cref="DbContext" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IConventionSetBuilder
    {
        /// <summary>
        ///     Builds and returns the convention set to use.
        /// </summary>
        /// <returns> The convention set to use. </returns>
        ConventionSet CreateConventionSet();
    }
}
