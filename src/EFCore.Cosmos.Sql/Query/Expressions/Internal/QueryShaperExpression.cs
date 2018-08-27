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
        public QueryShaperExpression(Expression queryExpression, IShaper shaper)
        {
            if (queryExpression.Type.TryGetSequenceType() != typeof(JObject))
            {
                throw new InvalidOperationException("Invalid type");
            }

            QueryExpression = queryExpression;
            Shaper = shaper;
        }

        public override Expression Reduce()
        {
            return Call(
                typeof(QueryShaperExpression).GetTypeInfo().GetDeclaredMethod(nameof(_Shape)).MakeGenericMethod(Shaper.Type),
                QueryExpression,
                Shaper.CreateShaperLambda());
        }

        public Expression QueryExpression { get; }

        public IShaper Shaper { get; }

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
        public override Type Type => typeof(IEnumerable<>).MakeGenericType(Shaper.Type);
        public override ExpressionType NodeType => ExpressionType.Extension;


    }
}
