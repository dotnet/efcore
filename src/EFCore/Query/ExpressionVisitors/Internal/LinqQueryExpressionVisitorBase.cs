// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public abstract class LinqQueryExpressionVisitorBase : ExpressionVisitor
    {
        protected MethodInfo QueryableWhereMethodInfo { get; set; }
        protected MethodInfo QueryableSelectMethodInfo { get; set; }
        protected MethodInfo QueryableOrderByMethodInfo { get; set; }
        protected MethodInfo QueryableOrderByDescendingMethodInfo { get; set; }
        protected MethodInfo QueryableThenByMethodInfo { get; set; }
        protected MethodInfo QueryableThenByDescendingMethodInfo { get; set; }
        protected MethodInfo QueryableJoinMethodInfo { get; set; }
        protected MethodInfo QueryableGroupJoinMethodInfo { get; set; }
        protected MethodInfo QueryableSelectManyMethodInfo { get; set; }
        protected MethodInfo QueryableSelectManyWithResultOperatorMethodInfo { get; set; }

        protected MethodInfo QueryableFirstPredicate { get; set; }
        protected MethodInfo QueryableFirstOrDefaultPredicate { get; set; }
        protected MethodInfo QueryableSinglePredicate { get; set; }
        protected MethodInfo QueryableSingleOrDefaultPredicate { get; set; }

        protected MethodInfo QueryableCount { get; set; }
        protected MethodInfo QueryableCountPredicate { get; set; }

        protected MethodInfo EnumerableFirstPredicate { get; set; }
        protected MethodInfo EnumerableFirstOrDefaultPredicate { get; set; }
        protected MethodInfo EnumerableSinglePredicate { get; set; }
        protected MethodInfo EnumerableSingleOrDefaultPredicate { get; set; }

        protected MethodInfo EnumerableDefaultIfEmpty { get; set; }

        protected LinqQueryExpressionVisitorBase()
        {
            var queryableMethods = typeof(Queryable).GetMethods().ToList();

            QueryableWhereMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Where) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            QueryableSelectMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Select) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            QueryableOrderByMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Count() == 2).Single();
            QueryableOrderByDescendingMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Count() == 2).Single();
            QueryableThenByMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Count() == 2).Single();
            QueryableThenByDescendingMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Count() == 2).Single();
            QueryableJoinMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.Join) && m.GetParameters().Count() == 5).Single();
            QueryableGroupJoinMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.GroupJoin) && m.GetParameters().Count() == 5).Single();

            QueryableSelectManyMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.SelectMany) && m.GetParameters().Count() == 2 && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            QueryableSelectManyWithResultOperatorMethodInfo = queryableMethods.Where(m => m.Name == nameof(Queryable.SelectMany) && m.GetParameters().Count() == 3 && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();

            QueryableFirstPredicate = queryableMethods.Where(m => m.Name == nameof(Queryable.First) && m.GetParameters().Count() == 2).Single();
            QueryableFirstOrDefaultPredicate = queryableMethods.Where(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Count() == 2).Single();
            QueryableSinglePredicate = queryableMethods.Where(m => m.Name == nameof(Queryable.Single) && m.GetParameters().Count() == 2).Single();
            QueryableSingleOrDefaultPredicate = queryableMethods.Where(m => m.Name == nameof(Queryable.SingleOrDefault) && m.GetParameters().Count() == 2).Single();

            QueryableCount = queryableMethods.Where(m => m.Name == nameof(Queryable.Count) && m.GetParameters().Count() == 1).Single();
            QueryableCountPredicate = queryableMethods.Where(m => m.Name == nameof(Queryable.Count) && m.GetParameters().Count() == 2).Single();

            var enumerableMethods = typeof(Enumerable).GetMethods().ToList();

            EnumerableFirstPredicate = enumerableMethods.Where(m => m.Name == nameof(Enumerable.First) && m.GetParameters().Count() == 2).Single();
            EnumerableFirstOrDefaultPredicate = enumerableMethods.Where(m => m.Name == nameof(Enumerable.FirstOrDefault) && m.GetParameters().Count() == 2).Single();
            EnumerableSinglePredicate = enumerableMethods.Where(m => m.Name == nameof(Enumerable.Single) && m.GetParameters().Count() == 2).Single();
            EnumerableSingleOrDefaultPredicate = enumerableMethods.Where(m => m.Name == nameof(Enumerable.SingleOrDefault) && m.GetParameters().Count() == 2).Single();

            EnumerableDefaultIfEmpty = enumerableMethods.Where(m => m.Name == nameof(Enumerable.DefaultIfEmpty) && m.GetParameters().Count() == 1).Single();
        }
    }
}
