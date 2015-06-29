// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Sqlite.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationAnnotationProvider : MigrationAnnotationProvider
    {
        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.StoreGeneratedPattern == StoreGeneratedPattern.Identity)
            {
                yield return new Annotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement, true);
            }
        }
    }
}
