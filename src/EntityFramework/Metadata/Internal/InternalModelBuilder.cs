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
        private readonly Dictionary<EntityType, InternalEntityBuilder> _entityBuilders = new Dictionary<EntityType, InternalEntityBuilder>();

        public InternalModelBuilder([NotNull] Model metadata, [CanBeNull] IModelChangeListener modelChangeListener)
            : base(metadata)
        {
            _modelChangeListener = modelChangeListener;
        }

        public override InternalModelBuilder ModelBuilder
        {
            get { return this; }
        }

        public virtual InternalEntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            InternalEntityBuilder entityBuilder;
            var entityType = Metadata.TryGetEntityType(name);
            if (entityType == null)
            {
                entityType = new EntityType(name);
                Metadata.AddEntityType(entityType);
                EntityTypeAdded(entityType);
            }
            else
            {
                if (_entityBuilders.TryGetValue(entityType, out entityBuilder))
                {
                    return entityBuilder;
                }
            }

            entityBuilder = new InternalEntityBuilder(entityType, ModelBuilder);
            _entityBuilders.Add(entityType, entityBuilder);
            return entityBuilder;
        }

        public virtual InternalEntityBuilder Entity([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            InternalEntityBuilder entityBuilder;
            var entityType = Metadata.TryGetEntityType(type);
            if (entityType == null)
            {
                entityType = new EntityType(type);
                Metadata.AddEntityType(entityType);
                EntityTypeAdded(entityType);
            }
            else
            {
                if (_entityBuilders.TryGetValue(entityType, out entityBuilder))
                {
                    return entityBuilder;
                }
            }

            entityBuilder = new InternalEntityBuilder(entityType, ModelBuilder);
            _entityBuilders.Add(entityType, entityBuilder);
            return entityBuilder;
        }

        private void EntityTypeAdded(EntityType entityType)
        {
            if (_modelChangeListener != null)
            {
                _modelChangeListener.OnEntityTypeAdded(entityType);
            }
        }
    }
}
