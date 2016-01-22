// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class RelationalPropertyExtensions
    {
        public static bool IsColumnNullable([NotNull] this IProperty property)
            => (property.DeclaringEntityType.BaseType != null) || property.IsNullable;
    }
}
