// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public interface IResultOperatorHandler
    {
        Expression HandleResultOperator(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] ResultOperatorBase resultOperator,
            [NotNull] QueryModel queryModel);
    }
}
