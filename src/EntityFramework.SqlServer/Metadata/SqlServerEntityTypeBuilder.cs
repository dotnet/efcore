// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerEntityTypeBuilder
    {
        private readonly EntityType _entityType;

        public SqlServerEntityTypeBuilder([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            _entityType = entityType;
        }

        public virtual SqlServerEntityTypeBuilder Table([CanBeNull] string tableName)
        {
            Check.NullButNotEmpty(tableName, nameof(tableName));

            _entityType.SqlServer().Table = tableName;

            return this;
        }

        public virtual SqlServerEntityTypeBuilder Table([CanBeNull] string tableName, [CanBeNull] string schemaName)
        {
            Check.NullButNotEmpty(tableName, nameof(tableName));
            Check.NullButNotEmpty(schemaName, nameof(schemaName));

            _entityType.SqlServer().Table = tableName;
            _entityType.SqlServer().Schema = schemaName;

            return this;
        }
    }
}
