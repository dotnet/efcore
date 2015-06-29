// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.SqlServer.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerMigrationAnnotationProvider : MigrationAnnotationProvider
    {
        public override IEnumerable<IAnnotation> For(IKey key)
            => key.Annotations.Where(a => a.Name == SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered);

        public override IEnumerable<IAnnotation> For(IIndex index)
            => index.Annotations.Where(a => a.Name == SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered);

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            var annotations = property.Annotations.Where(
                a => a.Name == SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression);
            foreach (var annotation in annotations)
            {
                yield return annotation;
            }

            if (GetIdentityStrategy(property) == SqlServerIdentityStrategy.IdentityColumn)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ItentityStrategy,
                    SqlServerIdentityStrategy.IdentityColumn.ToString());
            }
        }

        // TODO: Move to metadata API?
        private static SqlServerIdentityStrategy? GetIdentityStrategy(IProperty property)
            => property.StoreGeneratedPattern == StoreGeneratedPattern.Identity
               && property.SqlServer().DefaultValueSql == null
               && property.SqlServer().DefaultValue == null
               && property.SqlServer().ComputedExpression == null
                ? property.SqlServer().IdentityStrategy
                : null;
    }
}
