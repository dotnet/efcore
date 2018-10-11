// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class KeyAttributeConvention : PropertyAttributeConvention<KeyAttribute>, IModelBuiltConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalPropertyBuilder Apply(
            InternalPropertyBuilder propertyBuilder, KeyAttribute attribute, MemberInfo clrMember)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            var entityType = propertyBuilder.Metadata.DeclaringEntityType;
            if (entityType.BaseType != null)
            {
                return propertyBuilder;
            }

            var entityTypeBuilder = entityType.Builder;
            var currentKey = entityTypeBuilder.Metadata.FindPrimaryKey();
            var properties = new List<string>
            {
                propertyBuilder.Metadata.Name
            };

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
                    entityTypeBuilder.RemoveKey(currentKey, ConfigurationSource.DataAnnotation);
                }
            }

            entityTypeBuilder.PrimaryKey(properties, ConfigurationSource.DataAnnotation);

            return propertyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
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

            return modelBuilder;
        }
    }
}
