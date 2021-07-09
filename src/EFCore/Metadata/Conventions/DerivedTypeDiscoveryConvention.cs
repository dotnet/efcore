// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that finds derived entity types that are already part of the model based on the associated
    ///     CLR type hierarchy.
    /// </summary>
    [Obsolete]
    public class DerivedTypeDiscoveryConvention : InheritanceDiscoveryConventionBase, IEntityTypeAddedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="DerivedTypeDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public DerivedTypeDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (entityType.HasSharedClrType
                || entityType.HasDefiningNavigation()
                || entityType.IsOwned())
            {
                return;
            }

            var clrType = entityType.ClrType;
            var model = entityType.Model;
            foreach (var directlyDerivedType in model.GetEntityTypes())
            {
                if (directlyDerivedType != entityType
                        && !directlyDerivedType.HasSharedClrType
                        && !directlyDerivedType.HasDefiningNavigation()
                        && !directlyDerivedType.IsOwned()
                        && directlyDerivedType.FindDeclaredOwnership() == null
                        && ((directlyDerivedType.BaseType == null && clrType.IsAssignableFrom(directlyDerivedType.ClrType))
                            || (directlyDerivedType.BaseType == entityType.BaseType && FindClosestBaseType(directlyDerivedType) == entityType)))
                {
                    directlyDerivedType.Builder.HasBaseType(entityType);
                }
            }
        }
    }
}
