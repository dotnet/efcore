// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures database indexes based on the <see cref="IndexAttribute" />.
    /// </summary>
    public class IndexAttributeConvention : IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention, IModelFinalizingConvention
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
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            CheckIndexAttributesAndEnsureIndex(entityTypeBuilder.Metadata, false);
        }

        /// <inheritdoc/>
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (oldBaseType == null)
            {
                return;
            }

            CheckIndexAttributesAndEnsureIndex(entityTypeBuilder.Metadata, false);
        }

        /// <inheritdoc/>
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                CheckIndexAttributesAndEnsureIndex(entityType, true);
            }
        }

        private void CheckIndexAttributesAndEnsureIndex(
            IConventionEntityType entityType,
            bool shouldThrow)
        {
            if (entityType.ClrType != null)
            {
                foreach (var indexAttribute in
                    entityType.ClrType.GetCustomAttributes<IndexAttribute>(true))
                {
                    IConventionIndexBuilder indexBuilder = null;
                    if (!shouldThrow)
                    {
                        var indexProperties = new List<IConventionProperty>();
                        foreach (var propertyName in indexAttribute.PropertyNames)
                        {
                            // TODO Change this to the IsIgnored() which takes
                            // fromDataAnnotation, when bug 21220 is fixed.
                            if (entityType.IsIgnored(propertyName))
                            {
                                return;
                            }

                            var property = entityType.FindProperty(propertyName);
                            if (property == null)
                            {
                                return;
                            }

                            indexProperties.Add(property);
                        }

                        indexBuilder = indexAttribute.Name == null
                           ? entityType.Builder.HasIndex(
                               indexProperties, fromDataAnnotation: true)
                           : entityType.Builder.HasIndex(
                               indexProperties, indexAttribute.Name, fromDataAnnotation: true);
                    }
                    else
                    {
                        // TODO See bug 21220 - we have to do this _before_ calling
                        // HasIndex() because during the call to HasIndex()
                        // IsIgnored (wrongly) returns false and we would end up
                        // creating a property where we shouldn't.
                        CheckIgnoredProperties(indexAttribute, entityType);

                        try
                        {
                            // Using the HasIndex(propertyNames) overload gives us
                            // a chance to create a missing property
                            // e.g. if the CLR property existed but was non-public.
                            indexBuilder = indexAttribute.Name == null
                                ? entityType.Builder.HasIndex(
                                    indexAttribute.PropertyNames, fromDataAnnotation: true)
                                : entityType.Builder.HasIndex(
                                    indexAttribute.PropertyNames, indexAttribute.Name, fromDataAnnotation: true);
                        }
                        catch(InvalidOperationException exception)
                        {
                            CheckMissingProperties(indexAttribute, entityType, exception);

                            throw;
                        }
                    }

                    if (indexBuilder != null
                        && indexAttribute.GetIsUnique().HasValue)
                    {
                        indexBuilder.IsUnique(indexAttribute.GetIsUnique().Value, fromDataAnnotation: true);
                    }
                }
            }
        }

        private void CheckIgnoredProperties(
            IndexAttribute indexAttribute,
            IConventionEntityType entityType)
        {
            foreach (var propertyName in indexAttribute.PropertyNames)
            {
                if (entityType.IsIgnored(propertyName))
                {
                    if (indexAttribute.Name == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.UnnamedIndexDefinedOnIgnoredProperty(
                                entityType.DisplayName(),
                                indexAttribute.PropertyNames.Format(),
                                propertyName));
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NamedIndexDefinedOnIgnoredProperty(
                                indexAttribute.Name,
                                entityType.DisplayName(),
                                indexAttribute.PropertyNames.Format(),
                                propertyName));
                    }
                }
            }
        }

        private void CheckMissingProperties(
            IndexAttribute indexAttribute,
            IConventionEntityType entityType,
            InvalidOperationException innerException)
        {
            foreach (var propertyName in indexAttribute.PropertyNames)
            {
                var property = entityType.FindProperty(propertyName);
                if (property == null)
                {
                    if (indexAttribute.Name == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.UnnamedIndexDefinedOnNonExistentProperty(
                                entityType.DisplayName(),
                                indexAttribute.PropertyNames.Format(),
                                propertyName),
                            innerException);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NamedIndexDefinedOnNonExistentProperty(
                                indexAttribute.Name,
                                entityType.DisplayName(),
                                indexAttribute.PropertyNames.Format(),
                                propertyName),
                            innerException);
                    }
                }
            }
        }
    }
}
