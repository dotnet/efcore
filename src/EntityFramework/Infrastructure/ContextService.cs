// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     Used for constructor injection of services that are dynamic based on the configuration
    ///     of the currrently in scope <see cref="DbContext" />.
    /// </summary>
    /// <typeparam name="TService">The service that will be dynamically resolved.</typeparam>
    public class ContextService<TService>
    {
        private readonly LazyRef<TService> _service;

        public ContextService([NotNull] Func<TService> initializer)
        {
            Check.NotNull(initializer, "initializer");

            _service = new LazyRef<TService>(initializer);
        }

        public ContextService([CanBeNull] TService service)
        {
            _service = new LazyRef<TService>(service);
        }

        public virtual TService Service
        {
            get { return _service.Value; }
        }
    }
}
