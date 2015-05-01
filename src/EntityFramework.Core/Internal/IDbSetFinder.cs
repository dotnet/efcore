// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    public interface IDbSetFinder
    {
        IReadOnlyList<DbSetProperty> FindSets([NotNull] DbContext context);
    }
}
