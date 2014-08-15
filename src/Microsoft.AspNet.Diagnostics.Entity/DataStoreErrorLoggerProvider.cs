// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Diagnostics.Entity
{
    public class DataStoreErrorLoggerProvider : ILoggerProvider
    {
        private readonly DataStoreErrorLogger _logger = new DataStoreErrorLogger();

        public ILogger Create(string name)
        {
            return _logger;
        }

        public DataStoreErrorLogger Logger
        {
            get { return _logger; }
        }
    }
}