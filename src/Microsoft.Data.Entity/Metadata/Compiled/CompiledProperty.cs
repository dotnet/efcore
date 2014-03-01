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

        public Type DeclaringType
        {
            get { return typeof(TEntity); }
        }

        public ValueGenerationStrategy ValueGenerationStrategy
        {
            get { return ValueGenerationStrategy.None; }
        }

        public bool IsNullable
        {
            get { return typeof(TProperty).IsNullableType(); }
        }
    }
}
