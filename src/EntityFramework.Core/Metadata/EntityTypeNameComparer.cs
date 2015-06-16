// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityTypeNameComparer : IComparer<EntityType>
    {
        public virtual int Compare(EntityType x, EntityType y)
            => StringComparer.Ordinal.Compare(x.Name, y.Name);
    }
}
