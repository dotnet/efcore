// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public static class RelationalModelExtensions
    {
        public static IEnumerable<IEntityType> GetRootEntityTypes([NotNull] this IModel model)
            => model.EntityTypes.Where(e => e.BaseType == null);
    }
}
