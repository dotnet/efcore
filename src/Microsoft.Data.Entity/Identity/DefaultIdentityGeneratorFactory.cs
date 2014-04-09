// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Identity
{
    public class DefaultIdentityGeneratorFactory : IdentityGeneratorFactory
    {
        public override IIdentityGenerator Create(IProperty property)
        {
            switch (property.ValueGenerationStrategy)
            {
                case ValueGenerationStrategy.None:
                    return null;
                case ValueGenerationStrategy.StoreIdentity:
                    return null;
                case ValueGenerationStrategy.Client:
                    if (property.PropertyType == typeof(Guid))
                    {
                        return new GuidIdentityGenerator();
                    }
                    goto default;
                default:
                    // TODO: Proper handling of error case.
                    throw new NotSupportedException("No identity generator has been registered for type x and strategy y.");
            }
        }
    }
}
