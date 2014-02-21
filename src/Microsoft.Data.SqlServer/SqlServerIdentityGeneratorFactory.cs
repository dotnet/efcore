// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerIdentityGeneratorFactory : DefaultIdentityGeneratorFactory
    {
        public override IIdentityGenerator Create(IProperty property)
        {
            switch (property.ValueGenerationStrategy)
            {
                case ValueGenerationStrategy.Client:
                    if (property.PropertyType == typeof(Guid))
                    {
                        return new SequentialGuidIdentityGenerator();
                    }
                    goto default;

                case ValueGenerationStrategy.StoreSequence:
#if NET45
                    if (property.PropertyType == typeof(long))
                    {
                        return new SequenceIdentityGenerator(new SqlServerSimpleCommandExecutor("TODO: Connection string"));
                    }
#endif
                    goto default;

                default:
                    return base.Create(property);
            }
        }
    }
}
