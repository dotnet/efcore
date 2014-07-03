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
    public class SelectExpression : ExtensionExpression
    {
        public SelectExpression([NotNull] Type type)
            : base(Check.NotNull(type, "type"))
        {
        }

        public virtual TakeExpression Take { get; [param: NotNull] set; }
        public virtual Expression Predicate { get; [param: NotNull] set; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as TableQueryGenerator;
            if (specificVisitor != null)
            {
                return specificVisitor.VisitSelectExpression(this);
            }
            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            Predicate = visitor.VisitExpression(Predicate);
            return this;
        }
    }
}
