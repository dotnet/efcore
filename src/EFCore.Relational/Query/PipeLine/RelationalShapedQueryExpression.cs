// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryExpression : ShapedQueryExpression
    {
        public RelationalShapedQueryExpression(IEntityType entityType)
        {
            QueryExpression = new SelectExpression(entityType);
            var resultParameter = Parameter(typeof(SelectExpression), "result");
            ShaperExpression = Lambda(new EntityShaperExpression(
                entityType,
                new ProjectionBindingExpression(
                    QueryExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false),
                resultParameter);
        }
    }
}
