// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class RelationalPropertyAnnotationsExtensions
    {
        public static bool HasStoreDefault([NotNull] this IRelationalPropertyAnnotations property)
            => Check.NotNull(property, nameof(property)).DefaultValueSql != null
               || property.DefaultValue != null;
    }
}
