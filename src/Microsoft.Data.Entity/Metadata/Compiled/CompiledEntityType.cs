// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
