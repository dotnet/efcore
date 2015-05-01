// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class KeyExtensions
    {
        public static bool IsPrimaryKey([NotNull] this IKey key)
        {
            Check.NotNull(key, nameof(key));

            return key.EntityType.GetPrimaryKey() == key;
        }
    }
}
