// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledModel : CompiledMetadataBase
    {
        private IEntityType[] _entityTypes;

        public IEntityType Entity([NotNull] object instance)
        {
            return Entity(instance.GetType());
        }

        public IEntityType Entity([NotNull] Type type)
        {
            return Entities.FirstOrDefault(e => e.Type == type);
        }

        protected abstract IEntityType[] LoadEntityTypes();

        public IEnumerable<IEntityType> Entities
        {
            get { return LazyInitializer.EnsureInitialized(ref _entityTypes, LoadEntityTypes); }
        }
    }
}
