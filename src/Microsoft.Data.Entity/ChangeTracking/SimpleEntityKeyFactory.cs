// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class SimpleEntityKeyFactory<TKey> : EntityKeyFactory
    {
        public override EntityKey Create(StateEntry entry)
        {
            var entityType = entry.EntityType;
            return new SimpleEntityKey<TKey>(entityType, (TKey)entry.GetPropertyValue(entityType.Key[0]));
        }
    }
}
