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
