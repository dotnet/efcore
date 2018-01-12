// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LazyLoaderParameterBindingFactory : ParameterBindingFactory
    {
        private static readonly MethodInfo _loadMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.Load));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ParameterBinding TryBindParameter(IMutableEntityType entityType, ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(ILazyLoader))
            {
                EnsureFieldAccess(entityType);

                return new ServiceParameterBinding(typeof(ILazyLoader), typeof(ILazyLoader));
            }

            if (parameter.ParameterType == typeof(Action<object, string>)
                && parameter.Name.Equals("lazyLoader", StringComparison.OrdinalIgnoreCase))
            {
                EnsureFieldAccess(entityType);

                return new ServiceMethodParameterBinding(typeof(Action<object, string>), typeof(ILazyLoader), _loadMethod);
            }

            return null;
        }

        private static void EnsureFieldAccess(IMutableEntityType entityType)
        {
            foreach (var navigation in entityType.GetNavigations())
            {
                navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
            }
        }
    }
}
