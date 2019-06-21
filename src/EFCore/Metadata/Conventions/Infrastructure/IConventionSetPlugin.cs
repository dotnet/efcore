// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A service on the EF internal service provider that allows extensions to customize
    ///         the <see cref="ConventionSet" /> being used.
    ///     </para>
    ///     <para>
    ///         Database providers should implement <see cref="IProviderConventionSetBuilder" />. This service
    ///         is intended only for non-provider extensions that need to customize conventions.
    ///     </para>
    ///     <para>
    ///         This type is typically used by extensions. It is generally not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime" /> and multiple registrations
    ///         are allowed. This means that each <see cref="IConventionSetPlugin" /> instance will use its own
    ///         set of instances of this service.
    ///         The implementations may depend on other services registered with any lifetime.
    ///         The implementations do not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IConventionSetPlugin
    {
        /// <summary>
        ///     Called to customize or otherwise modify the given convention set.
        /// </summary>
        /// <param name="conventionSet"> The convention set to customize. </param>
        /// <returns> The customized convention set. </returns>
        ConventionSet ModifyConventions([NotNull] ConventionSet conventionSet);
    }
}
