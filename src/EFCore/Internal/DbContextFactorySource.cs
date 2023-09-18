// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbContextFactorySource<TContext> : IDbContextFactorySource<TContext>
    where TContext : DbContext
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DbContextFactorySource()
    {
        Factory = CreateActivator();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<IServiceProvider, DbContextOptions<TContext>, TContext> Factory { get; }

    private static Func<IServiceProvider, DbContextOptions<TContext>, TContext> CreateActivator()
    {
        var constructors
            = typeof(TContext).GetTypeInfo().DeclaredConstructors
                .Where(c => c is { IsStatic: false, IsPublic: true } && c.GetParameters().Length != 0)
                .ToArray();

        if (constructors.Length == 1)
        {
            var parameters = constructors[0].GetParameters();

            if (parameters.Length == 1)
            {
                var isGeneric = parameters[0].ParameterType == typeof(DbContextOptions<TContext>);
                if (isGeneric
                    || parameters[0].ParameterType == typeof(DbContextOptions))
                {
                    var optionsParam = Expression.Parameter(typeof(DbContextOptions<TContext>), "options");
                    var providerParam = Expression.Parameter(typeof(IServiceProvider), "provider");

                    return Expression.Lambda<Func<IServiceProvider, DbContextOptions<TContext>, TContext>>(
                            Expression.New(
                                constructors[0],
                                isGeneric
                                    ? optionsParam
                                    : Expression.Convert(optionsParam, typeof(DbContextOptions))),
                            providerParam, optionsParam)
                        .Compile();
                }
            }
        }

        var factory = ActivatorUtilities.CreateFactory(typeof(TContext), Type.EmptyTypes);

        return (p, _) => (TContext)factory(p, null);
    }
}
