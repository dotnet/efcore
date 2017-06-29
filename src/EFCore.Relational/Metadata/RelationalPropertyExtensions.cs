// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class RelationalPropertyExtensions
    {
        public static bool IsColumnNullable([NotNull] this IProperty property)
        {
            if (property.DeclaringEntityType.BaseType != null
                || property.IsNullable)
            {
                return true;
            }

            if (property.IsPrimaryKey())
            {
                return false;
            }

            var pk = property.DeclaringEntityType.FindPrimaryKey();
            return pk != null
                   && property.DeclaringEntityType.FindForeignKeys(pk.Properties)
                       .Any(fk => fk.PrincipalKey.IsPrimaryKey()
                                  && fk.PrincipalEntityType.BaseType != null
                                  && fk.DeclaringEntityType.Relational().TableName == fk.PrincipalEntityType.Relational().TableName
                                  && fk.DeclaringEntityType.Relational().Schema == fk.PrincipalEntityType.Relational().Schema);
        }
    }
}
