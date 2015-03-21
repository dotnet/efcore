// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledModel : CompiledMetadataBase
    {
        private IEntityType[] _entityTypes;

        public IEntityType FindEntityType(Type type) => EntityTypes.FirstOrDefault(e => e.ClrType == type);

        public IEntityType GetEntityType(Type type) => EntityTypes.First(e => e.ClrType == type);

        public IEntityType FindEntityType(string name) => EntityTypes.FirstOrDefault(e => e.Name == name);

        public IEntityType GetEntityType(string name) => EntityTypes.First(e => e.Name == name);

        protected abstract IEntityType[] LoadEntityTypes();

        public IReadOnlyList<IEntityType> EntityTypes => LazyInitializer.EnsureInitialized(ref _entityTypes, LoadEntityTypes);

        public virtual IEnumerable<IForeignKey> GetReferencingForeignKeys(IEntityType entityType) => EntityTypes.SelectMany(et => et.GetForeignKeys()).Where(fk => fk.PrincipalEntityType == entityType);

        public virtual IEnumerable<IForeignKey> GetReferencingForeignKeys(IKey key) => EntityTypes.SelectMany(et => et.GetForeignKeys()).Where(fk => fk.PrincipalKey == key);

        public virtual IEnumerable<IForeignKey> GetReferencingForeignKeys(IProperty property) => EntityTypes.SelectMany(e => e.GetForeignKeys().Where(f => f.ReferencedProperties.Contains(property))).ToArray();
    }
}
