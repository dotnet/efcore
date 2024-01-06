// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class EvaluatableExpressionFilter : IEvaluatableExpressionFilter
{
    // This methods are non-deterministic and result varies based on time of running the query.
    // Hence we don't evaluate them. See issue#2069
    private static readonly PropertyInfo DateTimeNow
        = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now))!;

    private static readonly PropertyInfo DateTimeUtcNow
        = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.UtcNow))!;

    private static readonly PropertyInfo DateTimeToday
        = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Today))!;

    private static readonly PropertyInfo DateTimeOffsetNow
        = typeof(DateTimeOffset).GetTypeInfo().GetDeclaredProperty(nameof(DateTimeOffset.Now))!;

    private static readonly PropertyInfo DateTimeOffsetUtcNow
        = typeof(DateTimeOffset).GetTypeInfo().GetDeclaredProperty(nameof(DateTimeOffset.UtcNow))!;

    private static readonly MethodInfo GuidNewGuid
        = typeof(Guid).GetTypeInfo().GetDeclaredMethod(nameof(Guid.NewGuid))!;

    private static readonly MethodInfo RandomNextNoArgs
        = typeof(Random).GetRuntimeMethod(nameof(Random.Next), Type.EmptyTypes)!;

    private static readonly MethodInfo RandomNextOneArg
        = typeof(Random).GetRuntimeMethod(nameof(Random.Next), [typeof(int)])!;

    private static readonly MethodInfo RandomNextTwoArgs
        = typeof(Random).GetRuntimeMethod(nameof(Random.Next), [typeof(int), typeof(int)])!;

    /// <summary>
    ///     <para>
    ///         Creates a new <see cref="EvaluatableExpressionFilter" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    public EvaluatableExpressionFilter(
        EvaluatableExpressionFilterDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual EvaluatableExpressionFilterDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual bool IsEvaluatableExpression(Expression expression, IModel model)
    {
        switch (expression)
        {
            case MemberExpression memberExpression:
                var member = memberExpression.Member;
                if (Equals(member, DateTimeNow)
                    || Equals(member, DateTimeUtcNow)
                    || Equals(member, DateTimeToday)
                    || Equals(member, DateTimeOffsetNow)
                    || Equals(member, DateTimeOffsetUtcNow))
                {
                    return false;
                }

                break;

            case MethodCallExpression methodCallExpression:
                var method = methodCallExpression.Method;

                if (Equals(method, GuidNewGuid)
                    || Equals(method, RandomNextNoArgs)
                    || Equals(method, RandomNextOneArg)
                    || Equals(method, RandomNextTwoArgs)
                    || method.DeclaringType == typeof(DbFunctionsExtensions))
                {
                    return false;
                }

                break;
        }

        foreach (var plugin in Dependencies.Plugins)
        {
            if (!plugin.IsEvaluatableExpression(expression))
            {
                return false;
            }
        }

        return true;
    }
}
