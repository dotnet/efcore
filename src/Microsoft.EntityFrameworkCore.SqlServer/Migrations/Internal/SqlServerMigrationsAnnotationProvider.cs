// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
