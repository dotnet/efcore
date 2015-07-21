// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Internal
{
    public static class DbContextActivator
    {
        [ThreadStatic]
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }

            [param: CanBeNull] set { _serviceProvider = value; }
        }

        public static TContext CreateInstance<TContext>([NotNull] IServiceProvider serviceProvider)
        {
            try
            {
                _serviceProvider = serviceProvider;

                return (TContext)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TContext));
            }
            finally
            {
                _serviceProvider = null;
            }
        }

        public static TContext CreateInstance<TContext>([NotNull] IServiceProvider serviceProvider, [NotNull] params object[] parameters)
        {
            try
            {
                _serviceProvider = serviceProvider;

                return (TContext)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TContext), parameters);
            }
            finally
            {
                _serviceProvider = null;
            }
        }
    }
}
