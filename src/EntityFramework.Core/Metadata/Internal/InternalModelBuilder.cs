// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalModelBuilder : InternalMetadataBuilder<Model>
    {
        private readonly MetadataDictionary<EntityType, InternalEntityTypeBuilder> _entityTypeBuilders =
            new MetadataDictionary<EntityType, InternalEntityTypeBuilder>();

        private readonly LazyRef<Dictionary<string, ConfigurationSource>> _ignoredEntityTypeNames =
            new LazyRef<Dictionary<string, ConfigurationSource>>(() => new Dictionary<string, ConfigurationSource>());
        
        public InternalModelBuilder([NotNull] Model metadata, [NotNull] ConventionSet conventions)
            : base(metadata)
        {
            ConventionDispatcher = new ConventionDispatcher(conventions);
        }

        public override InternalModelBuilder ModelBuilder => this;

        public virtual ConventionDispatcher ConventionDispatcher { get; }

        public virtual InternalEntityTypeBuilder Entity([NotNull] string name, ConfigurationSource configurationSource)
        {
            return IsIgnored(name, configurationSource)
                ? null
                : _entityTypeBuilders.GetOrAdd(
                    () => Metadata.FindEntityType(name),
                    () => Metadata.AddEntityType(name),
                    entityType => new InternalEntityTypeBuilder(entityType, ModelBuilder),
                    OnEntityTypeAdded,
                    configurationSource);
        }

        public virtual InternalEntityTypeBuilder Entity([NotNull] Type type, ConfigurationSource configurationSource)
        {
            return IsIgnored(type.FullName, configurationSource)
                ? null
                : _entityTypeBuilders.GetOrAdd(
                    () => Metadata.FindEntityType(type),
                    () => Metadata.AddEntityType(type),
                    entityType => new InternalEntityTypeBuilder(entityType, ModelBuilder),
                    OnEntityTypeAdded,
                    configurationSource);
        }

        private InternalEntityTypeBuilder OnEntityTypeAdded(InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (_ignoredEntityTypeNames.HasValue)
            {
                _ignoredEntityTypeNames.Value.Remove(entityTypeBuilder.Metadata.Name);
            }

            return ConventionDispatcher.OnEntityTypeAdded(entityTypeBuilder);
        }

        private bool IsIgnored(string name, ConfigurationSource configurationSource)
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                return false;
            }

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredEntityTypeNames.HasValue
                && _ignoredEntityTypeNames.Value.TryGetValue(name, out ignoredConfigurationSource))
            {
                if (ignoredConfigurationSource.Overrides(configurationSource))
                {
                    return true;
                }

                _ignoredEntityTypeNames.Value.Remove(name);
            }

            return false;
        }

        public virtual bool Ignore([NotNull] Type type, ConfigurationSource configurationSource)
            => Ignore(type.FullName, configurationSource);

        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
        {
            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredEntityTypeNames.Value.TryGetValue(name, out ignoredConfigurationSource))
            {
                _ignoredEntityTypeNames.Value[name] = configurationSource.Max(ignoredConfigurationSource);
                return true;
            }

            _ignoredEntityTypeNames.Value[name] = configurationSource;

            var entityType = Metadata.FindEntityType(name);
            if (entityType != null)
            {
                if (!Remove(entityType, configurationSource))
                {
                    _ignoredEntityTypeNames.Value.Remove(name);
                    return false;
                }
            }

            return true;
        }

        private bool Remove(EntityType entityType, ConfigurationSource configurationSource)
        {
            var entityTypeBuilder = _entityTypeBuilders.TryGetValue(entityType, ConfigurationSource.Convention);
            if (!_entityTypeBuilders.Remove(entityType, configurationSource).HasValue)
            {
                return false;
            }

            foreach (var foreignKey in entityType.GetForeignKeys().ToList())
            {
                var removed = entityTypeBuilder.RemoveForeignKey(foreignKey, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            foreach (var foreignKey in Metadata.FindReferencingForeignKeys(entityType).ToList())
            {
                var removed = entityTypeBuilder.RemoveForeignKey(foreignKey, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            Metadata.RemoveEntityType(entityType);

            return true;
        }
        
        public virtual void RemoveEntityTypesUnreachableByNavigations(ConfigurationSource configurationSource)
        {
            foreach (var orphan in new ModelNavigationsGraphAdapter(Metadata).GetUnreachableVertices(GetRoots(configurationSource)))
            {
                Remove(orphan, configurationSource);
            }
        }

        private IReadOnlyList<EntityType> GetRoots(ConfigurationSource configurationSource)
        {
            var roots = new List<EntityType>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var entityType in Metadata.EntityTypes)
            {
                var currentConfigurationSource = _entityTypeBuilders.GetConfigurationSource(entityType);
                if (currentConfigurationSource.Overrides(configurationSource))
                {
                    roots.Add(entityType);
                }
            }

            return roots;
        }

        public virtual InternalModelBuilder Initialize() => ConventionDispatcher.OnModelInitialized(this);

        public virtual InternalModelBuilder Validate() => ConventionDispatcher.OnModelBuilt(this);
    }
}
