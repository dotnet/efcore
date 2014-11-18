// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
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

                if (ignoredConfigurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(Strings.EntityIgnoredExplicitly(name));
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
                if (!_entityBuilders.Remove(entityType, configurationSource, canOverrideSameSource: false))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(Strings.EntityAddedExplicitly(name));
                    }

                    return false;
                }

                foreach (ForeignKey foreignKey in Metadata.GetReferencingForeignKeys(entityType).ToList())
                {
                    var removed = RemoveForeignKey(foreignKey, configurationSource);

                    Debug.Assert(removed);
                }

                Metadata.RemoveEntityType(entityType);
            }

            _ignoredEntityTypeNames.Value[name] = configurationSource;

            return true;
        }


        private bool RemoveForeignKey(ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            var entityBuilder = Entity(foreignKey.EntityType.Type, ConfigurationSource.Convention);

            return entityBuilder.RemoveForeignKey(foreignKey, configurationSource);
        }

        public virtual bool IgnoreEntity([NotNull] Type type, ConfigurationSource configurationSource)
        {
            Check.NotNull(type, "type");

            return IgnoreEntity(type.FullName, configurationSource);
        }
    }
}
