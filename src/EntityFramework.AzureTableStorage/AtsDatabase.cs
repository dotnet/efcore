// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDatabase : Database
    {
        public AtsDatabase(
            [NotNull] LazyRef<IModel> model,
            [NotNull] AtsDataStoreCreator dataStoreCreator,
            [NotNull] AtsConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
            : base(model, dataStoreCreator, connection, loggerFactory)
        {
        }
    }
}
