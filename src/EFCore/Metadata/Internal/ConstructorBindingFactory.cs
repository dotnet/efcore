// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class ConstructorBindingFactory : IConstructorBindingFactory
    {
        private readonly IPropertyParameterBindingFactory _propertyFactory;
        private readonly IParameterBindingFactories _factories;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ConstructorBindingFactory(
            [NotNull] IPropertyParameterBindingFactory propertyFactory,
            [NotNull] IParameterBindingFactories factories)
        {
            _propertyFactory = propertyFactory;
            _factories = factories;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryBindConstructor(
            IMutableEntityType entityType,
            ConstructorInfo constructor,
            out InstantiationBinding binding,
            out IEnumerable<ParameterInfo> unboundParameters)
            => TryBindConstructor(
                entityType,
                constructor,
                (f, e, p, n) => f?.Bind((IMutableEntityType)e, p, n),
                out binding,
                out unboundParameters);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryBindConstructor(
            IConventionEntityType entityType,
            ConstructorInfo constructor,
            out InstantiationBinding binding,
            out IEnumerable<ParameterInfo> unboundParameters)
            => TryBindConstructor(
                entityType,
                constructor,
                (f, e, p, n) => f?.Bind((IConventionEntityType)e, p, n),
                out binding,
                out unboundParameters);

        private bool TryBindConstructor(
            IEntityType entityType,
            ConstructorInfo constructor,
            Func<IParameterBindingFactory, IEntityType, Type, string, ParameterBinding> bind,
            out InstantiationBinding binding,
            out IEnumerable<ParameterInfo> unboundParameters)
        {
            IEnumerable<(ParameterInfo Parameter, ParameterBinding Binding)> bindings
                = constructor.GetParameters().Select(
                        p => (p, string.IsNullOrEmpty(p.Name)
                            ? null
                            : _propertyFactory.FindParameter(entityType, p.ParameterType, p.Name)
                            ?? bind(_factories.FindFactory(p.ParameterType, p.Name), entityType, p.ParameterType, p.Name)))
                    .ToList();

            if (bindings.Any(b => b.Binding == null))
            {
                unboundParameters = bindings.Where(b => b.Binding == null).Select(b => b.Parameter);
                binding = null;

                return false;
            }

            unboundParameters = null;
            binding = new ConstructorBinding(constructor, bindings.Select(b => b.Binding).ToList());

            return true;
        }
    }
}
