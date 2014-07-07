// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public class TableBatchRequest : AtsAsyncRequest<IList<TableResult>>
    {
        private readonly TableBatchOperation _batch = new TableBatchOperation();
        private readonly AtsTable _table;

        public TableBatchRequest([NotNull] AtsTable table)
        {
            Check.NotNull(table, "table");
            _table = table;
        }

        public virtual void Add([NotNull] TableOperationRequest operation)
        {
            Check.NotNull(operation, "operation");
            
            _batch.Add(operation.Operation);
        }

        public virtual int Count
        {
            get { return _batch.Count; }
        }

        public override string Name
        {
            get { return "TableBatchRequest"; }
        }

        public override IList<TableResult> Execute([NotNull] RequestContext requestContext)
        {
            Check.NotNull(requestContext, "requestContext");

            return requestContext
                .TableClient
                .GetTableReference(_table.Name)
                .ExecuteBatch(_batch, null, requestContext.OperationContext);
        }

        public override Task<IList<TableResult>> ExecuteAsync([NotNull] RequestContext requestContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(requestContext, "requestContext");

            return Task.Run(
                () => requestContext
                    .TableClient
                    .GetTableReference(_table.Name)
                    .ExecuteBatchAsync(_batch, requestContext.TableRequestOptions, requestContext.OperationContext, cancellationToken)
                , cancellationToken);
        }
    }
}
