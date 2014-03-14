// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
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

        public override EntityKey Create(IEntityType entityType, IReadOnlyList<IProperty> properties, StateEntry entry)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(properties, "properties");
            Check.NotNull(entry, "entry");

            // TODO: What happens if we get a null property value?
            return new CompositeEntityKey(entityType, properties.Select(entry.GetPropertyValue).ToArray());
        }
    }
}
