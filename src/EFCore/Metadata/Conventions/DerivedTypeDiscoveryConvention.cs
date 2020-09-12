// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
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
        public DerivedTypeDiscoveryConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
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
            var clrType = entityType.ClrType;
            if (clrType == null
                || entityType.HasSharedClrType
                || entityType.HasDefiningNavigation()
                || entityType.Model.FindIsOwnedConfigurationSource(clrType) != null
                || entityType.FindOwnership() != null)
            {
                return;
            }

            var model = entityType.Model;
            foreach (var directlyDerivedType in model.GetEntityTypes())
            {
                if (directlyDerivedType != entityType
                        && directlyDerivedType.HasClrType()
                        && !directlyDerivedType.HasSharedClrType
                        && !directlyDerivedType.HasDefiningNavigation()
                        && model.FindIsOwnedConfigurationSource(directlyDerivedType.ClrType) == null
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
