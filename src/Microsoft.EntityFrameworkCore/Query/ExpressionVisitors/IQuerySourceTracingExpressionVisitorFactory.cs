// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public interface IQuerySourceTracingExpressionVisitorFactory
    {
        QuerySourceTracingExpressionVisitor Create();
    }
}
