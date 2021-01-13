// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that removes discriminators from non-TPH entity types and unmaps the inherited properties for TPT entity types.
    /// </summary>
    public class EntityTypeHierarchyMappingConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="EntityTypeHierarchyMappingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public EntityTypeHierarchyMappingConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
        }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var nonTphRoots = new HashSet<IConventionEntityType>();

            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.BaseType == null)
                {
                    continue;
                }

                var tableName = entityType.GetTableName();
                var schema = entityType.GetSchema();
                if (tableName != null
                    && (tableName != entityType.BaseType.GetTableName()
                        || schema != entityType.BaseType.GetSchema()))
                {
                    var pk = entityType.FindPrimaryKey();
                    if (pk != null
                        && !entityType.FindDeclaredForeignKeys(pk.Properties)
                            .Any(fk => fk.PrincipalKey.IsPrimaryKey() && fk.PrincipalEntityType.IsAssignableFrom(entityType)))
                    {
                        entityType.Builder.HasRelationship(entityType.BaseType, pk.Properties, entityType.BaseType.FindPrimaryKey())
                            .IsUnique(true);
                    }

                    nonTphRoots.Add(entityType.GetRootType());
                }

                var viewName = entityType.GetViewName();
                var viewSchema = entityType.GetViewSchema();
                if (viewName != null
                    && (viewName != entityType.BaseType.GetViewName()
                        || viewSchema != entityType.BaseType.GetViewSchema()))
                {
                    nonTphRoots.Add(entityType.GetRootType());
                }
            }

            foreach (var root in nonTphRoots)
            {
                root.Builder.HasNoDiscriminator();
            }
        }
    }
}
