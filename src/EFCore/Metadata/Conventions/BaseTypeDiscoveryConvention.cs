// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Collections.Generic;
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
        IForeignKeyRemovedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="BaseTypeDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public BaseTypeDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (entityType.HasSharedClrType
                || entityType.IsOwned())
            {
                return;
            }

            Check.DebugAssert(entityType.GetDeclaredForeignKeys().FirstOrDefault(fk => fk.IsOwnership) == null,
                "Ownerships present on non-owned entity type");
            ProcessEntityType(entityTypeBuilder, context);
        }

        private static void ProcessEntityType(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext context)
        {
            var entityType = entityTypeBuilder.Metadata;
            var model = entityType.Model;
            var derivedTypesMap = (Dictionary<Type, List<IConventionEntityType>>?)model[CoreAnnotationNames.DerivedTypes];
            if (derivedTypesMap == null)
            {
                derivedTypesMap = new Dictionary<Type, List<IConventionEntityType>>();
                model.SetAnnotation(CoreAnnotationNames.DerivedTypes, derivedTypesMap);
            }

            var clrType = entityType.ClrType;
            var baseType = clrType.BaseType!;
            if (derivedTypesMap.TryGetValue(clrType, out var derivedTypes))
            {
                foreach (var derivedType in derivedTypes)
                {
                    if (!derivedType.IsOwned())
                    {
                        derivedType.Builder.HasBaseType(entityType);
                    }

                    var otherBaseType = baseType;
                    while (otherBaseType != typeof(object))
                    {
                        if (derivedTypesMap.TryGetValue(otherBaseType, out var otherDerivedTypes))
                        {
                            otherDerivedTypes.Remove(derivedType);
                        }

                        otherBaseType = otherBaseType.BaseType!;
                    }
                }

                derivedTypesMap.Remove(clrType);
            }

            if (baseType == typeof(object))
            {
                return;
            }

            IConventionEntityType? baseEntityType = null;
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
                && !baseEntityType.IsOwned())
            {
                entityTypeBuilder.HasBaseType(baseEntityType);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionForeignKey foreignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            if (foreignKey.IsOwnership
                && !entityTypeBuilder.Metadata.IsOwned())
            {
                ProcessEntityType(entityTypeBuilder, context);
            }
        }
    }
}
