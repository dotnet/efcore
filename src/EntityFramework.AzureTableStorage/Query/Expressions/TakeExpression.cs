// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.AzureTableStorage.Query.Expressions
{
    public class TakeExpression : ExtensionExpression
    {
        public TakeExpression([NotNull] Type type, int limit)
            : base(Check.NotNull(type, "type"))
        {
            Limit = limit;
        }

        public virtual int Limit { get; private set; }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
