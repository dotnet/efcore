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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class KeyAttributeConvention : PropertyAttributeConvention<KeyAttribute>, IModelFinalizedConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public KeyAttributeConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            : base(logger)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder, KeyAttribute attribute, MemberInfo clrMember, IConventionContext context)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            var entityType = propertyBuilder.Metadata.DeclaringEntityType;
            if (entityType.BaseType != null)
            {
                return;
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
                    entityTypeBuilder.HasNoKey(currentKey, fromDataAnnotation: true);
                }
            }

            entityTypeBuilder.PrimaryKey(
                entityTypeBuilder.GetOrCreateProperties(properties, fromDataAnnotation: true), fromDataAnnotation: true);
        }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
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
