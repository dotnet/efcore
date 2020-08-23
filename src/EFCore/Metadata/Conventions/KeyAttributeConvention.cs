// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the entity type key based on the <see cref="KeyAttribute" /> specified on a property.
    /// </summary>
    public class KeyAttributeConvention : PropertyAttributeConventionBase<KeyAttribute>, IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="KeyAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public KeyAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after a property is added to the entity type with an attribute on the associated CLR property or field.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="clrMember"> The member that has the attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected override void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            KeyAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            var entityType = propertyBuilder.Metadata.DeclaringEntityType;
            if (entityType.IsKeyless)
            {
                switch (entityType.GetIsKeylessConfigurationSource())
                {
                    case ConfigurationSource.DataAnnotation:
                        Dependencies.Logger
                            .ConflictingKeylessAndKeyAttributesWarning(propertyBuilder.Metadata);
                        return;

                    case ConfigurationSource.Explicit:
                        // fluent API overrides the attribute - no warning
                        return;
                }
            }

            if (entityType.BaseType != null)
            {
                return;
            }

            if (entityType.IsKeyless
                && entityType.GetIsKeylessConfigurationSource().Overrides(ConfigurationSource.DataAnnotation))
            {
                // TODO: Log a warning that KeyAttribute is being ignored. See issue#20014
                // This code path will also be hit when entity is marked as Keyless explicitly
                return;
            }

            var entityTypeBuilder = entityType.Builder;
            var currentKey = entityTypeBuilder.Metadata.FindPrimaryKey();
            var properties = new List<string> { propertyBuilder.Metadata.Name };

            if (currentKey != null
                && entityType.GetPrimaryKeyConfigurationSource() == ConfigurationSource.DataAnnotation)
            {
                properties.AddRange(
                    currentKey.Properties
                        .Where(p => !p.Name.Equals(propertyBuilder.Metadata.Name, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Name));
                if (properties.Count > 1)
                {
                    properties.Sort(StringComparer.OrdinalIgnoreCase);
                    entityTypeBuilder.HasNoKey(currentKey, fromDataAnnotation: true);
                }
            }

            entityTypeBuilder.PrimaryKey(
                entityTypeBuilder.GetOrCreateProperties(properties, fromDataAnnotation: true), fromDataAnnotation: true);
        }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var entityTypes = modelBuilder.Metadata.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                if (entityType.BaseType == null)
                {
                    var currentPrimaryKey = entityType.FindPrimaryKey();
                    if (currentPrimaryKey?.Properties.Count > 1
                        && entityType.GetPrimaryKeyConfigurationSource() == ConfigurationSource.DataAnnotation)
                    {
                        throw new InvalidOperationException(CoreStrings.CompositePKWithDataAnnotation(entityType.DisplayName()));
                    }
                }
                else
                {
                    foreach (var declaredProperty in entityType.GetDeclaredProperties())
                    {
                        var memberInfo = declaredProperty.GetIdentifyingMemberInfo();

                        if (memberInfo != null
                            && Attribute.IsDefined(memberInfo, typeof(KeyAttribute), inherit: true))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.KeyAttributeOnDerivedEntity(entityType.DisplayName(), declaredProperty.Name));
                        }
                    }
                }
            }
        }
    }
}
