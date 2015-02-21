// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalForeignKeyBuilder
    {
        private readonly ForeignKey _foreignKey;

        public RelationalForeignKeyBuilder([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            _foreignKey = foreignKey;
        }

        public virtual RelationalForeignKeyBuilder Name([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, "name");

            _foreignKey.Relational().Name = name;

            return this;
        }
    }
}
