// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity
{
    public static class KeyExtensions
    {
        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IKey key)
        {
            Check.NotNull(key, nameof(key));

            return key.DeclaringEntityType.Model.GetEntityTypes().SelectMany(e =>
                e.GetDeclaredForeignKeys()).Where(fk => fk.PrincipalKey == key);
        }
    }
}
