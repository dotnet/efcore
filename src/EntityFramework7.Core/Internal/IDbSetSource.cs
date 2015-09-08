// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    public interface IDbSetSource
    {
        object Create([NotNull] DbContext context, [NotNull] Type type);
    }
}
