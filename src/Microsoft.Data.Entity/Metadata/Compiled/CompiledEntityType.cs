// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledEntityType<TEntity> : CompiledMetadataBase
    {
        private IProperty[] _keys;
        private IProperty[] _properties;

        public IProperty Property([NotNull] string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        public Type Type
        {
            get { return typeof(TEntity); }
        }

        protected abstract int[] LoadKey();
        protected abstract IProperty[] LoadProperties();

        public IEnumerable<IProperty> Key
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _keys, BuildKey);
            }
        }

        public IEnumerable<IForeignKey> ForeignKeys
        {
            // TODO: Implement FKs in the compiled model
            get { return Enumerable.Empty<IForeignKey>(); }
        }

        public IEnumerable<IProperty> Properties
        {
            get { return EnsurePropertiesInitialized(); }
        }

        private IProperty[] BuildKey()
        {
            var properties = EnsurePropertiesInitialized();
            return LoadKey().Select(k => properties[k]).ToArray();
        }

        private IProperty[] EnsurePropertiesInitialized()
        {
            return LazyInitializer.EnsureInitialized(ref _properties, LoadProperties);
        }
    }
}
