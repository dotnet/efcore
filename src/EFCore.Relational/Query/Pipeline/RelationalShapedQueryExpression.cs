// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryExpression : ShapedQueryExpression
    {
        public RelationalShapedQueryExpression(Expression queryExpression, Expression shaperExpression)
        {
            QueryExpression = queryExpression;
            ShaperExpression = shaperExpression;
        }
    }
}
