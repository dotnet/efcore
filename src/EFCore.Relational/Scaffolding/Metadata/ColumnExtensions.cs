// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public static class ColumnExtensions
    {
        public static string GetUnderlyingStoreType([NotNull] this Column column)
            => (string)Check.NotNull(column, nameof(column))[ScaffoldingAnnotationNames.UnderlyingStoreType];

        public static void SetUnderlyingStoreType([NotNull] this Column column, [CanBeNull] string value)
            => Check.NotNull(column, nameof(column))[ScaffoldingAnnotationNames.UnderlyingStoreType] = value;
    }
}
