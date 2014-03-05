// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledEntityType<TEntity> : CompiledMetadataBase
    {
        private IProperty[] _keys;
        private IProperty[] _properties;
        private IDictionary<string, int> _propertyIndexes;

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

        public IReadOnlyList<IProperty> Key
        {
            get { return LazyInitializer.EnsureInitialized(ref _keys, BuildKey); }
        }

        public IReadOnlyList<IForeignKey> ForeignKeys
        {
            // TODO: Implement FKs in the compiled model
            get { return ImmutableList<IForeignKey>.Empty; }
        }

        public IReadOnlyList<INavigation> Navigations
        {
            // TODO: Implement navigations in the compiled model
            get { return ImmutableList<INavigation>.Empty; }
        }

        public IReadOnlyList<IProperty> Properties
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

        public int PropertyIndex([NotNull] string name)
        {
            var indexes = LazyInitializer.EnsureInitialized(ref _propertyIndexes, BuildIndexes);
            int index;
            return indexes.TryGetValue(name, out index) ? index : -1;
        }

        private IDictionary<string, int> BuildIndexes()
        {
            var properties = EnsurePropertiesInitialized();

            _propertyIndexes = new Dictionary<string, int>(properties.Length, StringComparer.Ordinal);
            for (var i = 0; i < _properties.Length; i++)
            {
                _propertyIndexes[_properties[i].Name] = i;
            }

            return _propertyIndexes;
        }
    }
}
