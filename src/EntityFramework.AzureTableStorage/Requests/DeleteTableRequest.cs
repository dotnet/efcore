// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public class DeleteTableRequest : TableRequest<bool>
    {
        public DeleteTableRequest([NotNull] AtsTable table)
            : base(table)
        {
        }

        public override string Name
        {
            get { return "DeleteTableCommand"; }
        }

        protected override bool ExecuteOnTable(CloudTable table, RequestContext requestContext)
        {
            Check.NotNull(table, "table");
            Check.NotNull(requestContext, "requestContext");

            return table.DeleteIfExists(requestContext.TableRequestOptions, requestContext.OperationContext);
        }

        protected override Task<bool> ExecuteOnTableAsync(CloudTable table, RequestContext requestContext, CancellationToken cancellationToken)
        {
            Check.NotNull(table, "table");
            Check.NotNull(requestContext, "requestContext");

            return table.DeleteIfExistsAsync(requestContext.TableRequestOptions, requestContext.OperationContext, cancellationToken);
        }
    }
}
