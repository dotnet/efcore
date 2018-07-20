// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ConfigurationSourceExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool Overrides(this ConfigurationSource newConfigurationSource, ConfigurationSource? oldConfigurationSource)
        {
            if (oldConfigurationSource == null)
            {
                return true;
            }

            if (newConfigurationSource == ConfigurationSource.Explicit)
            {
                return true;
            }

            if (oldConfigurationSource == ConfigurationSource.Explicit)
            {
                return false;
            }

            if (newConfigurationSource == ConfigurationSource.DataAnnotation)
            {
                return true;
            }

            return oldConfigurationSource != ConfigurationSource.DataAnnotation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool Overrides(this ConfigurationSource? newConfigurationSource, ConfigurationSource? oldConfigurationSource)
            => newConfigurationSource?.Overrides(oldConfigurationSource) ?? oldConfigurationSource == null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool OverridesStrictly(this ConfigurationSource newConfigurationSource, ConfigurationSource? oldConfigurationSource)
            => newConfigurationSource.Overrides(oldConfigurationSource) && newConfigurationSource != oldConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool OverridesStrictly(this ConfigurationSource? newConfigurationSource, ConfigurationSource? oldConfigurationSource)
            => newConfigurationSource.HasValue && newConfigurationSource.Value.OverridesStrictly(oldConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [ContractAnnotation("left:notnull => notnull;right:notnull => notnull")]
        public static ConfigurationSource? Max(this ConfigurationSource? left, ConfigurationSource? right)
            => !right.HasValue
               || (left.HasValue
                   && left.Value.Overrides(right.Value))
                ? left
                : right.Value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ConfigurationSource Max(this ConfigurationSource left, ConfigurationSource? right)
            => Max((ConfigurationSource?)left, right).Value;
    }
}
