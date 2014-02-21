// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking
{
    internal class EntityKeyFactoryFactory
    {
        public virtual EntityKeyFactory Create(IEntityType entityType)
        {
            var keyDefinition = entityType.Key;
            var partCount = keyDefinition.Count();

            if (partCount == 1)
            {
                var keyProperty = keyDefinition.First();
                return (EntityKeyFactory)Activator.CreateInstance(
                    typeof(SimpleEntityKeyFactory<,>).MakeGenericType(entityType.Type, keyProperty.PropertyType),
                    keyProperty);
            }

            // TODO: Implement composite keys
            throw new NotImplementedException("Composite keys");
        }
    }
}
