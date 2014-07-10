// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public abstract class TableRequest<TResult> : AtsAsyncRequest<TResult>
    {
        private readonly AtsTable _table;

        protected TableRequest([NotNull] AtsTable table)
        {
            Check.NotNull(table, "table");

            _table = table;
        }

        public virtual AtsTable Table
        {
            get { return _table; }
        }

        protected abstract TResult ExecuteOnTable([NotNull] CloudTable table, [NotNull] RequestContext requestContext);
        protected abstract Task<TResult> ExecuteOnTableAsync([NotNull] CloudTable table, [NotNull] RequestContext requestContext, CancellationToken cancellationToken);

        public override TResult Execute([NotNull] RequestContext requestContext)
        {
            Check.NotNull(requestContext, "requestContext");

            var cloudTable = requestContext.TableClient.GetTableReference(_table.Name);
            return ExecuteOnTable(cloudTable, requestContext);
        }

        public override Task<TResult> ExecuteAsync([NotNull] RequestContext requestContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(requestContext, "requestContext");

            var cloudTable = requestContext.TableClient.GetTableReference(_table.Name);
            return ExecuteOnTableAsync(cloudTable, requestContext, cancellationToken);
        }
    }
}
