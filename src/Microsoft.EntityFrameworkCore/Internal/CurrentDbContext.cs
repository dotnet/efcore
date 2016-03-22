// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class CurrentDbContext : ICurrentDbContext
    {
        public CurrentDbContext([NotNull] DbContext context)
        {
            Context = context;
        }

        public virtual DbContext Context { get; }
    }
}
