// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsQueryFactory
    {
        private readonly AtsValueReaderFactory _valueReaderFactory;
        private readonly TableFilterFactory _tableFilterFactory;

        public AtsQueryFactory([NotNull] AtsValueReaderFactory valueReaderFactory, [NotNull] TableFilterFactory tableFilterFactory)
        {
            Check.NotNull(valueReaderFactory, "valueReaderFactory");
            Check.NotNull(tableFilterFactory, "tableFilterFactory");
            _valueReaderFactory = valueReaderFactory;
            _tableFilterFactory = tableFilterFactory;
        }

        public AtsQueryCompilationContext MakeCompilationContext(IModel model)
        {
            return new AtsQueryCompilationContext(model, _tableFilterFactory);
        }

        public AtsQueryContext MakeQueryContext(IModel model, ILogger logger, StateManager stateManager, AtsConnection connection)
        {
            return new AtsQueryContext(model, logger, stateManager, connection, _valueReaderFactory);
        }
    }
}
