// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that ensures that properties aren't configured to have a default value and as computed column at the same time.
    /// </summary>
    public class StoreGenerationConvention : IPropertyAnnotationChangedConvention, IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="StoreGenerationConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public StoreGenerationConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an annotation is changed on a property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyAnnotationChanged(
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
                    if ((propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                         | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                        && propertyBuilder.HasDefaultValue(null, fromDataAnnotation) != null)
                    {
                        context.StopProcessing();
                    }

                    break;
                case RelationalAnnotationNames.DefaultValueSql:
                    if ((propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                         | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                        && propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) != null)
                    {
                        context.StopProcessing();
                    }

                    break;
                case RelationalAnnotationNames.ComputedColumnSql:
                    if ((propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                         | propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null)
                        && propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) != null)
                    {
                        context.StopProcessing();
                    }

                    break;
            }
        }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var declaredProperty in entityType.GetDeclaredProperties())
                {
                    Validate(declaredProperty);
                }
            }
        }

        /// <summary>
        ///     Throws if there is conflicting store generation configuration for this property.
        /// </summary>
        /// <param name="property"> The property to check. </param>
        protected virtual void Validate([NotNull] IConventionProperty property)
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
