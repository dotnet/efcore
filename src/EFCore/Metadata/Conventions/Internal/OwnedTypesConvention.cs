// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OwnedTypesConvention : IEntityTypeRemovedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalModelBuilder modelBuilder, EntityType type)
        {
            if (type.HasDefiningNavigation())
            {
                var entityTypes = modelBuilder.Metadata.GetEntityTypes(type.Name);
                var otherEntityType = entityTypes.FirstOrDefault();
                if (otherEntityType?.HasDefiningNavigation() == true)
                {
                    var ownership = otherEntityType.FindOwnership();
                    if (ownership != null
                        && entityTypes.Count == 1)
                    {
                        using (modelBuilder.Metadata.ConventionDispatcher.StartBatch())
                        {
                            var detachedRelationship = InternalEntityTypeBuilder.DetachRelationship(ownership);

                            var weakSnapshot = InternalEntityTypeBuilder.DetachAllMembers(otherEntityType);
                            modelBuilder.RemoveEntityType(otherEntityType, ConfigurationSource.Explicit);

                            detachedRelationship.WeakEntityTypeSnapshot = weakSnapshot;
                            detachedRelationship.Attach(ownership.PrincipalEntityType.Builder);
                        }
                    }
                }
            }

            return true;
        }
    }
}
