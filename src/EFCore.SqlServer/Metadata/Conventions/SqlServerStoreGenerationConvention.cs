// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that ensures that properties aren't configured to have a default value, as computed column
    ///     or using a <see cref="SqlServerValueGenerationStrategy" /> at the same time.
    /// </summary>
    public class SqlServerStoreGenerationConvention : StoreGenerationConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerStoreGenerationConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerStoreGenerationConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     Called after an annotation is changed on a property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (annotation == null
                || oldAnnotation?.Value != null)
            {
                return;
            }

            var configurationSource = annotation.GetConfigurationSource();
            var fromDataAnnotation = configurationSource != ConfigurationSource.Convention;
            switch (name)
            {
                case RelationalAnnotationNames.DefaultValue:
                    if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
                        && propertyBuilder.HasDefaultValue(null, fromDataAnnotation) != null)
                    {
                        context.StopProcessing();
                        return;
                    }

                    break;
                case RelationalAnnotationNames.DefaultValueSql:
                    if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
                        && propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) != null)
                    {
                        context.StopProcessing();
                        return;
                    }

                    break;
                case RelationalAnnotationNames.ComputedColumnSql:
                    if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
                        && propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) != null)
                    {
                        context.StopProcessing();
                        return;
                    }

                    break;
                case SqlServerAnnotationNames.ValueGenerationStrategy:
                    if ((propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                            | propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                            | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                        && propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) != null)
                    {
                        context.StopProcessing();
                        return;
                    }

                    break;
            }

            base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
        }

        protected override void Validate(IConventionProperty property)
        {
            if (property.GetValueGenerationStrategyConfigurationSource() != null
                && property.GetValueGenerationStrategy() != SqlServerValueGenerationStrategy.None)
            {
                if (property.GetDefaultValue() != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(
                            "SqlServerValueGenerationStrategy", property.Name, "DefaultValue"));
                }

                if (property.GetDefaultValueSql() != null)
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
