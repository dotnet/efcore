// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalNavigationIdentityBuilder
    {
        private readonly InternalNavigationBuilder _internalNavigationBuilder;
        private readonly InternalSkipNavigationBuilder _internalSkipNavigationBuilder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalNavigationIdentityBuilder(
            [NotNull] IMutableNavigationBase navigationBase,
            [NotNull] InternalModelBuilder modelBuilder)
        {
            Check.NotNull(navigationBase, nameof(navigationBase));

            Metadata = navigationBase;
            var navigation = Metadata as Navigation;
            if (navigation != null)
            {
                _internalNavigationBuilder = new InternalNavigationBuilder(navigation, modelBuilder);
            }
            else
            {
                var skipNavigation = Metadata as SkipNavigation;
                Check.NotNull(skipNavigation, nameof(skipNavigation));
                _internalSkipNavigationBuilder = new InternalSkipNavigationBuilder(skipNavigation, modelBuilder);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IMutableNavigationBase Metadata { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalNavigationIdentityBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
        {
            if (_internalNavigationBuilder != null)
            {
                _internalNavigationBuilder.UsePropertyAccessMode(propertyAccessMode, configurationSource);
            }
            else
            {
                _internalSkipNavigationBuilder.UsePropertyAccessMode(propertyAccessMode, configurationSource);
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
        {
            return _internalNavigationBuilder != null
                ? _internalNavigationBuilder.CanSetPropertyAccessMode(
                    propertyAccessMode,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : _internalSkipNavigationBuilder.CanSetPropertyAccessMode(
                    propertyAccessMode,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
        }
    }
}
