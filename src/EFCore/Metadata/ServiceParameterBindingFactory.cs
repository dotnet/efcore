// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A <see cref="IParameterBindingFactory" /> for binding to dependency-injected services.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class ServiceParameterBindingFactory : IParameterBindingFactory
    {
        private readonly Type _serviceType;

        /// <summary>
        ///     Creates a new <see cref="ServiceParameterBindingFactory" /> instance for the given service type.
        /// </summary>
        /// <param name="serviceType"> The service type. </param>
        public ServiceParameterBindingFactory([NotNull] Type serviceType)
        {
            Check.NotNull(serviceType, nameof(serviceType));

            _serviceType = serviceType;
        }

        /// <summary>
        ///     Checks whether or not this factory can bind a parameter with the given type and name.
        /// </summary>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> <see langword="true" /> if this parameter can be bound; <see langword="false" /> otherwise. </returns>
        public virtual bool CanBind(
            Type parameterType,
            string parameterName)
            => parameterType == _serviceType;

        /// <summary>
        ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> The binding. </returns>
        public virtual ParameterBinding Bind(IMutableEntityType entityType, Type parameterType, string parameterName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(parameterType, nameof(parameterType));
            Check.NotEmpty(parameterName, nameof(parameterName));

            return new DependencyInjectionParameterBinding(
                _serviceType,
                _serviceType,
                entityType.GetServiceProperties().FirstOrDefault(p => p.ClrType == _serviceType));
        }

        /// <summary>
        ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> The binding. </returns>
        public virtual ParameterBinding Bind(
            IConventionEntityType entityType,
            Type parameterType,
            string parameterName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(parameterType, nameof(parameterType));
            Check.NotEmpty(parameterName, nameof(parameterName));

            return new DependencyInjectionParameterBinding(
                _serviceType,
                _serviceType,
                entityType.GetServiceProperties().FirstOrDefault(p => p.ClrType == _serviceType));
        }
    }
}
