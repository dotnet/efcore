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

        public IEntityType TryGetEntityType([NotNull] Type type)
        {
            return EntityTypes.FirstOrDefault(e => e.Type == type);
        }

        public IEntityType GetEntityType([NotNull] Type type)
        {
            return EntityTypes.First(e => e.Type == type);
        }

        protected abstract IEntityType[] LoadEntityTypes();

        public IEnumerable<IEntityType> EntityTypes
        {
            get { return LazyInitializer.EnsureInitialized(ref _entityTypes, LoadEntityTypes); }
        }

        public virtual IEqualityComparer<object> EntityEqualityComparer
        {
            get { return null; } // TODO
        }
    }
}
