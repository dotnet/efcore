// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.InMemory
{
    public class InMemoryIdentityGeneratorFactory : DefaultIdentityGeneratorFactory
    {
        public override IIdentityGenerator Create(IProperty property)
        {
            switch (property.ValueGenerationStrategy)
            {
                case ValueGenerationStrategy.Client:
                    if (property.PropertyType == typeof(long))
                    {
                        return new InMemoryIdentityGenerator();
                    }
                    goto default;
                default:
                    return base.Create(property);
            }
        }
    }
}
