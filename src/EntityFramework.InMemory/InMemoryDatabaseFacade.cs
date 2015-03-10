// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDatabaseFacade : Database
    {
        public InMemoryDatabaseFacade(
            [NotNull] DbContext context,
            [NotNull] IInMemoryDataStoreCreator dataStoreCreator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(context, dataStoreCreator, loggerFactory)
        {
        }
    }
}
