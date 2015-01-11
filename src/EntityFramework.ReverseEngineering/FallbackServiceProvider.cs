// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    public class FallbackServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly IServiceProvider _fallbackServiceProvider;

        public FallbackServiceProvider(IServiceProvider fallbackServiceProvider)
        {
            _instances[typeof(IServiceProvider)] = this;
            _fallbackServiceProvider = fallbackServiceProvider;
        }

        public void Add(Type type, object instance)
        {
            _instances[type] = instance;
        }

        public object GetService(Type serviceType)
        {
            object instance;
            if (_instances.TryGetValue(serviceType, out instance))
            {
                return instance;
            }

            if (_fallbackServiceProvider != null)
            {
                return _fallbackServiceProvider.GetService(serviceType);
            }

            return null;
        }
    }
}
