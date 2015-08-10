// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public interface IMaterializerFactory
    {
        Expression CreateMaterializer(
            [NotNull] IEntityType entityType,
            [NotNull] SelectExpression selectExpression,
            [NotNull] Func<IProperty, SelectExpression, int> projectionAdder,
            [CanBeNull] IQuerySource querySource);
    }
}
