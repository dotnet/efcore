// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryEntityQueryableExpressionVisitors : EntityQueryableExpressionVisitors
    {
        private readonly IModel _model;

        public InMemoryEntityQueryableExpressionVisitors(IModel model)
        {
            _model = model;
        }

        public override IEnumerable<ExpressionVisitor> GetVisitors()
        {
            yield return new InMemoryEntityQueryableExpressionVisitor2(_model);
        }
    }
}
