// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryExpressionBase : Expression
    {
        public override abstract Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;
        protected override abstract Expression VisitChildren(ExpressionVisitor visitor);
        public override abstract bool Equals(object obj);
        public override abstract int GetHashCode();
        public override abstract string ToString();

    }
}
