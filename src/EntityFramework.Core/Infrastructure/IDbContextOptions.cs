// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IDbContextOptions
    {
        IEnumerable<IDbContextOptionsExtension> Extensions { get; }

        TExtension FindExtension<TExtension>() where TExtension : class, IDbContextOptionsExtension;
    }
}
