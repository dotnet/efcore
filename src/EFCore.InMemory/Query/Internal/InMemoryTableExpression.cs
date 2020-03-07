// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryTableExpression : Expression, IPrintableExpression
    {
        public InMemoryTableExpression(IEntityType entityType)
        {
            EntityType = entityType;
        }

        public override Type Type => typeof(IEnumerable<ValueBuffer>);

        public virtual IEntityType EntityType { get; }

        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append(nameof(InMemoryTableExpression) + ": Entity: " + EntityType.DisplayName());
        }
    }
}
