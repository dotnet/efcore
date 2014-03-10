// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledProperty<TEntity, TProperty> : CompiledMetadataBase
    {
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
            // TODO
            get { return null; }
        }

        public int Index
        {
            // TODO
            get { return 0; }
        }

        public int ShadowIndex
        {
            // TODO
            get { return -1; }
        }

        public bool HasClrProperty
        {
            // TODO
            get { return true; }
        }
    }
}
