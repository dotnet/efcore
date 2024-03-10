// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public abstract class ExpressionMutator(DbContext context)
{
    protected static MethodInfo IncludeMethodInfo;
    protected static MethodInfo ThenIncludeReferenceMethodInfo;
    protected static MethodInfo ThenIncludeCollectionMethodInfo;

    protected DbContext Context { get; } = context;

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

    protected class ExpressionInjector(Expression expressionToInject, Func<Expression, Expression> injectionPattern) : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (node == expressionToInject)
            {
                return injectionPattern(node);
            }

            return base.Visit(node);
        }
    }
}
