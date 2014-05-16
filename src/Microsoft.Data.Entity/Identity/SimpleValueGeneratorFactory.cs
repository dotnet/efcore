// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class SimpleValueGeneratorFactory<TValueGenerator> : IValueGeneratorFactory
        where TValueGenerator : IValueGenerator, new()
    {
        public virtual IValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, "property");

            return new TValueGenerator();
        }

        public virtual int GetPoolSize(IProperty property)
        {
            Check.NotNull(property, "property");

            return 1;
        }

        public virtual string GetCacheKey(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.EntityType.Name + "." + property.Name;
        }
    }
}
