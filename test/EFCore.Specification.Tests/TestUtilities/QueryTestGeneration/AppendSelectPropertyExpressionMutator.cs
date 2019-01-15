// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class AppendSelectPropertyExpressionMutator : ExpressionMutator
    {
        private bool HasValidPropertyToSelect(Expression expression)
            => expression.Type.GetGenericArguments()[0].GetProperties().Any(p => !p.GetMethod.IsStatic);

        public AppendSelectPropertyExpressionMutator(DbContext context)
            : base(context)
        {
        }

        public override bool IsValid(Expression expression)
            => IsQueryableResult(expression)
               && HasValidPropertyToSelect(expression);

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];
            var properties = typeArgument.GetProperties().Where(p => !p.GetMethod.IsStatic).ToList();
            properties = FilterPropertyInfos(typeArgument, properties);

            var i = random.Next(properties.Count);

            var select = SelectMethodInfo.MakeGenericMethod(typeArgument, properties[i].PropertyType);
            var prm = Expression.Parameter(typeArgument, "prm");

            var lambdaBody = (Expression)Expression.Property(prm, properties[i]);

            if (properties[i].PropertyType.IsValueType
                && !(properties[i].PropertyType.IsGenericType
                     && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                var nullablePropertyType = typeof(Nullable<>).MakeGenericType(properties[i].PropertyType);
                select = SelectMethodInfo.MakeGenericMethod(typeArgument, nullablePropertyType);
                lambdaBody = Expression.Convert(lambdaBody, nullablePropertyType);
            }

            if (typeArgument == typeof(string))
            {
                // string.Length - make it nullable in case we access optional argument
                select = SelectMethodInfo.MakeGenericMethod(typeArgument, typeof(int?));
                lambdaBody = Expression.Convert(lambdaBody, typeof(int?));
            }

            var lambda = Expression.Lambda(lambdaBody, prm);
            var resultExpression = Expression.Call(select, expression, lambda);

            return resultExpression;
        }
    }
}
