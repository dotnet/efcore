// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class DbContextActivator
    {
        [ThreadStatic]
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }

            [param: CanBeNull]
            set { _serviceProvider = value; }
        }

        public static TContext CreateInstance<TContext>([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            // TODO: Figure out what needs to be done if there is no ITypeActivator.
            var typeActivator = serviceProvider.GetService<ITypeActivator>();

            try
            {
                _serviceProvider = serviceProvider;

                return (TContext)typeActivator.CreateInstance(serviceProvider, typeof(TContext));
            }
            finally
            {
                _serviceProvider = null;
            }
        }
    }
}
