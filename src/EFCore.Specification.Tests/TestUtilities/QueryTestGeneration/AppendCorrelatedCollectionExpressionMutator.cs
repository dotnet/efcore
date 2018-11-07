// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class AppendCorrelatedCollectionExpressionMutator : ExpressionMutator
    {
        public AppendCorrelatedCollectionExpressionMutator(DbContext context)
            : base(context)
        {
        }

        private bool ContainsCollectionNavigation(Type type)
            => Context.Model.FindEntityType(type)?.GetNavigations().Any(n => n.IsCollection()) ?? false;

        public override bool IsValid(Expression expression)
            => IsQueryableResult(expression)
            && IsEntityType(expression.Type.GetGenericArguments()[0])
            && ContainsCollectionNavigation(expression.Type.GetGenericArguments()[0]);

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];
            var navigations = Context.Model.FindEntityType(typeArgument).GetNavigations().Where(n => n.IsCollection()).ToList();

            var i = random.Next(navigations.Count);
            var navigation = navigations[i];

            var collectionElementType = navigation.ForeignKey.DeclaringEntityType.ClrType;
            var listType = typeof(List<>).MakeGenericType(collectionElementType);

            var select = SelectMethodInfo.MakeGenericMethod(typeArgument, listType);
            var where = EnumerableWhereMethodInfo.MakeGenericMethod(collectionElementType);
            var toList = ToListMethodInfo.MakeGenericMethod(collectionElementType);

            var outerPrm = Expression.Parameter(typeArgument, "outerPrm");
            var innerPrm = Expression.Parameter(collectionElementType, "innerPrm");

            var outerLambdaBody = Expression.Call(
                toList,
                    Expression.Call(
                    where,
                    Expression.Property(outerPrm, navigation.PropertyInfo),
                    Expression.Lambda(Expression.Constant(true), innerPrm)));

            var resultExpression = Expression.Call(
                select,
                expression,
                Expression.Lambda(outerLambdaBody, outerPrm));

            return resultExpression;
        }
    }
}
