// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledKey : NoAnnotations
    {
        private readonly IModel _model;

        protected CompiledKey(IModel model)
        {
            _model = model;
        }

        protected abstract KeyDefinition Definition { get; }

        public IEntityType EntityType => _model.EntityTypes[Definition.EntityTypeIndex];

        public IReadOnlyList<IProperty> Properties => Definition.PropertyIndexes.Select(i => EntityType.Properties[i]).ToArray();

        protected struct KeyDefinition
        {
            public short EntityTypeIndex;
            public short[] PropertyIndexes;

            public KeyDefinition(short entityTypeIndex, short[] propertyIndexes)
            {
                EntityTypeIndex = entityTypeIndex;
                PropertyIndexes = propertyIndexes;
            }
        }
    }
}
