// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     A <see cref="IParameterBindingFactory" /> for binding to the <see cref="ILazyLoader" /> service.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
///         instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class LazyLoaderParameterBindingFactory : ServiceParameterBindingFactory
{
    private static readonly MethodInfo LoadMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.Load))!;
    private static readonly MethodInfo LoadAsyncMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.LoadAsync))!;

    /// <summary>
    ///     Creates a new <see cref="LazyLoaderParameterBindingFactory" /> instance.
    /// </summary>
    /// <param name="dependencies">The service dependencies to use.</param>
    public LazyLoaderParameterBindingFactory(LazyLoaderParameterBindingFactoryDependencies dependencies)
        : base(typeof(ILazyLoader))
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual LazyLoaderParameterBindingFactoryDependencies Dependencies { get; }

    /// <summary>
    ///     Checks whether or not this factory can bind a parameter with the given type and name.
    /// </summary>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns><see langword="true" /> if this parameter can be bound; <see langword="false" /> otherwise.</returns>
    public override bool CanBind(
        Type parameterType,
        string parameterName)
        => IsLazyLoader(parameterType)
            || IsLazyLoaderMethod(parameterType, parameterName)
            || IsLazyLoaderAsyncMethod(parameterType, parameterName);

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    public override ParameterBinding Bind(
        IMutableEntityType entityType,
        Type parameterType,
        string parameterName)
    {
        var baseType = entityType;
        do
        {
            baseType.SetNavigationAccessMode(PropertyAccessMode.Field);
            baseType = baseType.BaseType;
        }
        while (baseType != null);

        return Bind((IEntityType)entityType, parameterType);
    }

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    public override ParameterBinding Bind(
        IConventionEntityType entityType,
        Type parameterType,
        string parameterName)
    {
        var baseType = entityType;
        do
        {
            baseType.SetNavigationAccessMode(PropertyAccessMode.Field);
            baseType = baseType.BaseType;
        }
        while (baseType != null);

        return Bind((IEntityType)entityType, parameterType);
    }

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    public override ParameterBinding Bind(
        IReadOnlyEntityType entityType,
        Type parameterType,
        string parameterName)
        => Bind((IEntityType)entityType, parameterType);

    private static ParameterBinding Bind(IEntityType entityType, Type parameterType)
        => parameterType == typeof(ILazyLoader)
            ? new DependencyInjectionParameterBinding(
                typeof(ILazyLoader),
                typeof(ILazyLoader),
                entityType.GetServiceProperties().Cast<IPropertyBase>().Where(p => IsLazyLoader(p.ClrType)).ToArray())
            : parameterType == typeof(Action<object, string>)
                ? new DependencyInjectionMethodParameterBinding(
                    typeof(Action<object, string>),
                    typeof(ILazyLoader),
                    LoadMethod,
                    entityType.GetServiceProperties().Cast<IPropertyBase>().Where(p => IsLazyLoaderMethod(p.ClrType, p.Name)).ToArray())
                : new DependencyInjectionMethodParameterBinding(
                    typeof(Func<object, CancellationToken, string, Task>),
                    typeof(ILazyLoader),
                    LoadAsyncMethod,
                    entityType.GetServiceProperties().Cast<IPropertyBase>().Where(p => IsLazyLoaderAsyncMethod(p.ClrType, p.Name))
                        .ToArray());

    private static bool IsLazyLoader(Type type)
        => type == typeof(ILazyLoader);

    private static bool IsLazyLoaderMethod(Type type, string name)
        => type == typeof(Action<object, string>)
            && name.Equals("lazyLoader", StringComparison.OrdinalIgnoreCase);

    private static bool IsLazyLoaderAsyncMethod(Type type, string name)
        => type == typeof(Func<object, CancellationToken, string, Task>)
            && name.Equals("lazyLoader", StringComparison.OrdinalIgnoreCase);
}
