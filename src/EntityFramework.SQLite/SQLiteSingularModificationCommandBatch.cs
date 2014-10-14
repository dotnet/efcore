// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteSingularModificationCommandBatch : SingularModificationCommandBatch
    {
        public SQLiteSingularModificationCommandBatch([NotNull] SqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
        {
            Check.NotNull(property, "property");

            // TODO: SQLite-specific extensions. Issue #875
            return property.Relational();
        }
    }
}
