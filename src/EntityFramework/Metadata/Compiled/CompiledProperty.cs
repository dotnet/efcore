// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public Type UnderlyingType
        {
            get { return Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty); }
        }

        public virtual ValueGeneration ValueGeneration
        {
            get { return ValueGeneration.None; }
        }

        public bool IsNullable
        {
            get { return typeof(TProperty).IsNullableType(); }
        }

        public bool IsReadOnly
        {
            // TODO:
            get { return false; }
        }

        public int MaxLength
        {
            // TODO:
            get { return 0; }
        }

        public bool UseStoreDefault
        {
            get { return false; }
        }

        public IEntityType EntityType
        {
            get { return _entityType; }
        }

        public bool IsShadowProperty
        {
            get { return false; }
        }

        public bool IsConcurrencyToken
        {
            // TODO:
            get { return true; }
        }

        public int OriginalValueIndex
        {
            // TODO:
            get { return -1; }
        }
    }
}
