// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerForeignKeyBuilder
    {
        private readonly ForeignKey _foreignKey;

        public SqlServerForeignKeyBuilder([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            _foreignKey = foreignKey;
        }

        public virtual SqlServerForeignKeyBuilder Name([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            _foreignKey.SqlServer().Name = name;

            return this;
        }
    }
}
