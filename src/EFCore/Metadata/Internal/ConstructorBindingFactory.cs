// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConstructorBindingFactory : IConstructorBindingFactory
    {
        private readonly IPropertyParameterBindingFactory _propertyFactory;
        private readonly IParameterBindingFactories _factories;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ConstructorBindingFactory(
            [NotNull] IPropertyParameterBindingFactory propertyFactory,
            [NotNull] IParameterBindingFactories factories)
        {
            _propertyFactory = propertyFactory;
            _factories = factories;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryBindConstructor(
            IMutableEntityType entityType,
            ConstructorInfo constructor,
            out ConstructorBinding binding,
            out IEnumerable<ParameterInfo> failedBindings)
        {
            IEnumerable<(ParameterInfo Parameter, ParameterBinding Binding)> bindings
                = constructor.GetParameters().Select(
                        p => (p, _propertyFactory.TryBindParameter(entityType, p.ParameterType, p.Name)
                                 ?? _factories.FindFactory(p.ParameterType, p.Name)?.Bind(entityType, p.ParameterType, p.Name)))
                    .ToList();

            if (bindings.Any(b => b.Binding == null))
            {
                failedBindings = bindings.Where(b => b.Binding == null).Select(b => b.Parameter);
                binding = null;

                return false;
            }

            failedBindings = null;
            binding = new DirectConstructorBinding(constructor, bindings.Select(b => b.Binding).ToList());

            return true;
        }
    }
}
