// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationshipValidationConvention : IModelBuiltConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
                {
                    if (foreignKey.IsUnique
                        && foreignKey.GetPrincipalEndConfigurationSource() == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.AmbiguousOneToOneRelationship(
                                foreignKey.DeclaringEntityType.DisplayName() + (foreignKey.DependentToPrincipal == null ? "" : "." + foreignKey.DependentToPrincipal.Name),
                                foreignKey.PrincipalEntityType.DisplayName() + (foreignKey.PrincipalToDependent == null ? "" : "." + foreignKey.PrincipalToDependent.Name)));
                    }
                }
            }

            return modelBuilder;
        }
    }
}
