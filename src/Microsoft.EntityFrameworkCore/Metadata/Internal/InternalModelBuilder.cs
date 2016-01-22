// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalModelBuilder : InternalMetadataBuilder<Model>
    {
        public InternalModelBuilder([NotNull] Model metadata)
            : base(metadata)
        {
        }

        public override InternalModelBuilder ModelBuilder => this;

        public virtual InternalEntityTypeBuilder Entity([NotNull] string name, ConfigurationSource configurationSource)
        {
            if (IsIgnored(name, configurationSource))
            {
                return null;
            }

            var entityType = Metadata.FindEntityType(name);
            if (entityType == null)
            {
                Metadata.Unignore(name);

                entityType = Metadata.AddEntityType(name, configurationSource);
            }
            else
            {
                entityType.UpdateConfigurationSource(configurationSource);
            }

            return entityType?.Builder;
        }

        public virtual InternalEntityTypeBuilder Entity([NotNull] Type type, ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            var entityType = Metadata.FindEntityType(type);
            if (entityType == null)
            {
                Metadata.Unignore(type);

                entityType = Metadata.AddEntityType(type, configurationSource);
            }
            else
            {
                entityType.UpdateConfigurationSource(configurationSource);
            }

            return entityType?.Builder;
        }

        public virtual bool IsIgnored([NotNull] Type type, ConfigurationSource configurationSource)
            => IsIgnored(type.DisplayName(), configurationSource);

        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource configurationSource)
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                return false;
            }

            var ignoredConfigurationSource = Metadata.FindIgnoredEntityTypeConfigurationSource(name);
            return ignoredConfigurationSource.HasValue
                   && ignoredConfigurationSource.Value.Overrides(configurationSource);
        }

        public virtual bool Ignore([NotNull] Type type, ConfigurationSource configurationSource)
            => Ignore(type.DisplayName(), configurationSource);

        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
        {
            var ignoredConfigurationSource = Metadata.FindIgnoredEntityTypeConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue)
            {
                if (configurationSource.Overrides(ignoredConfigurationSource)
                    && (configurationSource != ignoredConfigurationSource))
                {
                    Metadata.Ignore(name, configurationSource);
                }
                return true;
            }

            var entityType = Metadata.FindEntityType(name);
            if (entityType == null)
            {
                Metadata.Ignore(name, configurationSource);
                return true;
            }

            return Ignore(entityType, configurationSource);
        }

        private bool Ignore(EntityType entityType, ConfigurationSource configurationSource)
        {
            var entityTypeConfigurationSource = entityType.GetConfigurationSource();
            if (!configurationSource.Overrides(entityTypeConfigurationSource))
            {
                return false;
            }

            // Set base type as null to remove the entityType from directly derived types of the base type
            var baseType = entityType.BaseType;
            entityType.Builder.HasBaseType((EntityType)null, configurationSource);

            Metadata.Ignore(entityType.Name, configurationSource);

            var entityTypeBuilder = entityType.Builder;
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
            {
                var removed = entityTypeBuilder.RemoveForeignKey(foreignKey, configurationSource, runConventions: false);
                Debug.Assert(removed.HasValue);
            }

            foreach (var foreignKey in entityType.GetDeclaredReferencingForeignKeys().ToList())
            {
                var removed = foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            foreach (var directlyDerivedType in entityType.GetDirectlyDerivedTypes().ToList())
            {
                var derivedEntityTypeBuilder = directlyDerivedType.Builder
                    .HasBaseType(baseType, configurationSource);
                Debug.Assert(derivedEntityTypeBuilder != null);
            }

            Metadata.RemoveEntityType(entityType.Name);

            return true;
        }

        public virtual void RemoveEntityTypesUnreachableByNavigations(ConfigurationSource configurationSource)
        {
            var rootEntityTypes = GetRoots(configurationSource);
            foreach (var orphan in new ModelNavigationsGraphAdapter(Metadata).GetUnreachableVertices(rootEntityTypes))
            {
                // Ignoring the type prevents it from being rediscovered by conventions that run as part of the removal
                Ignore(orphan, configurationSource);
            }
        }

        private IReadOnlyList<EntityType> GetRoots(ConfigurationSource configurationSource)
        {
            var roots = new List<EntityType>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var entityType in Metadata.GetEntityTypes())
            {
                var currentConfigurationSource = entityType.GetConfigurationSource();
                if (currentConfigurationSource.Overrides(configurationSource))
                {
                    roots.Add(entityType);
                }
            }

            return roots;
        }

        public virtual InternalModelBuilder Validate() => Metadata.Validate();
    }
}
