// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public static class InMemoryLinqOperatorProvider
    {
        private static MethodInfo GetMethod(string name, int parameterCount = 0)
            => GetMethods(name, parameterCount).Single();

        private static IEnumerable<MethodInfo> GetMethods(string name, int parameterCount = 0)
            => typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(name)
                .Where(mi => mi.GetParameters().Length == parameterCount + 1);

        public static MethodInfo Where = GetMethods(nameof(Enumerable.Where), 1)
            .Single(mi => mi.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2);
        public static MethodInfo Select = GetMethods(nameof(Enumerable.Select), 1)
            .Single(mi => mi.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2);

        public static MethodInfo Join = GetMethod(nameof(Enumerable.Join), 4);
        public static MethodInfo Contains = GetMethod(nameof(Enumerable.Contains), 1);

        public static MethodInfo OrderBy = GetMethod(nameof(Enumerable.OrderBy), 1);
        public static MethodInfo OrderByDescending = GetMethod(nameof(Enumerable.OrderByDescending), 1);
        public static MethodInfo ThenBy = GetMethod(nameof(Enumerable.ThenBy), 1);
        public static MethodInfo ThenByDescending = GetMethod(nameof(Enumerable.ThenByDescending), 1);
        public static MethodInfo All = GetMethod(nameof(Enumerable.All), 1);
        public static MethodInfo Any = GetMethod(nameof(Enumerable.Any));
        public static MethodInfo AnyPredicate = GetMethod(nameof(Enumerable.Any), 1);
        public static MethodInfo Count = GetMethod(nameof(Enumerable.Count));
        public static MethodInfo LongCount = GetMethod(nameof(Enumerable.LongCount));
        public static MethodInfo CountPredicate = GetMethod(nameof(Enumerable.Count), 1);
        public static MethodInfo LongCountPredicate = GetMethod(nameof(Enumerable.LongCount), 1);
        public static MethodInfo Distinct = GetMethod(nameof(Enumerable.Distinct));
        public static MethodInfo Take = GetMethod(nameof(Enumerable.Take), 1);
        public static MethodInfo Skip = GetMethod(nameof(Enumerable.Skip), 1);

        public static MethodInfo FirstPredicate = GetMethod(nameof(Enumerable.First), 1);
        public static MethodInfo FirstOrDefaultPredicate = GetMethod(nameof(Enumerable.FirstOrDefault), 1);
        public static MethodInfo LastPredicate = GetMethod(nameof(Enumerable.Last), 1);
        public static MethodInfo LastOrDefaultPredicate = GetMethod(nameof(Enumerable.LastOrDefault), 1);
        public static MethodInfo SinglePredicate = GetMethod(nameof(Enumerable.Single), 1);
        public static MethodInfo SingleOrDefaultPredicate = GetMethod(nameof(Enumerable.SingleOrDefault), 1);

        public static MethodInfo GetAggregateMethod(string methodName, Type elementType, int parameterCount = 0)
        {
            Check.NotEmpty(methodName, nameof(methodName));
            Check.NotNull(elementType, nameof(elementType));

            var aggregateMethods = GetMethods(methodName, parameterCount).ToList();

            return
                aggregateMethods
                    .Single(
                        mi => mi.GetParameters().Last().ParameterType.GetGenericArguments().Last() == elementType);
            //?? aggregateMethods.Single(mi => mi.IsGenericMethod)
            //    .MakeGenericMethod(elementType);
        }
    }

}
