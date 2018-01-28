// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ServiceParameterBindingFactory : IParameterBindingFactory
    {
        private readonly Type _serviceType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ServiceParameterBindingFactory([NotNull] Type serviceType)
        {
            _serviceType = serviceType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanBind(
            Type parameterType,
            string parameterName)
            => parameterType == _serviceType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ParameterBinding Bind(
            IMutableEntityType entityType,
            Type parameterType,
            string parameterName)
            => new DefaultServiceParameterBinding(
                _serviceType,
                _serviceType,
                entityType.GetServiceProperties().FirstOrDefault(p => p.ClrType == _serviceType));
    }
}
