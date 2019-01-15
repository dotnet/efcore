// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryShapedQueryExpression : ShapedQueryExpression
    {
        public InMemoryShapedQueryExpression(IEntityType entityType)
        {
            QueryExpression = new InMemoryQueryExpression(entityType);
            var resultParameter = Parameter(typeof(InMemoryQueryExpression), "result");
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
