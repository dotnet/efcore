// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGeneratorCache : ValueGeneratorCache
    {
        public InMemoryValueGeneratorCache(
            [NotNull] InMemoryValueGeneratorSelector selector, 
            [NotNull] ForeignKeyValueGenerator foreignKeyValueGenerator)
            : base(selector, foreignKeyValueGenerator)
        {
        }
    }
}
