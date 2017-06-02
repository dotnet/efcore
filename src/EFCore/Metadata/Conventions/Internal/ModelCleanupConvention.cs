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
    public class ModelCleanupConvention : IModelBuiltConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.RemoveEntityTypesUnreachableByNavigations(ConfigurationSource.DataAnnotation);
            RemoveNavigationlessForeignKeys(modelBuilder);

            return modelBuilder;
        }

        private static void RemoveNavigationlessForeignKeys(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
                {
                    if (foreignKey.PrincipalToDependent == null
                        && foreignKey.DependentToPrincipal == null)
                    {
                        entityType.Builder.RemoveForeignKey(foreignKey, ConfigurationSource.DataAnnotation);
                    }
                }
            }
        }
    }
}
