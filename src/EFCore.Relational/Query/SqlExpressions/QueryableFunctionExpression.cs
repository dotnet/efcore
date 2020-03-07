// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     Represents a SQL Table Valued Fuction in the sql generation tree.
    /// </summary>
    public class QueryableFunctionExpression : TableExpressionBase
    {
        public QueryableFunctionExpression(
            [CanBeNull] string schema, [NotNull] string name, [NotNull] IReadOnlyList<SqlExpression> arguments, [NotNull] string alias)
            : base(alias)
        {
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));

            Schema = schema;
            Name = name;
            Arguments = arguments;
        }

        public virtual string Schema { get; }
        public virtual string Name { get; }
        public virtual IReadOnlyList<SqlExpression> Arguments { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var arguments = new SqlExpression[Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
                changed |= arguments[i] != Arguments[i];
            }

            return changed
                ? new QueryableFunctionExpression(Schema, Name, arguments, Alias)
                : this;
        }

        public virtual QueryableFunctionExpression Update([NotNull] IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(arguments, nameof(arguments));

            return !arguments.SequenceEqual(Arguments)
                ? new QueryableFunctionExpression(Schema, Name, arguments, Alias)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }

            expressionPrinter.Append(Name);
            expressionPrinter.Append("(");
            expressionPrinter.VisitCollection(Arguments);
            expressionPrinter.Append(") AS ");
            expressionPrinter.Append(Alias);
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is QueryableFunctionExpression queryableExpression
                    && Equals(queryableExpression));

        private bool Equals(QueryableFunctionExpression queryableExpression)
            => base.Equals(queryableExpression)
                && string.Equals(Name, queryableExpression.Name)
                && string.Equals(Schema, queryableExpression.Schema)
                && Arguments.SequenceEqual(queryableExpression.Arguments);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Schema);
            hash.Add(Name);
            for (var i = 0; i < Arguments.Count; i++)
            {
                hash.Add(Arguments[i]);
            }

            return hash.ToHashCode();
        }
    }
}
