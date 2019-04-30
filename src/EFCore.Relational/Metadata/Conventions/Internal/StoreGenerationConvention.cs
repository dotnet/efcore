// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class StoreGenerationConvention : IPropertyAnnotationChangedConvention, IModelBuiltConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Annotation Apply(
            InternalPropertyBuilder propertyBuilder,
            string name,
            Annotation annotation,
            Annotation oldAnnotation)
        {
            if (annotation == null
                || oldAnnotation?.Value != null)
            {
                return annotation;
            }

            var configurationSource = ((IConventionAnnotation)annotation).GetConfigurationSource();
            var fromDataAnnotation = configurationSource != ConfigurationSource.Convention;
            switch (name)
            {
                case RelationalAnnotationNames.DefaultValue:
                    if (propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                        | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                    {
                        return propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                            ? annotation
                            : null;
                    }

                    break;
                case RelationalAnnotationNames.DefaultValueSql:
                    if (propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                        | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                    {
                        return propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                            ? annotation
                            : null;
                    }

                    break;
                case RelationalAnnotationNames.ComputedColumnSql:
                    if (propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                        | propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null)
                    {
                        return propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null
                            ? annotation
                            : null;
                    }

                    break;
            }

            return annotation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var declaredProperty in entityType.GetDeclaredProperties())
                {
                    Validate(declaredProperty);
                }
            }

            return modelBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void Validate(IConventionProperty property)
        {
            if (property.GetDefaultValue() != null)
            {
                if (property.GetDefaultValueSql() != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration("DefaultValue", property.Name, "DefaultValueSql"));
                }

                if (property.GetComputedColumnSql() != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration("DefaultValue", property.Name, "ComputedColumnSql"));
                }
            }
            else if (property.GetDefaultValueSql() != null)
            {
                if (property.GetComputedColumnSql() != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration("DefaultValueSql", property.Name, "ComputedColumnSql"));
                }
            }
        }
    }
}
