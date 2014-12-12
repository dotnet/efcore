// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public static class PropertyBagEntryExtensions
    {
        public static bool HasDefaultValue([NotNull] this IPropertyBagEntry entry, [NotNull] IProperty property)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(property, "property");

            return property.PropertyType.IsDefaultValue(entry[property]);
        }
    }
}
