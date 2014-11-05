// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.InMemory.Metadata
{
    public class EntityTypeNameEqualityComparer : IEqualityComparer<IEntityType>
    {
        public virtual bool Equals(IEntityType x, IEntityType y)
        {
            return StringComparer.Ordinal.Equals(x.Name, y.Name);
        }

        public virtual int GetHashCode(IEntityType obj)
        {
            return StringComparer.Ordinal.GetHashCode(obj.Name);
        }
    }
}
