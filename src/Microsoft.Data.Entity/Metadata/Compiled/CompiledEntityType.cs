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

        public IProperty TryGetProperty([NotNull] string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        public IProperty GetProperty([NotNull] string name)
        {
            var property = TryGetProperty(name);
            if (property == null)
            {
                throw new Exception(Strings.FormatPropertyNotFound(name, typeof(TEntity).Name));
            }
            return property;
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

        public int ShadowPropertyCount
        {
            // TODO: Implement
            get { return 0; }
        }

        public bool HasClrType
        {
            // TODO: Implement
            get { return true; }
        }

        public object CreateInstance([NotNull] object[] values)
        {
            return null;
        }
    }
}
