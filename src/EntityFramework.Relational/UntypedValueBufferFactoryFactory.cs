// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Relational
{
    public class UntypedValueBufferFactoryFactory : IRelationalValueBufferFactoryFactory
    {
        public virtual IRelationalValueBufferFactory CreateValueBufferFactory(
            IReadOnlyCollection<Type> _, IReadOnlyList<int> indexMap)
            => indexMap == null
                ? (IRelationalValueBufferFactory)new UntypedValueBufferFactory()
                : new RemappingUntypedValueBufferFactory(indexMap);
    }
}
