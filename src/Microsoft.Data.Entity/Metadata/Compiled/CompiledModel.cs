// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public IEntityType TryGetEntityType([NotNull] string name)
        {
            return EntityTypes.FirstOrDefault(e => e.Name == name);
        }

        public IEntityType GetEntityType([NotNull] string name)
        {
            return EntityTypes.First(e => e.Name == name);
        }

        protected abstract IEntityType[] LoadEntityTypes();

        public IReadOnlyList<IEntityType> EntityTypes
        {
            get { return LazyInitializer.EnsureInitialized(ref _entityTypes, LoadEntityTypes); }
        }
    }
}
