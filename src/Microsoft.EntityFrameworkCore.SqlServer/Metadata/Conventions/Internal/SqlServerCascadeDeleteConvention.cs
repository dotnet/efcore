// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class SqlServerCascadeDeleteConvention : CascadeDeleteConvention, IEntityTypeAnnotationSetConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation Apply(
            InternalEntityTypeBuilder entityTypeBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == SqlServerFullAnnotationNames.Instance.MemoryOptimized)
            {
                foreach (var foreignKey in entityTypeBuilder.Metadata.GetDerivedForeignKeysInclusive())
                {
                    Apply(foreignKey.Builder);
                }
            }

            return annotation;
        }

        protected override DeleteBehavior TargetDeleteBehavior(ForeignKey foreignKey)
        {
            if (foreignKey.DeclaringEntityType.GetAllBaseTypesInclusive().Any(e => e.SqlServer().IsMemoryOptimized))
            {
                return DeleteBehavior.Restrict;
            }

            return base.TargetDeleteBehavior(foreignKey);
        }
    }
}
