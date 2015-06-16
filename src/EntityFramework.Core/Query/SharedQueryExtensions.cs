// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public static class SharedQueryExtensions
    {
        public static bool HasGeneratedItemName([NotNull] this IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotEmpty(querySource.ItemName, nameof(querySource.ItemName));

            return querySource.ItemName.StartsWith("<generated>_");
        }
    }
}
