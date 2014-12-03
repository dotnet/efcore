// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledEntityType<TEntity> : CompiledMetadataBase
    {
        private readonly IModel _model;
        private IKey _key;
        private IProperty[] _properties;
        private IForeignKey[] _foreignKeys;
        private INavigation[] _navigations;
        private IIndex[] _indexes;

        protected CompiledEntityType(IModel model)
        {
            _model = model;
        }

        public IProperty TryGetProperty(string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        public IProperty GetProperty(string name)
        {
            var property = TryGetProperty(name);
            if (property == null)
            {
                throw new Exception(string.Format("The property '{0}' on entity type '{1}' could not be found. Ensure that the property exists and has been included in the model.",name, typeof(TEntity).Name));
            }
            return property;
        }

        public INavigation TryGetNavigation(string name)
        {
            return Navigations.FirstOrDefault(p => p.Name == name);
        }

        public INavigation GetNavigation(string name)
        {
            var navigation = TryGetNavigation(name);
            if (navigation == null)
            {
                throw new Exception(string.Format("The navigation property '{0}' on entity type '{1}' could not be found. Ensure that the navigation property exists and has been included in the model.", name, typeof(TEntity).Name));
            }
            return navigation;
        }

        public Type Type
        {
            get { return typeof(TEntity); }
        }

        public string SimpleName
        {
            get { return typeof(TEntity).Name; }
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

        protected virtual IIndex[] LoadIndexes()
        {
            return Empty.Indexes;
        }

        public IKey TryGetPrimaryKey()
        {
            if (_key == null)
            {
                Interlocked.CompareExchange(ref _key, LoadKey(), null);
            }

            return _key;
        }

        public IKey GetPrimaryKey()
        {
            return LazyInitializer.EnsureInitialized(ref _key, LoadKey);
        }

        public IReadOnlyList<IKey> Keys
        {
            get { return new[] { GetPrimaryKey() }; }
        }

        public IReadOnlyList<IForeignKey> ForeignKeys
        {
            get { return LazyInitializer.EnsureInitialized(ref _foreignKeys, LoadForeignKeys); }
        }

        public IReadOnlyList<INavigation> Navigations
        {
            get { return LazyInitializer.EnsureInitialized(ref _navigations, LoadNavigations); }
        }

        public IReadOnlyList<IIndex> Indexes
        {
            get { return LazyInitializer.EnsureInitialized(ref _indexes, LoadIndexes); }
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
            // TODO:
            get { return 0; }
        }

        public int OriginalValueCount
        {
            // TODO:
            get { return 0; }
        }

        public bool UseLazyOriginalValues
        {
            // TODO:
            get { return true; }
        }

        public bool HasClrType
        {
            get { return true; }
        }

        public IModel Model
        {
            get { return _model; }
        }
    }
}
