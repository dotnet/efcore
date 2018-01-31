// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ParameterBindingFactories : IParameterBindingFactories
    {
        private readonly IRegisteredServices _registeredServices;
        private readonly List<IParameterBindingFactory> _parameterBindingFactories;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ParameterBindingFactories(
            [CanBeNull] IEnumerable<IParameterBindingFactory> registeredFactories,
            [NotNull] IRegisteredServices registeredServices)
        {
            _registeredServices = registeredServices;

            _parameterBindingFactories
                = registeredFactories?.ToList() ?? new List<IParameterBindingFactory>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IParameterBindingFactory FindFactory(Type type, string name)
            => _parameterBindingFactories.FirstOrDefault(f => f.CanBind(type, name))
               ?? (_registeredServices.Services.Contains(type)
                   ? new ServiceParameterBindingFactory(type)
                   : null);
    }
}
