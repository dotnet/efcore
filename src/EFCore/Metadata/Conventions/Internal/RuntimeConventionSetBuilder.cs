// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
    ///     are allowed. This means that each <see cref="DbContext" /> instance will use its own
    ///     set of instances of this service.
    ///     The implementations may depend on other services registered with any lifetime.
    ///     The implementations do not need to be thread-safe.
    /// </remarks>
    public class RuntimeConventionSetBuilder : IConventionSetBuilder
    {
        private readonly IProviderConventionSetBuilder _conventionSetBuilder;
        private readonly IList<IConventionSetPlugin> _plugins;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RuntimeConventionSetBuilder(
            IProviderConventionSetBuilder providerConventionSetBuilder,
            IEnumerable<IConventionSetPlugin> plugins)
        {
            _conventionSetBuilder = providerConventionSetBuilder;
            _plugins = plugins.ToList();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = _conventionSetBuilder.CreateConventionSet();

            foreach (var plugin in _plugins)
            {
                conventionSet = plugin.ModifyConventions(conventionSet);
            }

            return conventionSet;
        }
    }
}
