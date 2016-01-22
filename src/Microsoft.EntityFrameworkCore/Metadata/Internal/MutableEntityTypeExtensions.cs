// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class MutableEntityTypeExtensions
    {
        public static IEnumerable<IMutableForeignKey> GetDeclaredForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetDeclaredForeignKeys().Cast<IMutableForeignKey>();
    }
}
