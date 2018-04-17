// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LazyLoaderParameterBindingFactory : ServiceParameterBindingFactory
    {
        private static readonly MethodInfo _loadMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.Load));
        private static readonly MethodInfo _loadAsyncMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.LoadAsync));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public LazyLoaderParameterBindingFactory()
            : base(typeof(ILazyLoader))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CanBind(
            Type parameterType,
            string parameterName)
            => IsLazyLoader(parameterType)
               || IsLazyLoaderMethod(parameterType, parameterName)
               || IsLazyLoaderAsyncMethod(parameterType, parameterName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

            return parameterType == typeof(ILazyLoader)
                ? new DefaultServiceParameterBinding(
                    typeof(ILazyLoader),
                    typeof(ILazyLoader),
                    entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoader(p.ClrType)))
                : parameterType == typeof(Action<object, string>)
                    ? new ServiceMethodParameterBinding(
                        typeof(Action<object, string>),
                        typeof(ILazyLoader),
                        _loadMethod,
                        entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoaderMethod(p.ClrType, p.Name)))
                    : new ServiceMethodParameterBinding(
                        typeof(Func<object, CancellationToken, string, Task>),
                        typeof(ILazyLoader),
                        _loadAsyncMethod,
                        entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoaderAsyncMethod(p.ClrType, p.Name)));
        }

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
