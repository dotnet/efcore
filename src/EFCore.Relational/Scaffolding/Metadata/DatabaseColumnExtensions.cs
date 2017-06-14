// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public static class DatabaseColumnExtensions
    {
        public static string GetUnderlyingStoreType([NotNull] this DatabaseColumn column)
            => (string)Check.NotNull(column, nameof(column))[ScaffoldingAnnotationNames.UnderlyingStoreType];

        public static void SetUnderlyingStoreType([NotNull] this DatabaseColumn column, [CanBeNull] string value)
            => Check.NotNull(column, nameof(column))[ScaffoldingAnnotationNames.UnderlyingStoreType] = value;
    }
}
