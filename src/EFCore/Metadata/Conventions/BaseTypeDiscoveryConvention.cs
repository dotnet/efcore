// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that finds base and derived entity types that are already part of the model based on the associated
    ///     CLR type hierarchy.
    /// </summary>
    public class BaseTypeDiscoveryConvention :
#pragma warning disable CS0612 // Type or member is obsolete
        InheritanceDiscoveryConventionBase,
#pragma warning restore CS0612 // Type or member is obsolete
        IEntityTypeAddedConvention,
        IForeignKeyOwnershipChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="BaseTypeDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public BaseTypeDiscoveryConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
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
                || entityType.FindDeclaredOwnership() != null)
            {
                return;
            }

            var model = entityType.Model;
            var derivedTypesMap = (Dictionary<Type, List<IConventionEntityType>>)model[CoreAnnotationNames.DerivedTypes];
            if (derivedTypesMap == null)
            {
                derivedTypesMap = new Dictionary<Type, List<IConventionEntityType>>();
                model.SetAnnotation(CoreAnnotationNames.DerivedTypes, derivedTypesMap);
            }

            var baseType = clrType.BaseType;
            if (derivedTypesMap.TryGetValue(clrType, out var derivedTypes))
            {
                foreach (var derivedType in derivedTypes)
                {
                    derivedType.Builder.HasBaseType(entityType);

                    var otherBaseType = baseType;
                    while (otherBaseType != typeof(object))
                    {
                        if (derivedTypesMap.TryGetValue(otherBaseType, out var otherDerivedTypes))
                        {
                            otherDerivedTypes.Remove(derivedType);
                        }

                        otherBaseType = otherBaseType.BaseType;
                    }
                }

                derivedTypesMap.Remove(clrType);
            }

            if (baseType == typeof(object))
            {
                return;
            }

            IConventionEntityType baseEntityType = null;
            while (baseEntityType == null
                && baseType != typeof(object)
                && baseType != null)
            {
                baseEntityType = model.FindEntityType(baseType);
                if (baseEntityType == null)
                {
                    derivedTypesMap.GetOrAddNew(baseType).Add(entityType);
                }

                baseType = baseType.BaseType;
            }

            if (baseEntityType == null)
            {
                return;
            }

            if (!baseEntityType.HasSharedClrType
                && !baseEntityType.HasDefiningNavigation()
                && baseEntityType.FindOwnership() == null)
            {
                entityTypeBuilder = entityTypeBuilder.HasBaseType(baseEntityType);
                if (entityTypeBuilder != null)
                {
                    context.StopProcessingIfChanged(entityTypeBuilder);
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (foreignKey.IsOwnership
                && foreignKey.DeclaringEntityType.GetDirectlyDerivedTypes().Any())
            {
                foreach (var derivedType in foreignKey.DeclaringEntityType.GetDirectlyDerivedTypes().ToList())
                {
                    derivedType.Builder.HasBaseType(null);
                }
            }
        }
    }
}
