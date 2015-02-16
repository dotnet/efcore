// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class SimpleValueGeneratorFactory : ValueGeneratorFactory
    {
        public override int GetPoolSize(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return 1;
        }

        public override string GetCacheKey(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.EntityType.Name + "." + property.Name;
        }
    }
}
