// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Design.Internal
{
    public class AggregateServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider[] _serviceProviders;

        public AggregateServiceProvider([NotNull] params IServiceProvider[] serviceProviders)
        {
            _serviceProviders = serviceProviders;
        }

        public virtual object GetService(Type serviceType)
            => Enumerable.FirstOrDefault(
                from p in _serviceProviders
                let s = p.GetService(serviceType)
                where s != null
                select s);
    }
}
