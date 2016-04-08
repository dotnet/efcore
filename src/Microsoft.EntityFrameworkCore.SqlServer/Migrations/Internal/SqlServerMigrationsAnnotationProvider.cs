// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class SqlServerMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        public override IEnumerable<IAnnotation> For(IKey key)
        {
            var isClustered = key.SqlServer().IsClustered;
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    SqlServerFullAnnotationNames.Instance.Clustered,
                    isClustered.Value);
            }
        }

        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            var isClustered = index.SqlServer().IsClustered;
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    SqlServerFullAnnotationNames.Instance.Clustered,
                    isClustered.Value);
            }
        }

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn)
            {
                yield return new Annotation(
                    SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy,
                    SqlServerValueGenerationStrategy.IdentityColumn);
            }
        }
    }
}
