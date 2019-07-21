// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Internal
{
    public static class LinqMethodHelpers
    {
        public static MethodInfo AsQueryable { get; }

        public static MethodInfo QueryableWhereMethodInfo { get; }
        public static MethodInfo QueryableSelectMethodInfo { get; }
        public static MethodInfo QueryableOrderByMethodInfo { get; }
        public static MethodInfo QueryableOrderByDescendingMethodInfo { get; }
        public static MethodInfo QueryableThenByMethodInfo { get; }
        public static MethodInfo QueryableThenByDescendingMethodInfo { get; }
        public static MethodInfo QueryableJoinMethodInfo { get; }
        public static MethodInfo QueryableGroupJoinMethodInfo { get; }
        public static MethodInfo QueryableSelectManyMethodInfo { get; }
        public static MethodInfo QueryableSelectManyWithResultOperatorMethodInfo { get; }

        public static MethodInfo QueryableGroupByKeySelector { get; }
        public static MethodInfo QueryableGroupByKeySelectorResultSelector { get; }
        public static MethodInfo QueryableGroupByKeySelectorElementSelector { get; }
        public static MethodInfo QueryableGroupByKeySelectorElementSelectorResultSelector { get; }

        public static MethodInfo QueryableFirstMethodInfo { get; }
        public static MethodInfo QueryableFirstOrDefaultMethodInfo { get; }
        public static MethodInfo QueryableSingleMethodInfo { get; }
        public static MethodInfo QueryableSingleOrDefaultMethodInfo { get; }

        public static MethodInfo QueryableFirstPredicateMethodInfo { get; }
        public static MethodInfo QueryableFirstOrDefaultPredicateMethodInfo { get; }
        public static MethodInfo QueryableSinglePredicateMethodInfo { get; }
        public static MethodInfo QueryableSingleOrDefaultPredicateMethodInfo { get; }

        public static MethodInfo QueryableAnyMethodInfo { get; }
        public static MethodInfo QueryableAnyPredicateMethodInfo { get; }
        public static MethodInfo QueryableAllMethodInfo { get; }
        public static MethodInfo QueryableContainsMethodInfo { get; }

        public static MethodInfo QueryableCountMethodInfo { get; }
        public static MethodInfo QueryableCountPredicateMethodInfo { get; }
        public static MethodInfo QueryableLongCountMethodInfo { get; }
        public static MethodInfo QueryableLongCountPredicateMethodInfo { get; }
        public static MethodInfo QueryableDistinctMethodInfo { get; }
        public static MethodInfo QueryableTakeMethodInfo { get; }
        public static MethodInfo QueryableSkipMethodInfo { get; }

        public static MethodInfo QueryableOfType { get; }

        public static MethodInfo QueryableDefaultIfEmpty { get; }
        public static MethodInfo QueryableDefaultIfEmptyWithDefaultValue { get; }

        public static MethodInfo EnumerableWhereMethodInfo { get; }
        public static MethodInfo EnumerableSelectMethodInfo { get; }

        public static MethodInfo EnumerableJoinMethodInfo { get; }
        public static MethodInfo EnumerableGroupJoinMethodInfo { get; }
        public static MethodInfo EnumerableSelectManyWithResultOperatorMethodInfo { get; }

        public static MethodInfo EnumerableGroupByKeySelector { get; }
        public static MethodInfo EnumerableGroupByKeySelectorResultSelector { get; }
        public static MethodInfo EnumerableGroupByKeySelectorElementSelector { get; }
        public static MethodInfo EnumerableGroupByKeySelectorElementSelectorResultSelector { get; }

        public static MethodInfo EnumerableFirstMethodInfo { get; }
        public static MethodInfo EnumerableFirstOrDefaultMethodInfo { get; }
        public static MethodInfo EnumerableSingleMethodInfo { get; }
        public static MethodInfo EnumerableSingleOrDefaultMethodInfo { get; }

        public static MethodInfo EnumerableFirstPredicateMethodInfo { get; }
        public static MethodInfo EnumerableFirstOrDefaultPredicateMethodInfo { get; }
        public static MethodInfo EnumerableSinglePredicateMethodInfo { get; }
        public static MethodInfo EnumerableSingleOrDefaultPredicateMethodInfo { get; }

        public static MethodInfo EnumerableDefaultIfEmptyMethodInfo { get; }

        public static MethodInfo EnumerableAnyMethodInfo { get; }
        public static MethodInfo EnumerableAnyPredicateMethodInfo { get; }
        public static MethodInfo EnumerableAllMethodInfo { get; }
        public static MethodInfo EnumerableContainsMethodInfo { get; }

        public static MethodInfo EnumerableCountMethodInfo { get; }
        public static MethodInfo EnumerableCountPredicateMethodInfo { get; }
        public static MethodInfo EnumerableLongCountMethodInfo { get; }
        public static MethodInfo EnumerableLongCountPredicateMethodInfo { get; }

        static LinqMethodHelpers()
        {
            var queryableMethods = typeof(Queryable).GetMethods().ToList();
            var enumerableMethods = typeof(Enumerable).GetMethods().ToList();

            AsQueryable = queryableMethods.Single(m => m.Name == nameof(Queryable.AsQueryable) && m.IsGenericMethod);

            QueryableWhereMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Where) && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1));
            QueryableSelectMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Select) && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1));
            QueryableOrderByMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Length == 2);
            QueryableOrderByDescendingMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Length == 2);
            QueryableThenByMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Length == 2);
            QueryableThenByDescendingMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Length == 2);
            QueryableJoinMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Join) && m.GetParameters().Length == 5);
            QueryableGroupJoinMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.GroupJoin) && m.GetParameters().Length == 5);

            QueryableSelectManyMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.SelectMany) && m.GetParameters().Length == 2 && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1));
            QueryableSelectManyWithResultOperatorMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.SelectMany) && m.GetParameters().Length == 3 && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1));

            QueryableGroupByKeySelector = queryableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 2);
            QueryableGroupByKeySelectorResultSelector = queryableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 3 && IsExpressionOfFunc(m.GetParameters()[2].ParameterType, 2));
            QueryableGroupByKeySelectorElementSelector = queryableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 3 && IsExpressionOfFunc(m.GetParameters()[2].ParameterType, 1));
            QueryableGroupByKeySelectorElementSelectorResultSelector = queryableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 4 && IsExpressionOfFunc(m.GetParameters()[2].ParameterType, 1) && IsExpressionOfFunc(m.GetParameters()[3].ParameterType, 2));

            QueryableFirstMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.First) && m.GetParameters().Length == 1);
            QueryableFirstOrDefaultMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Length == 1);
            QueryableSingleMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Single) && m.GetParameters().Length == 1);
            QueryableSingleOrDefaultMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.SingleOrDefault) && m.GetParameters().Length == 1);

            QueryableFirstPredicateMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.First) && m.GetParameters().Length == 2);
            QueryableFirstOrDefaultPredicateMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Length == 2);
            QueryableSinglePredicateMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Single) && m.GetParameters().Length == 2);
            QueryableSingleOrDefaultPredicateMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.SingleOrDefault) && m.GetParameters().Length == 2);

            QueryableCountMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Count) && m.GetParameters().Length == 1);
            QueryableCountPredicateMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Count) && m.GetParameters().Length == 2);
            QueryableLongCountMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.LongCount) && m.GetParameters().Length == 1);
            QueryableLongCountPredicateMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.LongCount) && m.GetParameters().Length == 2);

            QueryableDistinctMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Distinct) && m.GetParameters().Length == 1);
            QueryableTakeMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Take) && m.GetParameters().Length == 2);
            QueryableSkipMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Skip) && m.GetParameters().Length == 2);

            QueryableAnyMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Any) && m.GetParameters().Length == 1);
            QueryableAnyPredicateMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Any) && m.GetParameters().Length == 2);
            QueryableAllMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.All) && m.GetParameters().Length == 2);
            QueryableContainsMethodInfo = queryableMethods.Single(m => m.Name == nameof(Queryable.Contains) && m.GetParameters().Length == 2);

            QueryableOfType = queryableMethods.Single(m => m.Name == nameof(Queryable.OfType) && m.GetParameters().Length == 1);

            QueryableDefaultIfEmpty = queryableMethods.Single(m => m.Name == nameof(Queryable.DefaultIfEmpty) && m.GetParameters().Length == 1);
            QueryableDefaultIfEmptyWithDefaultValue = queryableMethods.Single(m => m.Name == nameof(Queryable.DefaultIfEmpty) && m.GetParameters().Length == 2);

            EnumerableWhereMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Where) && IsFunc(m.GetParameters()[1].ParameterType, 1));
            EnumerableSelectMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Select) && IsFunc(m.GetParameters()[1].ParameterType, 1));

            EnumerableJoinMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Join) && m.GetParameters().Length == 5);
            EnumerableGroupJoinMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.GroupJoin) && m.GetParameters().Length == 5);
            EnumerableSelectManyWithResultOperatorMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.SelectMany) && m.GetParameters().Length == 3 && IsFunc(m.GetParameters()[1].ParameterType, 1));

            EnumerableGroupByKeySelector = enumerableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 2);
            EnumerableGroupByKeySelectorResultSelector = enumerableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 3 && IsFunc(m.GetParameters()[2].ParameterType, 2));
            EnumerableGroupByKeySelectorElementSelector = enumerableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 3 && IsFunc(m.GetParameters()[2].ParameterType, 1));
            EnumerableGroupByKeySelectorElementSelectorResultSelector = enumerableMethods.Single(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Length == 4 && IsFunc(m.GetParameters()[2].ParameterType, 1) && IsFunc(m.GetParameters()[3].ParameterType, 2));

            EnumerableFirstMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.First) && m.GetParameters().Length == 1);
            EnumerableFirstOrDefaultMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.FirstOrDefault) && m.GetParameters().Length == 1);
            EnumerableSingleMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Single) && m.GetParameters().Length == 1);
            EnumerableSingleOrDefaultMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.SingleOrDefault) && m.GetParameters().Length == 1);

            EnumerableFirstPredicateMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.First) && m.GetParameters().Length == 2);
            EnumerableFirstOrDefaultPredicateMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.FirstOrDefault) && m.GetParameters().Length == 2);
            EnumerableSinglePredicateMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Single) && m.GetParameters().Length == 2);
            EnumerableSingleOrDefaultPredicateMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.SingleOrDefault) && m.GetParameters().Length == 2);

            EnumerableDefaultIfEmptyMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.DefaultIfEmpty) && m.GetParameters().Length == 1);

            EnumerableAnyMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 1);
            EnumerableAnyPredicateMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2);
            EnumerableAllMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.All) && m.GetParameters().Length == 2);
            EnumerableContainsMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

            EnumerableCountMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Length == 1);
            EnumerableCountPredicateMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Length == 2);
            EnumerableLongCountMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.LongCount) && m.GetParameters().Length == 1);
            EnumerableLongCountPredicateMethodInfo = enumerableMethods.Single(m => m.Name == nameof(Enumerable.LongCount) && m.GetParameters().Length == 2);
        }

        private static bool IsExpressionOfFunc(Type type, int parameterCount)
            => type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(Expression<>)
            && type.GetGenericArguments()[0] is Type expressionTypeArgument
            && expressionTypeArgument.IsGenericType
            && expressionTypeArgument.Name.StartsWith(nameof(Func<object>))
            && expressionTypeArgument.GetGenericArguments().Length == parameterCount + 1;

        private static bool IsFunc(Type type, int parameterCount)
            => type.IsGenericType
            && type.Name.StartsWith(nameof(Func<object>))
            && type.GetGenericArguments().Length == parameterCount + 1;
    }
}
