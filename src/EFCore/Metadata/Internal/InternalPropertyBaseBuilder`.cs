// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalPropertyBaseBuilder<TPropertyBase> : InternalModelItemBuilder<TPropertyBase>
        where TPropertyBase : PropertyBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalPropertyBaseBuilder([NotNull] TPropertyBase metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBaseBuilder<TPropertyBase> UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
        {
            if (CanSetPropertyAccessMode(propertyAccessMode, configurationSource))
            {
                Metadata.SetPropertyAccessMode(propertyAccessMode, configurationSource);

                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetPropertyAccessModeConfigurationSource())
                || ((IPropertyBase)Metadata).GetPropertyAccessMode() == propertyAccessMode;
    }
}
