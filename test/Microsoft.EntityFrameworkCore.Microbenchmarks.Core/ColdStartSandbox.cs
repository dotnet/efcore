// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core
{
    public class ColdStartSandbox : IDisposable
    {
        private AppDomain _domain = AppDomain.CreateDomain(
            "Cold Start Sandbox",
            null,
            new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory });

        ~ColdStartSandbox()
        {
            Dispose(false);
        }

        public T CreateInstance<T>(params object[] args)
        {
            return (T)CreateInstance(typeof(T), args);
        }

        public object CreateInstance(Type type, params object[] args)
        {
            HandleDisposed();

            return _domain.CreateInstanceAndUnwrap(
                type.Assembly.FullName,
                type.FullName,
                ignoreCase: false,
                bindingAttr: 0,
                binder: null,
                args: args,
                culture: null,
                activationAttributes: null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (_domain != null))
            {
                AppDomain.Unload(_domain);
                _domain = null;
            }
        }

        private void HandleDisposed()
        {
            if (_domain == null)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}

#endif
