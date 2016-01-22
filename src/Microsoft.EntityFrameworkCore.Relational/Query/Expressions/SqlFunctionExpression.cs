// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    [DebuggerDisplay("{this.FunctionName}({string.Join(\", \", this.Arguments)})")]
    public class SqlFunctionExpression : Expression
    {
        private readonly ReadOnlyCollection<Expression> _arguments;

        public SqlFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType)
            : this(functionName, returnType, Enumerable.Empty<Expression>())
        {
        }

        public SqlFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> arguments)
        {
            FunctionName = functionName;
            Type = returnType;
            _arguments = arguments.ToList().AsReadOnly();
        }

        public virtual string FunctionName { get; [param: NotNull] set; }

        public virtual IReadOnlyCollection<Expression> Arguments => _arguments;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSqlFunction(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newArguments = visitor.VisitAndConvert(_arguments, "VisitChildren");

            return newArguments != _arguments
                ? new SqlFunctionExpression(FunctionName, Type, newArguments)
                : this;
        }
    }
}
