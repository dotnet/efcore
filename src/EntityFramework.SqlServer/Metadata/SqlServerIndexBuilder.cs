// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerIndexBuilder
    {
        private readonly Index _index;

        public SqlServerIndexBuilder([NotNull] Index index)
        {
            Check.NotNull(index, nameof(index));

            _index = index;
        }

        public virtual SqlServerIndexBuilder Name([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            _index.SqlServer().Name = name;

            return this;
        }

        public virtual SqlServerIndexBuilder Clustered(bool isClustered = true)
        {
            _index.SqlServer().IsClustered = isClustered;

            return this;
        }
    }
}
