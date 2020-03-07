// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A <see cref="IParameterBindingFactory" /> for binding to the <see cref="IsLazyLoader" /> service.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class LazyLoaderParameterBindingFactory : ServiceParameterBindingFactory
    {
        private static readonly MethodInfo _loadMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.Load));
        private static readonly MethodInfo _loadAsyncMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.LoadAsync));

        /// <summary>
        ///     Creates a new <see cref="LazyLoaderParameterBindingFactory" /> instance.
        /// </summary>
        /// <param name="dependencies"> The service dependencies to use. </param>
        public LazyLoaderParameterBindingFactory([NotNull] LazyLoaderParameterBindingFactoryDependencies dependencies)
            : base(typeof(ILazyLoader))
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <summary>
        ///     Checks whether or not this factory can bind a parameter with the given type and name.
        /// </summary>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> True if this parameter can be bound; false otherwise. </returns>
        public override bool CanBind(
            Type parameterType,
            string parameterName)
        {
            Check.NotNull(parameterType, nameof(parameterType));
            Check.NotEmpty(parameterName, nameof(parameterName));

            return IsLazyLoader(parameterType)
                || IsLazyLoaderMethod(parameterType, parameterName)
                || IsLazyLoaderAsyncMethod(parameterType, parameterName);
        }

        /// <summary>
        ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> The binding. </returns>
        public override ParameterBinding Bind(
            IMutableEntityType entityType, Type parameterType, string parameterName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(parameterType, nameof(parameterType));
            Check.NotEmpty(parameterName, nameof(parameterName));

            var baseType = entityType;
            do
            {
                baseType.SetNavigationAccessMode(PropertyAccessMode.Field);
                baseType = baseType.BaseType;
            }
            while (baseType != null);

            return Bind(entityType, parameterType);
        }

        /// <summary>
        ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> The binding. </returns>
        public override ParameterBinding Bind(
            IConventionEntityType entityType,
            Type parameterType,
            string parameterName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(parameterType, nameof(parameterType));
            Check.NotEmpty(parameterName, nameof(parameterName));

            var baseType = entityType;
            do
            {
                baseType.SetNavigationAccessMode(PropertyAccessMode.Field);
                baseType = baseType.BaseType;
            }
            while (baseType != null);

            return Bind(entityType, parameterType);
        }

        private static ParameterBinding Bind(IEntityType entityType, Type parameterType)
            => parameterType == typeof(ILazyLoader)
                ? new DependencyInjectionParameterBinding(
                    typeof(ILazyLoader),
                    typeof(ILazyLoader),
                    entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoader(p.ClrType)))
                : parameterType == typeof(Action<object, string>)
                    ? new DependencyInjectionMethodParameterBinding(
                        typeof(Action<object, string>),
                        typeof(ILazyLoader),
                        _loadMethod,
                        entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoaderMethod(p.ClrType, p.Name)))
                    : new DependencyInjectionMethodParameterBinding(
                        typeof(Func<object, CancellationToken, string, Task>),
                        typeof(ILazyLoader),
                        _loadAsyncMethod,
                        entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoaderAsyncMethod(p.ClrType, p.Name)));

        private static bool IsLazyLoader(Type type)
            => type == typeof(ILazyLoader);

        private static bool IsLazyLoaderMethod(Type type, string name)
            => type == typeof(Action<object, string>)
                && name.Equals("lazyLoader", StringComparison.OrdinalIgnoreCase);

        private static bool IsLazyLoaderAsyncMethod(Type type, string name)
            => type == typeof(Func<object, CancellationToken, string, Task>)
                && name.Equals("lazyLoader", StringComparison.OrdinalIgnoreCase);
    }
}
