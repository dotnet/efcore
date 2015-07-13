// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalValueBufferFactoryFactory
    {
        IRelationalValueBufferFactory Create(
            [NotNull] IReadOnlyCollection<Type> valueTypes, [CanBeNull] IReadOnlyList<int> indexMap);
    }
}
