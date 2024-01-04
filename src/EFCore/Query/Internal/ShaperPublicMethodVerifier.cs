// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class ShaperPublicMethodVerifier : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCall)
    {
        var method = methodCall.Method;
        if (!method.IsPublic)
        {
            throw new InvalidOperationException($"Method '{method.DeclaringType?.Name}.{method.Name}' isn't public and therefore cannot be invoked in shaper code (incompatible with precompiled queries)");
        }

        var currentType = method.DeclaringType;
        while (currentType is not null)
        {
            if (currentType is { IsPublic: false, IsNestedPublic: false }
                // Exclude anonymous types
                && currentType.GetCustomAttribute(typeof(CompilerGeneratedAttribute), inherit: false) is null)
            {
                throw new InvalidOperationException($"Method '{method.DeclaringType?.Name}.{method.Name}' isn't public and therefore cannot be invoked in shaper code (incompatible with precompiled queries)");
            }

            currentType = currentType.DeclaringType;
        }

        return base.VisitMethodCall(methodCall);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression newExpression)
    {
        var constructor = newExpression.Constructor;
        if (constructor is not null)
        {
            if (!constructor.IsPublic)
            {
                throw new InvalidOperationException($"The constructor for '{constructor.DeclaringType?.Name}' isn't public and therefore cannot be invoked in shaper code (incompatible with precompiled queries)");
            }

            var currentType = constructor.DeclaringType;
            while (currentType is not null)
            {
                if (currentType is { IsPublic: false, IsNestedPublic: false }
                    // Exclude anonymous types
                    && currentType.GetCustomAttribute(typeof(CompilerGeneratedAttribute), inherit: false) is null)
                {
                    throw new InvalidOperationException($"Constructor for '{constructor.DeclaringType?.Name}' isn't public and therefore cannot be invoked in shaper code (incompatible with precompiled queries)");
                }

                currentType = currentType.DeclaringType;
            }
        }

        return base.VisitNew(newExpression);
    }

    // Ignore liftable constant nodes - they contain literals only (not method/constructor invocations) and cause exceptions.
    protected override Expression VisitExtension(Expression node)
        => node is LiftableConstantExpression ? node : base.VisitExtension(node);
}
