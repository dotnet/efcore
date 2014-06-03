// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AzureTableStorageQueryFactory
    {
        private readonly AtsValueReaderFactory _valueReaderFactory;
        private readonly TableFilterFactory _tableFilterFactory;

        public AzureTableStorageQueryFactory([NotNull] AtsValueReaderFactory valueReaderFactory, [NotNull] TableFilterFactory tableFilterFactory)
        {
            Check.NotNull(valueReaderFactory, "valueReaderFactory");
            Check.NotNull(tableFilterFactory, "tableFilterFactory");
            _valueReaderFactory = valueReaderFactory;
            _tableFilterFactory = tableFilterFactory;
        }

        public AzureTableStorageQueryCompilationContext MakeCompilationContext(IModel model)
        {
            return new AzureTableStorageQueryCompilationContext(model, _tableFilterFactory);
        }

        public AzureTableStorageQueryContext MakeQueryContext(IModel model, ILogger logger, StateManager stateManager, AzureTableStorageConnection database)
        {
            return new AzureTableStorageQueryContext(model, logger, stateManager, database, _valueReaderFactory);
        }
    }
}
