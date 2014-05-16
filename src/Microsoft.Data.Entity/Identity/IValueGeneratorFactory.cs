// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Identity
{
    public interface IValueGeneratorFactory
    {
        IValueGenerator Create([NotNull] IProperty property);
        int GetPoolSize([NotNull] IProperty property);
        string GetCacheKey([NotNull] IProperty property);
    }
}
