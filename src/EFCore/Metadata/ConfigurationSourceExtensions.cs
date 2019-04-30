// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Provides extension methods for <see cref="ConfigurationSource" />
    /// </summary>
    public static class ConfigurationSourceExtensions
    {
        /// <summary>
        ///     Returns a value indicating whether the new configuration source can override configuration set with the old configuration source.
        /// </summary>
        /// <param name="newConfigurationSource"> The new configuration source. </param>
        /// <param name="oldConfigurationSource"> The old configuration source. </param>
        /// <returns> <c>true</c> if the new configuration source can override configuration set with the old configuration source. </returns>
        [ContractAnnotation("oldConfigurationSource:null => true")]
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
        ///     Returns a value indicating whether the new configuration source can override configuration set with the old configuration source.
        /// </summary>
        /// <param name="newConfigurationSource"> The new configuration source. </param>
        /// <param name="oldConfigurationSource"> The old configuration source. </param>
        /// <returns> <c>true</c> if the new configuration source can override configuration set with the old configuration source. </returns>
        public static bool Overrides(this ConfigurationSource? newConfigurationSource, ConfigurationSource? oldConfigurationSource)
            => newConfigurationSource?.Overrides(oldConfigurationSource) ?? oldConfigurationSource == null;

        /// <summary>
        ///     Returns a value indicating whether the configuration source always takes precedence over the other configuration source.
        /// </summary>
        /// <param name="newConfigurationSource"> The new configuration source. </param>
        /// <param name="oldConfigurationSource"> The old configuration source. </param>
        /// <returns> <c>true</c> if the configuration source always takes precedence over the other configuration source. </returns>
        public static bool OverridesStrictly(this ConfigurationSource newConfigurationSource, ConfigurationSource? oldConfigurationSource)
            => newConfigurationSource.Overrides(oldConfigurationSource) && newConfigurationSource != oldConfigurationSource;

        /// <summary>
        ///     Returns a value indicating whether the configuration source always takes precedence over the other configuration source.
        /// </summary>
        /// <param name="newConfigurationSource"> The new configuration source. </param>
        /// <param name="oldConfigurationSource"> The old configuration source. </param>
        /// <returns> <c>true</c> if the configuration source always takes precedence over the other configuration source. </returns>
        public static bool OverridesStrictly(this ConfigurationSource? newConfigurationSource, ConfigurationSource? oldConfigurationSource)
            => newConfigurationSource.HasValue && newConfigurationSource.Value.OverridesStrictly(oldConfigurationSource);
    }
}
