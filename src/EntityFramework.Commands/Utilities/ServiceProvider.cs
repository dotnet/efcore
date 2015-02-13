// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public class ServiceProvider : IServiceProvider
    {
        private IServiceProvider _backupServiceProvider;
        private Dictionary<Type, object> _localServices = new Dictionary<Type, object>();

        public ServiceProvider([CanBeNull]IServiceProvider backupServiceProvider)
        {
            _backupServiceProvider = backupServiceProvider;
        }

        public void AddService(Type type, object service)
        {
            _localServices.Add(type, service);
        }

        public object GetService(Type serviceType)
        {
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