// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreTransaction : IDisposable
    {
        protected DataStoreTransaction([NotNull] ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            Logger = logger;
        }

        protected virtual ILogger Logger { get; }

        public abstract void Commit();

        public abstract void Rollback();

        public abstract void Dispose();
    }
}
