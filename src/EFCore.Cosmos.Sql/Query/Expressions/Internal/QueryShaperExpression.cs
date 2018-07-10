// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal
{
    public class QueryShaperExpression : Expression
    {
        private readonly Expression _queryExpression;
        private readonly IShaper _shaper;

        public QueryShaperExpression(Expression queryExpression, IShaper shaper)
        {
            _queryExpression = queryExpression;
            _shaper = shaper;
        }

        public override Expression Reduce()
        {
            return Call(
                typeof(QueryShaperExpression).GetTypeInfo().GetDeclaredMethod(nameof(_Shape)).MakeGenericMethod(_shaper.Type),
                _queryExpression,
                _shaper.CreateShaperLambda());
        }

        public Expression QueryExpression => _queryExpression;

        private static IEnumerable<T> _Shape<T>(
            IEnumerable<JObject> innerEnumerable,
            Func<JObject, T> shaper)
        {
            foreach (var jObject in innerEnumerable)
            {
                yield return shaper(jObject);
            }
        }

        public override bool CanReduce => true;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        public override Type Type => typeof(IEnumerable<>).MakeGenericType(_shaper.Type);
        public override ExpressionType NodeType => ExpressionType.Extension;
    }
}
