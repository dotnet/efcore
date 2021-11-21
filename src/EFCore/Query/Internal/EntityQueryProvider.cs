// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityQueryProvider : IAsyncQueryProvider
{
    private static readonly MethodInfo _genericCreateQueryMethod
        = typeof(EntityQueryProvider).GetRuntimeMethods()
            .Single(m => (m.Name == "CreateQuery") && m.IsGenericMethod);

    private readonly MethodInfo _genericExecuteMethod;

    private readonly IQueryCompiler _queryCompiler;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityQueryProvider(IQueryCompiler queryCompiler)
    {
        _queryCompiler = queryCompiler;
        _genericExecuteMethod = queryCompiler.GetType()
            .GetRuntimeMethods()
            .Single(m => (m.Name == "Execute") && m.IsGenericMethod);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new EntityQueryable<TElement>(this, expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IQueryable CreateQuery(Expression expression)
        => (IQueryable)_genericCreateQueryMethod
            .MakeGenericMethod(expression.Type.GetSequenceType())
            .Invoke(this, new object[] { expression })!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TResult Execute<TResult>(Expression expression)
        => _queryCompiler.Execute<TResult>(expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Execute(Expression expression)
        => _genericExecuteMethod.MakeGenericMethod(expression.Type)
            .Invoke(_queryCompiler, new object[] { expression })!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => _queryCompiler.ExecuteAsync<TResult>(expression, cancellationToken);
}
