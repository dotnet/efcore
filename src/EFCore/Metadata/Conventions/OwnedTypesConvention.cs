// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures owned entity types with defining navigation as owned entity types
    ///     without defining navigation if there's only one navigation of this type.
    /// </summary>
    public class OwnedTypesConvention : IEntityTypeRemovedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="OwnedTypesConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public OwnedTypesConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an entity type is removed from the model.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="entityType"> The removed entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            IConventionEntityType entityType,
            IConventionContext<IConventionEntityType> context)
        {
            if (!entityType.HasDefiningNavigation())
            {
                return;
            }

            var entityTypes = modelBuilder.Metadata.GetEntityTypes(entityType.Name);
            var otherEntityType = entityTypes.FirstOrDefault();
            if (otherEntityType?.HasDefiningNavigation() == true
                && entityTypes.Count == 1
                && otherEntityType.FindOwnership() is ForeignKey ownership)
            {
                using (context.DelayConventions())
                {
                    InternalEntityTypeBuilder.DetachRelationship(ownership).Attach(ownership.PrincipalEntityType.Builder);
                }
            }
        }
    }
}
