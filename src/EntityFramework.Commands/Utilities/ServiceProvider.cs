// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _backupServiceProvider;
        private readonly Dictionary<Type, object> _localServices = new Dictionary<Type, object>();

        public ServiceProvider([CanBeNull] IServiceProvider backupServiceProvider)
        {
            _backupServiceProvider = backupServiceProvider;
        }

        public virtual void AddService([NotNull] Type type, [NotNull] object service)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(service, nameof(service));

            _localServices.Add(type, service);
        }

        public virtual object GetService([NotNull] Type serviceType)
        {
            Check.NotNull(serviceType, nameof(serviceType));

            object service;
            if (_localServices.TryGetValue(serviceType, out service))
            {
                return service;
            }

            if (_backupServiceProvider != null)
            {
                return _backupServiceProvider.GetService(serviceType);
            }

            return null;
        }
    }
}
