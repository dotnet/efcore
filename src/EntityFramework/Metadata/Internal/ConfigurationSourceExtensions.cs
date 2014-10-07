// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class ConfigurationSourceExtensions
    {
        public static bool Overrides(this ConfigurationSource newConfiguration, ConfigurationSource oldConfiguration)
        {
            if (newConfiguration == ConfigurationSource.Explicit)
            {
                return true;
            }

            if (oldConfiguration == ConfigurationSource.Explicit)
            {
                return false;
            }

            if (newConfiguration == ConfigurationSource.DataAnnotation)
            {
                return true;
            }

            if (oldConfiguration == ConfigurationSource.DataAnnotation)
            {
                return false;
            }

            return true;
        }
    }
}
