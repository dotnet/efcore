// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ResultOperators;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query.Internal
{
    public interface IQueryOptimizer
    {
        void Optimize(
            [NotNull] IReadOnlyCollection<IQueryAnnotation> queryAnnotations,
            [NotNull] QueryModel queryModel);
    }
}
