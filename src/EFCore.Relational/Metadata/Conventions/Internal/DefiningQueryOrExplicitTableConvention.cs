// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DefiningQueryOrExplicitTableConvention : IEntityTypeAnnotationChangedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation Apply(
            InternalEntityTypeBuilder entityTypeBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            var entityType = entityTypeBuilder.Metadata;

            if (entityType.IsQueryType()
                && annotation?.Value != null)
            {
                switch (name)
                {
                    case RelationalAnnotationNames.TableName:
                        entityType.RemoveAnnotation(CoreAnnotationNames.DefiningQuery);
                        break;
                    case CoreAnnotationNames.DefiningQuery:
                        entityType.RemoveAnnotation(RelationalAnnotationNames.TableName);
                        break;
                }
            }

            return annotation;
        }
    }
}
