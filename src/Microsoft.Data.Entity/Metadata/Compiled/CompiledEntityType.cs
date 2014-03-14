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
        private readonly IModel _model;
        private IKey _key;
        private IProperty[] _properties;
        private IForeignKey[] _foreignKeys;
        private INavigation[] _navigations;

        protected CompiledEntityType(IModel model)
        {
            _model = model;
        }

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

        protected abstract IKey LoadKey();
        
        protected abstract IProperty[] LoadProperties();

        protected virtual IForeignKey[] LoadForeignKeys()
        {
            return Empty.ForeignKeys;
        }

        protected virtual INavigation[] LoadNavigations()
        {
            return Empty.Navigations;
        }

        public IKey GetKey()
        {
            return LazyInitializer.EnsureInitialized(ref _key, LoadKey);
        }

        public IReadOnlyList<IForeignKey> ForeignKeys
        {
            get { return LazyInitializer.EnsureInitialized(ref _foreignKeys, LoadForeignKeys); }
        }

        public IReadOnlyList<INavigation> Navigations
        {
            get { return LazyInitializer.EnsureInitialized(ref _navigations, LoadNavigations); }
        }

        public IReadOnlyList<IProperty> Properties
        {
            get { return EnsurePropertiesInitialized(); }
        }

        private IProperty[] EnsurePropertiesInitialized()
        {
            return LazyInitializer.EnsureInitialized(ref _properties, LoadProperties);
        }

        public int ShadowPropertyCount
        {
            get { return 0; }
        }

        public bool HasClrType
        {
            get { return true; }
        }

        public object CreateInstance([NotNull] object[] values)
        {
            return null;
        }

        public IModel Model
        {
            get { return _model; }
        }
    }
}
