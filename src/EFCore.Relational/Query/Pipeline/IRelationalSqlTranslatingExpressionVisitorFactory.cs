// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public interface IRelationalSqlTranslatingExpressionVisitorFactory
    {
        RelationalSqlTranslatingExpressionVisitor Create(
            IModel model,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor);
    }
}
