// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity
{
    public static class MutableKeyExtensions
    {
        public static IEnumerable<IMutableForeignKey> FindReferencingForeignKeys([NotNull] this IMutableKey key)
            => ((IKey)key).FindReferencingForeignKeys().Cast<IMutableForeignKey>();
    }
}
