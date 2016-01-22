// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class ConfigurationSourceExtensions
    {
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

            if (oldConfigurationSource == ConfigurationSource.DataAnnotation)
            {
                return false;
            }

            return true;
        }

        public static ConfigurationSource Max(this ConfigurationSource left, ConfigurationSource? right)
        {
            if (!right.HasValue
                || left.Overrides(right.Value))
            {
                return left;
            }

            return right.Value;
        }
    }
}
