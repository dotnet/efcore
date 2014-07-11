// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class ColumnExpression : ExtensionExpression
    {
        private readonly IProperty _property;
        private readonly string _tableAlias;

        public ColumnExpression([NotNull] IProperty property, [NotNull] string tableAlias)
            : base(Check.NotNull(property, "property").PropertyType)
        {
            Check.NotEmpty(tableAlias, "tableAlias");

            _property = property;
            _tableAlias = tableAlias;
        }

        public virtual string TableAlias
        {
            get { return _tableAlias; }
        }

#pragma warning disable 108
        public virtual IProperty Property
#pragma warning restore 108
        {
            get { return _property; }
        }

        public virtual string Name
        {
            get { return Property.ColumnName(); }
        }

        public virtual string Alias { get; [param: CanBeNull] set; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitColumnExpression(this);
            }

            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }

        public override string ToString()
        {
            var s = _tableAlias + "." + _property.ColumnName();

            if (Alias != null)
            {
                s += " " + Alias;
            }

            return s;
        }
    }
}
