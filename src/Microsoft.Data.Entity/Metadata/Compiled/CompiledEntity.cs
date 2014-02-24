// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledEntity<TEntity> : CompiledMetadataBase
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

        protected abstract string[] LoadKeys();
        protected abstract IProperty[] LoadProperties();

        public IEnumerable<IProperty> Key
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _keys,
                    () => LoadKeys().Select(k => Properties.First(p => p.Name == k)).ToArray());
            }
        }

        public IEnumerable<IProperty> Properties
        {
            get { return LazyInitializer.EnsureInitialized(ref _properties, LoadProperties); }
        }
    }
}
