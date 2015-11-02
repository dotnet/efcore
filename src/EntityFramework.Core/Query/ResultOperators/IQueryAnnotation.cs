// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ResultOperators
{
    public interface IQueryAnnotation
    {
        IQuerySource QuerySource { get; [param: NotNull] set; }
        QueryModel QueryModel { get; [param: NotNull] set; }
    }
}
