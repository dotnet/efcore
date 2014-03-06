// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class CompositeEntityKeyFactory : EntityKeyFactory
    {
        private static readonly CompositeEntityKeyFactory _instance = new CompositeEntityKeyFactory();

        public static CompositeEntityKeyFactory Instance
        {
            get { return _instance; }
        }

        public override EntityKey Create(StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            var entityType = entry.EntityType;
            return new CompositeEntityKey(entityType, entityType.Key.Select(entry.GetPropertyValue).ToArray());
        }
    }
}
