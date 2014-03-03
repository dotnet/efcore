// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class RelationalDataStore : DataStore
    {
        private readonly string _nameOrConnectionString;

        public RelationalDataStore([NotNull] string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            _nameOrConnectionString = nameOrConnectionString;
        }

        public virtual string NameOrConnectionString
        {
            get { return _nameOrConnectionString; }
        }

        public override Task<int> SaveChangesAsync(IEnumerable<EntityEntry> entityEntries)
        {
            // Entities are dependency ordered.

            return base.SaveChangesAsync(entityEntries);
        }
    }
}
