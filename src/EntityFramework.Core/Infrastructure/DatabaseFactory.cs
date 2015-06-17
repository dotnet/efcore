// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private readonly DbContext _context;
        private readonly IDataStoreCreator _dataStoreCreator;
        private readonly ILoggerFactory _loggerFactory;

        public DatabaseFactory(
            [NotNull] DbContext context,
            [NotNull] IDataStoreCreator dataStoreCreator,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(dataStoreCreator, nameof(dataStoreCreator));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _context = context;
            _dataStoreCreator = dataStoreCreator;
            _loggerFactory = loggerFactory;
        }

        public virtual Database CreateDatabase() => new Database(_context, _dataStoreCreator, _loggerFactory);
    }
}
