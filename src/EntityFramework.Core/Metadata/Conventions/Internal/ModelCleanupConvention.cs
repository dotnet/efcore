// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class ModelCleanupConvention : IModelConvention
    {
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.RemoveEntityTypesUnreachableByNavigations(ConfigurationSource.DataAnnotation);
            RemoveNavigationlessForeignKeys(modelBuilder);

            return modelBuilder;
        }

        private void RemoveNavigationlessForeignKeys(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.EntityTypes)
            {
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
                {
                    if (foreignKey.PrincipalToDependent == null
                        && foreignKey.DependentToPrincipal == null)
                    {
                        modelBuilder.Entity(entityType.Name, ConfigurationSource.Convention)
                            .RemoveForeignKey(foreignKey, ConfigurationSource.DataAnnotation);
                    }
                }
            }
        }
    }
}
