// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class SqlServerMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        public override IEnumerable<IAnnotation> For(IKey key)
        {
            if (key.SqlServer().IsClustered.HasValue)
            {
                yield return new Annotation(
                     SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered,
                     key.SqlServer().IsClustered.Value);
            }
        }

        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            if (index.SqlServer().IsClustered.HasValue)
            {
                yield return new Annotation(
                     SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered,
                     index.SqlServer().IsClustered.Value);
            }
        }

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy,
                    SqlServerValueGenerationStrategy.IdentityColumn);
            }
        }
    }
}
