// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public abstract class ExpressionMutator
    {
        protected static MethodInfo IncludeMethodInfo;
        protected static MethodInfo ThenIncludeReferenceMethodInfo;
        protected static MethodInfo ThenIncludeCollectionMethodInfo;

        protected DbContext Context { get; }

        static ExpressionMutator()
        {
            IncludeMethodInfo = typeof(EntityFrameworkQueryableExtensions).GetMethods().Where(
                    m => m.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                        && m.GetParameters()[1].ParameterType != typeof(string))
                .Single();
            ThenIncludeCollectionMethodInfo = typeof(EntityFrameworkQueryableExtensions).GetMethods().Where(
                    m => m.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude)
                        && m.GetParameters()[0].ParameterType.GetGenericArguments()[1].IsGenericType
                        && m.GetParameters()[0].ParameterType.GetGenericArguments()[1].GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Single();

            ThenIncludeReferenceMethodInfo = typeof(EntityFrameworkQueryableExtensions).GetMethods().Where(
                m => m.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude)
                    && m != ThenIncludeCollectionMethodInfo).Single();
        }

        public ExpressionMutator(DbContext context)
        {
            Context = context;
        }

        protected static bool IsQueryableType(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>);

        protected static bool IsEnumerableType(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

        protected static bool IsQueryableResult(Expression expression)
            => IsQueryableType(expression.Type)
                || expression.Type.GetInterfaces().Any(i => IsQueryableType(i));

        private static bool IsOrderedQueryableType(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>);

        protected static bool IsOrderedQueryableResult(Expression expression)
            => IsOrderedQueryableType(expression.Type)
                || expression.Type.GetInterfaces().Any(i => IsOrderedQueryableType(i));

        protected static bool IsOrderedableType(Type type)
            => !typeof(Geometry).IsAssignableFrom(type)
                && type.GetInterfaces().Any(i => i == typeof(IComparable));

        protected List<PropertyInfo> FilterPropertyInfos(Type type, List<PropertyInfo> properties)
        {
            if (type == typeof(string))
            {
                properties = properties.Where(p => p.Name != "Chars").ToList();
            }

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                properties = properties.Where(p => p.Name != "Item" && p.Name != "Capacity").ToList();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>)
                || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                properties = properties.Where(p => p.Name != "IsReadOnly").ToList();
            }

            if (type.IsArray)
            {
                properties = properties.Where(p => p.Name != "Rank" && p.Name != "IsFixedSize" && p.Name != "IsSynchronized").ToList();
            }

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                properties = properties.Where(p => p.Name != "Value").ToList();
            }

            var entityType = Context.Model.FindEntityType(type);
            if (entityType != null)
            {
                properties = properties.Where(
                    p => entityType.GetProperties().Where(pp => pp.PropertyInfo != null).Select(pp => pp.Name).Contains(p.Name)).ToList();
            }

            return properties;
        }

        protected bool IsEntityType(Type type)
            => Context.Model.FindEntityType(type) != null;

        public abstract bool IsValid(Expression expression);
        public abstract Expression Apply(Expression expression, Random random);

        protected class ExpressionInjector : ExpressionVisitor
        {
            private readonly Expression _expressionToInject;
            private readonly Func<Expression, Expression> _injectionPattern;

            public ExpressionInjector(Expression expressionToInject, Func<Expression, Expression> injectionPattern)
            {
                _expressionToInject = expressionToInject;
                _injectionPattern = injectionPattern;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _expressionToInject)
                {
                    return _injectionPattern(node);
                }

                return base.Visit(node);
            }
        }
    }
}
