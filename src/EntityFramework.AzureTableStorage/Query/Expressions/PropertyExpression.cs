// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.AzureTableStorage.Query.Expressions
{
    public class PropertyExpression : ExtensionExpression
    {
        public PropertyExpression([NotNull] IProperty property)
            : base(Check.NotNull(property, "property").PropertyType)
        {
            Check.NotNull(property, "property");

            PropertyName = property.AzureTableStorage().Column;
        }

        public virtual string PropertyName { get; private set; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as TableQueryGenerator;
            if (specificVisitor != null)
            {
                return specificVisitor.VisitPropertyExpression(this);
            }
            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
