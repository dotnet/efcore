// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisXunitTestFramework : XunitTestFramework
    {
        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new RedisXunitTestExecutor(assemblyName, SourceInformationProvider);
        }
    }

    public class RedisXunitTestExecutor : XunitTestFrameworkExecutor, IDisposable
    {
        private bool _isDisposed;

        public RedisXunitTestExecutor(
            AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider)
            : base(assemblyName, sourceInformationProvider)
        {
            try
            {
                RedisTestConfig.GetOrStartServer();
            }
            catch (Exception)
            {
                // do not let exceptions starting server prevent XunitTestExecutor from being created
            }
        }

        ~RedisXunitTestExecutor()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                try
                {
                    RedisTestConfig.StopRedisServer();
                }
                catch (Exception)
                {
                    // do not let exceptions stopping server prevent XunitTestExecutor from being disposed
                }

                _isDisposed = true;
            }
        }
    }
}
