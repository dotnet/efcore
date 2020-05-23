// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures database indexes based on the <see cref="IndexAttribute" />.
    /// </summary>
    public class IndexAttributeConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="IndexAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public IndexAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc/>
        public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.ClrType != null)
                {
                    var ignoredMembers = entityType.GetIgnoredMembers();
                    foreach (var indexAttribute in
                        entityType.ClrType.GetCustomAttributes<IndexAttribute>(true))
                    {
                        var shouldGenerate = true;
                        var indexProperties = new List<IConventionProperty>();
                        foreach (var propertyName in indexAttribute.PropertyNames)
                        {
                            if (ignoredMembers.Contains(propertyName))
                            {
                                Dependencies.Logger
                                    .IndexDefinedOnIgnoredProperty(
                                        entityType,
                                        indexAttribute,
                                        propertyName);
                                shouldGenerate = false;
                                break;
                            }

                            var property = entityType.FindProperty(propertyName);
                            if (property == null)
                            {
                                Dependencies.Logger
                                    .IndexDefinedOnNonExistentProperty(
                                        entityType,
                                        indexAttribute,
                                        propertyName);
                                shouldGenerate = false;
                                break;
                            }

                            indexProperties.Add(property);
                        }

                        if (shouldGenerate)
                        {
                            var indexBuilder = entityType.Builder.HasIndex(indexProperties, fromDataAnnotation: true);
                            if (indexBuilder != null)
                            {
                                if (indexAttribute.Name != null)
                                {
                                    indexBuilder.HasName(indexAttribute.Name, fromDataAnnotation: true);
                                }

                                if (indexAttribute.GetIsUnique().HasValue)
                                {
                                    indexBuilder.IsUnique(indexAttribute.GetIsUnique().Value, fromDataAnnotation: true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
