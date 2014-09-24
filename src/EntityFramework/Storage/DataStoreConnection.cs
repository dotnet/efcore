// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreConnection
    {
        private readonly LazyRef<ILogger> _logger;

        protected DataStoreConnection()
            : this(new NullLoggerFactory())
        {
        }

        protected DataStoreConnection([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, "loggerFactory");

            _logger = new LazyRef<ILogger>(() => loggerFactory.Create(GetType().Name));
        }

        protected virtual ILogger Logger
        {
            get { return _logger.Value; }
        }
    }
}
