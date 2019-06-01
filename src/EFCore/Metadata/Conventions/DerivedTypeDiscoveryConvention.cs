// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
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
            IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            var clrType = entityType.ClrType;
            if (clrType == null
                || entityType.HasDefiningNavigation())
            {
                return;
            }

            var model = entityType.Model;
            var directlyDerivedTypes = model.GetEntityTypes().Where(
                    t => t != entityType
                         && t.HasClrType()
                         && !t.HasDefiningNavigation()
                         && t.FindDeclaredOwnership() == null
                         && model.FindIsOwnedConfigurationSource(t.ClrType) == null
                         && ((t.BaseType == null && clrType.GetTypeInfo().IsAssignableFrom(t.ClrType.GetTypeInfo()))
                             || (t.BaseType == entityType.BaseType && FindClosestBaseType(t) == entityType)))
                .ToList();

            foreach (var directlyDerivedType in directlyDerivedTypes)
            {
                directlyDerivedType.Builder.HasBaseType(entityType);
            }
        }
    }
}
