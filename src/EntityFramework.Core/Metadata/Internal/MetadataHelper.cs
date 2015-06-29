// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    internal static class MetadataHelper
    {
        public static void CheckSameEntityType(IReadOnlyList<Property> properties, string argumentName)
        {
            if (properties.Count > 1)
            {
                var entityType = properties[0].EntityType;

                for (var i = 1; i < properties.Count; i++)
                {
                    if (properties[i].EntityType != entityType)
                    {
                        throw new ArgumentException(
                            Strings.InconsistentEntityType(argumentName));
                    }
                }
            }
        }
    }
}
