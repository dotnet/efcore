// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class TableExpression : ExtensionExpression
    {
        private readonly string _table;
        private readonly string _alias;

        private readonly IQuerySource _querySource;

        public TableExpression([NotNull] string table, [NotNull] string alias, [NotNull] IQuerySource querySource)
            : base(typeof(object))
        {
            Check.NotEmpty(table, "table");
            Check.NotEmpty(alias, "alias");
            Check.NotNull(querySource, "querySource");

            _table = table;
            _alias = alias;
            _querySource = querySource;
        }

        public virtual string Table
        {
            get { return _table; }
        }

        public virtual string Alias
        {
            get { return _alias; }
        }

        public virtual IQuerySource QuerySource
        {
            get { return _querySource; }
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitTableExpression(this);
            }

            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
