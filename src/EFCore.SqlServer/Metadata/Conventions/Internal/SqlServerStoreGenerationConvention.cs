// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Metadata.Conventions.Internal
{
    public class SqlServerStoreGenerationConvention : StoreGenerationConvention
    {
        public override Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
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
                    if (propertyBuilder.ForSqlServerHasValueGenerationStrategy(null, fromDataAnnotation) == null
                        && propertyBuilder.HasDefaultValue(null, fromDataAnnotation) != null)
                    {
                        return null;
                    }

                    break;
                case RelationalAnnotationNames.DefaultValueSql:
                    if (propertyBuilder.ForSqlServerHasValueGenerationStrategy(null, fromDataAnnotation) == null
                        && propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) != null)
                    {
                        return null;
                    }

                    break;
                case RelationalAnnotationNames.ComputedColumnSql:
                    if (propertyBuilder.ForSqlServerHasValueGenerationStrategy(null, fromDataAnnotation) == null
                        && propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) != null)
                    {
                        return null;
                    }

                    break;
                case SqlServerAnnotationNames.ValueGenerationStrategy:
                    if (propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                        | propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                        | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                    {
                        return propertyBuilder.ForSqlServerHasValueGenerationStrategy(null, fromDataAnnotation) == null
                            ? annotation
                            : null;
                    }

                    break;
            }

            return base.Apply(propertyBuilder, name, annotation, oldAnnotation);
        }

        protected override void Validate(IConventionProperty property)
        {
            if (property.GetSqlServerValueGenerationStrategyConfigurationSource() != null
                && property.GetSqlServerValueGenerationStrategy() != null)
            {
                if (property.GetDefaultValue() != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(
                            "SqlServerValueGenerationStrategy", property.Name, "DefaultValue"));
                }

                if(property.GetDefaultValueSql() != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(
                            "SqlServerValueGenerationStrategy", property.Name, "DefaultValueSql"));
                }

                if (property.GetComputedColumnSql() != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(
                            "SqlServerValueGenerationStrategy", property.Name, "ComputedColumnSql"));
                }
            }

            base.Validate(property);
        }
    }
}
