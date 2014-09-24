// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreTransaction : IDisposable
    {
        private readonly ILogger _logger;

        protected DataStoreTransaction()
            : this(NullLogger.Instance)
        {
            _logger = NullLogger.Instance;
        }

        protected DataStoreTransaction([NotNull] ILogger logger)
        {
            Check.NotNull(logger, "logger");

            _logger = logger;
        }

        protected virtual ILogger Logger
        {
            get { return _logger; } 
        }

        public abstract void Commit();

        public abstract void Rollback();

        public abstract void Dispose();
    }
}
