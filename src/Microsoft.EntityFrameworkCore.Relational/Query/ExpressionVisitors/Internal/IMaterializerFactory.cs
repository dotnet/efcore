// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public interface IMaterializerFactory
    {
        Expression<Func<ValueBuffer, object>> CreateMaterializer(
            [NotNull] IEntityType entityType,
            [NotNull] SelectExpression selectExpression,
            [NotNull] Func<IProperty, SelectExpression, int> projectionAdder,
            [CanBeNull] IQuerySource querySource);
    }
}
