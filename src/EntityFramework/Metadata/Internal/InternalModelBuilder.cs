// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalModelBuilder : InternalMetadataBuilder<Model>
    {
        private readonly IModelChangeListener _modelChangeListener;

        private readonly MetadataDictionary<EntityType, InternalEntityBuilder> _entityBuilders =
            new MetadataDictionary<EntityType, InternalEntityBuilder>();

        private readonly LazyRef<Dictionary<string, ConfigurationSource>> _ignoredEntityTypeNames =
            new LazyRef<Dictionary<string, ConfigurationSource>>(() => new Dictionary<string, ConfigurationSource>());

        public InternalModelBuilder([NotNull] Model metadata, [CanBeNull] IModelChangeListener modelChangeListener)
            : base(metadata)
        {
            _modelChangeListener = modelChangeListener;
        }

        public override InternalModelBuilder ModelBuilder
        {
            get { return this; }
        }

        public virtual InternalEntityBuilder Entity([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, "name");

            if (!CanAdd(name, configurationSource))
            {
                return null;
            }

            return _entityBuilders.GetOrAdd(
                () => Metadata.TryGetEntityType(name),
                () => Metadata.AddEntityType(name),
                EntityTypeAdded,
                configurationSource);
        }

        public virtual InternalEntityBuilder Entity([NotNull] Type type, ConfigurationSource configurationSource)
        {
            Check.NotNull(type, "type");

            if (!CanAdd(type.FullName, configurationSource))
            {
                return null;
            }

            return _entityBuilders.GetOrAdd(
                () => Metadata.TryGetEntityType(type),
                () => Metadata.AddEntityType(type),
                EntityTypeAdded,
                configurationSource);
        }

        private bool CanAdd(string name, ConfigurationSource configurationSource)
        {
            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredEntityTypeNames.HasValue
                && _ignoredEntityTypeNames.Value.TryGetValue(name, out ignoredConfigurationSource))
            {
                if (!configurationSource.Overrides(ignoredConfigurationSource))
                {
                    return false;
                }

                _ignoredEntityTypeNames.Value.Remove(name);
            }

            return true;
        }

        private InternalEntityBuilder EntityTypeAdded(EntityType entityType, bool isNew)
        {
            var builder = new InternalEntityBuilder(entityType, ModelBuilder);
            if (isNew 
                && _modelChangeListener != null)
            {
                _modelChangeListener.OnEntityTypeAdded(builder);
            }

            return builder;
        }

        public virtual bool IgnoreEntity([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, "name");

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredEntityTypeNames.Value.TryGetValue(name, out ignoredConfigurationSource))
            {
                if (!configurationSource.Overrides(ignoredConfigurationSource)
                    || configurationSource == ignoredConfigurationSource)
                {
                    return true;
                }
            }

            var entityType = Metadata.TryGetEntityType(name);
            if (entityType != null)
            {
                if (!_entityBuilders.Remove(entityType, configurationSource))
                {
                    return false;
                }

                Metadata.RemoveEntityType(entityType);
            }

            _ignoredEntityTypeNames.Value[name] = configurationSource;

            return true;
        }

        public virtual bool IgnoreEntity([NotNull] Type type, ConfigurationSource configurationSource)
        {
            Check.NotNull(type, "type");

            return IgnoreEntity(type.FullName, configurationSource);
        }
    }
}
