// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledIndex : NoAnnotations
    {
        private readonly IModel _model;

        protected CompiledIndex(IModel model)
        {
            _model = model;
        }

        protected abstract IndexDefinition Definition { get; }

        public IEntityType EntityType => _model.EntityTypes[Definition.EntityTypeIndex];

        public IReadOnlyList<IProperty> Properties => Definition.PropertyIndexes.Select(i => EntityType.Properties[i]).ToArray();

        public bool IsUnique => Definition.IsUnique;

        protected struct IndexDefinition
        {
            public short EntityTypeIndex;
            public short[] PropertyIndexes;
            public bool IsUnique;

            public IndexDefinition(short entityTypeIndex, short[] propertyIndexes, bool isUnique)
            {
                EntityTypeIndex = entityTypeIndex;
                PropertyIndexes = propertyIndexes;
                IsUnique = isUnique;
            }
        }
    }
}
