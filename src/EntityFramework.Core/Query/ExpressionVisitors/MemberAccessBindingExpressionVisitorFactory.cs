// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class MemberAccessBindingExpressionVisitorFactory : IMemberAccessBindingExpressionVisitorFactory
    {
        public virtual ExpressionVisitor Create(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
            => new MemberAccessBindingExpressionVisitor(
                Check.NotNull(querySourceMapping, nameof(querySourceMapping)),
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                inProjection);
    }
}
