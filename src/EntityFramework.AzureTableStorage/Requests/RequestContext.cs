// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public class RequestContext
    {
        public RequestContext()
        {
            OperationContext = new OperationContext();
        }

        public virtual OperationContext OperationContext { get; [param: NotNull] set; }

        public virtual CloudTableClient TableClient { get; [param: NotNull] set; }

        public virtual ILogger Logger { get; [param: NotNull] set; }
        public virtual TableRequestOptions TableRequestOptions { get; [param: NotNull] set; }
    }
}
