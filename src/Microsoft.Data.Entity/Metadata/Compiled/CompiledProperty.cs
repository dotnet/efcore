// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledProperty<TProperty> : CompiledMetadataBase
    {
        private readonly IEntityType _entityType;

        protected CompiledProperty(IEntityType entityType)
        {
            _entityType = entityType;
        }

        public Type PropertyType
        {
            get { return typeof(TProperty); }
        }

        public ValueGenerationStrategy ValueGenerationStrategy
        {
            get { return ValueGenerationStrategy.None; }
        }

        public bool IsNullable
        {
            get { return typeof(TProperty).IsNullableType(); }
        }

        public IEntityType EntityType
        {
            get { return _entityType; }
        }

        public bool IsClrProperty
        {
            get { return true; }
        }
    }
}
