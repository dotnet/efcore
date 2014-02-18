// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking
{
    internal class SimpleEntityKeyFactory<TEntity, TKey> : EntityKeyFactory
    {
        private readonly IProperty _keyProperty;

        public SimpleEntityKeyFactory(IProperty keyProperty)
        {
            _keyProperty = keyProperty;
        }

        public override EntityKey Create(object entity)
        {
            return new SimpleEntityKey<TEntity, TKey>((TKey)_keyProperty.GetValue(entity));
        }
    }
}
