// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class CollectionNavigation : Navigation
    {
        public CollectionNavigation([NotNull] ForeignKey foreignKey, [NotNull] string name)
            : base(foreignKey, name)
        {
        }

        public override void SetOrAddEntity(object ownerEntity, object relatedEntity)
        {
            Check.NotNull(ownerEntity, "ownerEntity");
            Check.NotNull(relatedEntity, "relatedEntity");

            // TODO: Avoid adding duplicates
            // TODO: Handle o/c mapping mechanisms for patterns other than ICollection
            // TODO: Handle shadow state
            // TODO: Handle nulls/removals
            var collection = EntityType.Type.GetAnyProperty(Name).GetValue(ownerEntity);
            collection.GetType().GetMethod("Add").Invoke(collection, new[] { relatedEntity });
        }
    }
}
