// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestServiceFactory
    {
        public static readonly TestServiceFactory Instance = new TestServiceFactory();

        private TestServiceFactory()
        {
        }

        private readonly ConcurrentDictionary<Type, IServiceProvider> _factories
            = new ConcurrentDictionary<Type, IServiceProvider>();

        public TService Create<TService>()
            where TService : class
        {
            return _factories.GetOrAdd(
                typeof(TService),
                t => AddType(new ServiceCollection(), typeof(TService)).BuildServiceProvider()).GetService<TService>();
        }

        private static ServiceCollection AddType(ServiceCollection serviceCollection, Type serviceType)
        {
            serviceCollection.AddSingleton(serviceType);

            var constructors = serviceType.GetConstructors();
            var constructor = constructors
                .FirstOrDefault(c => c.GetParameters().Length == constructors.Max(c2 => c2.GetParameters().Length));

            if (constructor == null)
            {
                throw new InvalidOperationException("Cannot use with no public constructors.");
            }

            foreach (var parameter in constructor.GetParameters())
            {
                AddType(serviceCollection, parameter.ParameterType);
            }

            return serviceCollection;
        }
    }
}
