// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public static class LinqMethodHelpers
    {
        public static MethodInfo QueryableWhereMethodInfo { get; private set; }
        public static MethodInfo QueryableSelectMethodInfo { get; private set; }
        public static MethodInfo QueryableOrderByMethodInfo { get; private set; }
        public static MethodInfo QueryableOrderByDescendingMethodInfo { get; private set; }
        public static MethodInfo QueryableThenByMethodInfo { get; private set; }
        public static MethodInfo QueryableThenByDescendingMethodInfo { get; private set; }
        public static MethodInfo QueryableJoinMethodInfo { get; private set; }
        public static MethodInfo QueryableGroupJoinMethodInfo { get; private set; }
        public static MethodInfo QueryableSelectManyMethodInfo { get; private set; }
        public static MethodInfo QueryableSelectManyWithResultOperatorMethodInfo { get; private set; }

        public static MethodInfo QueryableGroupByKeySelector { get; private set; }
        public static MethodInfo QueryableGroupByKeySelectorResultSelector { get; private set; }
        public static MethodInfo QueryableGroupByKeySelectorElementSelector { get; private set; }
        public static MethodInfo QueryableGroupByKeySelectorElementSelectorResultSelector { get; private set; }

        public static MethodInfo QueryableFirstMethodInfo { get; private set; }
        public static MethodInfo QueryableFirstOrDefaultMethodInfo { get; private set; }
        public static MethodInfo QueryableSingleMethodInfo { get; private set; }
        public static MethodInfo QueryableSingleOrDefaultMethodInfo { get; private set; }

        public static MethodInfo QueryableFirstPredicateMethodInfo { get; private set; }
        public static MethodInfo QueryableFirstOrDefaultPredicateMethodInfo { get; private set; }
        public static MethodInfo QueryableSinglePredicateMethodInfo { get; private set; }
        public static MethodInfo QueryableSingleOrDefaultPredicateMethodInfo { get; private set; }

        public static MethodInfo QueryableAnyMethodInfo { get; private set; }
        public static MethodInfo QueryableAnyPredicateMethodInfo { get; private set; }
        public static MethodInfo QueryableAllMethodInfo { get; private set; }
        public static MethodInfo QueryableContainsMethodInfo { get; private set; }

        public static MethodInfo QueryableCountMethodInfo { get; private set; }
        public static MethodInfo QueryableCountPredicateMethodInfo { get; private set; }
        public static MethodInfo QueryableLongCountMethodInfo { get; private set; }
        public static MethodInfo QueryableLongCountPredicateMethodInfo { get; private set; }
        public static MethodInfo QueryableDistinctMethodInfo { get; private set; }
        public static MethodInfo QueryableTakeMethodInfo { get; private set; }
        public static MethodInfo QueryableSkipMethodInfo { get; private set; }

        public static MethodInfo QueryableOfType { get; private set; }

        public static MethodInfo QueryableDefaultIfEmpty { get; private set; }
        public static MethodInfo QueryableDefaultIfEmptyWithDefaultValue { get; private set; }

        public static MethodInfo EnumerableWhereMethodInfo { get; private set; }
        public static MethodInfo EnumerableSelectMethodInfo { get; private set; }

        public static MethodInfo EnumerableJoinMethodInfo { get; private set; }
        public static MethodInfo EnumerableGroupJoinMethodInfo { get; private set; }
        public static MethodInfo EnumerableSelectManyWithResultOperatorMethodInfo { get; private set; }

        public static MethodInfo EnumerableGroupByKeySelector { get; private set; }
        public static MethodInfo EnumerableGroupByKeySelectorResultSelector { get; private set; }
        public static MethodInfo EnumerableGroupByKeySelectorElementSelector { get; private set; }
        public static MethodInfo EnumerableGroupByKeySelectorElementSelectorResultSelector { get; private set; }

        public static MethodInfo EnumerableFirstMethodInfo { get; private set; }
        public static MethodInfo EnumerableFirstOrDefaultMethodInfo { get; private set; }
        public static MethodInfo EnumerableSingleMethodInfo { get; private set; }
        public static MethodInfo EnumerableSingleOrDefaultMethodInfo { get; private set; }

        public static MethodInfo EnumerableFirstPredicateMethodInfo { get; private set; }
        public static MethodInfo EnumerableFirstOrDefaultPredicateMethodInfo { get; private set; }
        public static MethodInfo EnumerableSinglePredicateMethodInfo { get; private set; }
        public static MethodInfo EnumerableSingleOrDefaultPredicateMethodInfo { get; private set; }

        public static MethodInfo EnumerableDefaultIfEmptyMethodInfo { get; private set; }

        public static MethodInfo EnumerableAnyMethodInfo { get; private set; }
        public static MethodInfo EnumerableAnyPredicateMethodInfo { get; private set; }
        public static MethodInfo EnumerableAllMethodInfo { get; private set; }
        public static MethodInfo EnumerableContainsMethodInfo { get; private set; }

        public static MethodInfo EnumerableCountMethodInfo { get; private set; }
        public static MethodInfo EnumerableCountPredicateMethodInfo { get; private set; }
        public static MethodInfo EnumerableLongCountMethodInfo { get; private set; }
        public static MethodInfo EnumerableLongCountPredicateMethodInfo { get; private set; }

        static LinqMethodHelpers()
        {
            var queryableMethods = typeof(Queryable).GetMethods().ToList();

            QueryableWhereMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Where) && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1)).Single();
            QueryableSelectMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Select) && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1)).Single();
            QueryableOrderByMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Count() == 2).Single();
            QueryableOrderByDescendingMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Count() == 2).Single();
            QueryableThenByMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Count() == 2).Single();
            QueryableThenByDescendingMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Count() == 2).Single();
            QueryableJoinMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Join) && m.GetParameters().Count() == 5).Single();
            QueryableGroupJoinMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.GroupJoin) && m.GetParameters().Count() == 5).Single();

            QueryableSelectManyMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.SelectMany) && m.GetParameters().Count() == 2 && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1)).Single();
            QueryableSelectManyWithResultOperatorMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.SelectMany) && m.GetParameters().Count() == 3 && IsExpressionOfFunc(m.GetParameters()[1].ParameterType, 1)).Single();

            QueryableGroupByKeySelector = queryableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 2).Single();
            QueryableGroupByKeySelectorResultSelector = queryableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 3 && IsExpressionOfFunc(m.GetParameters()[2].ParameterType, 2)).Single();
            QueryableGroupByKeySelectorElementSelector = queryableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 3 && IsExpressionOfFunc(m.GetParameters()[2].ParameterType, 1)).Single();
            QueryableGroupByKeySelectorElementSelectorResultSelector = queryableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 4 && IsExpressionOfFunc(m.GetParameters()[2].ParameterType, 1) && IsExpressionOfFunc(m.GetParameters()[3].ParameterType, 2)).Single();

            QueryableFirstMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.First) && m.GetParameters().Count() == 1).Single();
            QueryableFirstOrDefaultMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Count() == 1).Single();
            QueryableSingleMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Single) && m.GetParameters().Count() == 1).Single();
            QueryableSingleOrDefaultMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.SingleOrDefault) && m.GetParameters().Count() == 1).Single();

            QueryableFirstPredicateMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.First) && m.GetParameters().Count() == 2).Single();
            QueryableFirstOrDefaultPredicateMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Count() == 2).Single();
            QueryableSinglePredicateMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Single) && m.GetParameters().Count() == 2).Single();
            QueryableSingleOrDefaultPredicateMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.SingleOrDefault) && m.GetParameters().Count() == 2).Single();

            QueryableCountMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Count) && m.GetParameters().Count() == 1).Single();
            QueryableCountPredicateMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Count) && m.GetParameters().Count() == 2).Single();
            QueryableLongCountMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.LongCount) && m.GetParameters().Count() == 1).Single();
            QueryableLongCountPredicateMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.LongCount) && m.GetParameters().Count() == 2).Single();

            QueryableDistinctMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Distinct) && m.GetParameters().Count() == 1).Single();
            QueryableTakeMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Take) && m.GetParameters().Count() == 2).Single();
            QueryableSkipMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Skip) && m.GetParameters().Count() == 2).Single();

            QueryableAnyMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Any) && m.GetParameters().Count() == 1).Single();
            QueryableAnyPredicateMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Any) && m.GetParameters().Count() == 2).Single();
            QueryableAllMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.All) && m.GetParameters().Count() == 2).Single();
            QueryableContainsMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Contains) && m.GetParameters().Count() == 2).Single();

            QueryableOfType = queryableMethods.Where(m => m.Name == nameof(Queryable.OfType) && m.GetParameters().Count() == 1).Single();

            QueryableDefaultIfEmpty = queryableMethods.Where(m => m.Name == nameof(Queryable.DefaultIfEmpty) && m.GetParameters().Count() == 1).Single();
            QueryableDefaultIfEmptyWithDefaultValue = queryableMethods.Where(m => m.Name == nameof(Queryable.DefaultIfEmpty) && m.GetParameters().Count() == 2).Single();

            var enumerableMethods = typeof(Enumerable).GetMethods().ToList();

            EnumerableWhereMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Where) && IsFunc(m.GetParameters()[1].ParameterType, 1)).Single();
            EnumerableSelectMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Select) && IsFunc(m.GetParameters()[1].ParameterType, 1)).Single();

            EnumerableJoinMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Join) && m.GetParameters().Count() == 5).Single();
            EnumerableGroupJoinMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.GroupJoin) && m.GetParameters().Count() == 5).Single();
            EnumerableSelectManyWithResultOperatorMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.SelectMany) && m.GetParameters().Count() == 3 && IsFunc(m.GetParameters()[1].ParameterType, 1)).Single();

            EnumerableGroupByKeySelector = enumerableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 2).Single();
            EnumerableGroupByKeySelectorResultSelector = enumerableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 3 && IsFunc(m.GetParameters()[2].ParameterType, 2)).Single();
            EnumerableGroupByKeySelectorElementSelector = enumerableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 3 && IsFunc(m.GetParameters()[2].ParameterType, 1)).Single();
            EnumerableGroupByKeySelectorElementSelectorResultSelector = enumerableMethods.Where(m => m.Name == nameof(Queryable.GroupBy) && m.GetParameters().Count() == 4 && IsFunc(m.GetParameters()[2].ParameterType, 1) && IsFunc(m.GetParameters()[3].ParameterType, 2)).Single();

            EnumerableFirstMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.First) && m.GetParameters().Count() == 1).Single();
            EnumerableFirstOrDefaultMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.FirstOrDefault) && m.GetParameters().Count() == 1).Single();
            EnumerableSingleMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Single) && m.GetParameters().Count() == 1).Single();
            EnumerableSingleOrDefaultMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.SingleOrDefault) && m.GetParameters().Count() == 1).Single();

            EnumerableFirstPredicateMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.First) && m.GetParameters().Count() == 2).Single();
            EnumerableFirstOrDefaultPredicateMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.FirstOrDefault) && m.GetParameters().Count() == 2).Single();
            EnumerableSinglePredicateMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Single) && m.GetParameters().Count() == 2).Single();
            EnumerableSingleOrDefaultPredicateMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.SingleOrDefault) && m.GetParameters().Count() == 2).Single();

            EnumerableDefaultIfEmptyMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.DefaultIfEmpty) && m.GetParameters().Count() == 1).Single();

            EnumerableAnyMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Count() == 1).Single();
            EnumerableAnyPredicateMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Count() == 2).Single();
            EnumerableAllMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.All) && m.GetParameters().Count() == 2).Single();
            EnumerableContainsMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Count() == 2).Single();

            EnumerableCountMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Count() == 1).Single();
            EnumerableCountPredicateMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Count() == 2).Single();
            EnumerableLongCountMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.LongCount) && m.GetParameters().Count() == 1).Single();
            EnumerableLongCountPredicateMethodInfo = enumerableMethods.Where(m => m.Name == nameof(Enumerable.LongCount) && m.GetParameters().Count() == 2).Single();
        }

        private static bool IsExpressionOfFunc(Type type, int parameterCount)
            => type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(Expression<>)
            && type.GetGenericArguments()[0] is Type expressionTypeArgument
            && expressionTypeArgument.IsGenericType
            && expressionTypeArgument.Name.StartsWith(nameof(Func<object>))
            && expressionTypeArgument.GetGenericArguments().Count() == parameterCount + 1;

        private static bool IsFunc(Type type, int parameterCount)
            => type.IsGenericType
            && type.Name.StartsWith(nameof(Func<object>))
            && type.GetGenericArguments().Count() == parameterCount + 1;
    }
}
