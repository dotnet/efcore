// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalModelBuilder : InternalMetadataBuilder<Model>
    {
        private readonly IModelChangeListener _modelChangeListener;

        public InternalModelBuilder([NotNull] Model metadata, [CanBeNull] IModelChangeListener modelChangeListener)
            : base(metadata)
        {
            _modelChangeListener = modelChangeListener;
        }

        public override InternalModelBuilder ModelBuilder
        {
            get { return this; }
        }

        public virtual InternalEntityBuilder GetOrAddEntity([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            var entityType = Metadata.TryGetEntityType(name);

            if (entityType == null)
            {
                Metadata.AddEntityType(entityType = new EntityType(name));
                EntityTypeAdded(entityType);
            }

            return new InternalEntityBuilder(entityType, ModelBuilder);
        }

        public virtual InternalEntityBuilder GetOrAddEntity([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            var entityType = Metadata.TryGetEntityType(type);

            if (entityType == null)
            {
                Metadata.AddEntityType(entityType = new EntityType(type));
                EntityTypeAdded(entityType);
            }

            return new InternalEntityBuilder(entityType, ModelBuilder);
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
