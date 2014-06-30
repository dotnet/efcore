// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public abstract class AtsAsyncRequest<TResult> : AtsRequest<TResult>
    {
        public abstract Task<TResult> ExecuteAsync([NotNull] RequestContext requestContext, CancellationToken cancellationToken = default(CancellationToken));

        public override TResult Execute([NotNull] RequestContext requestContext)
        {
            Check.NotNull(requestContext, "requestContext");
            //TODO may not be a sensible default for all commands
            return ExecuteAsync(requestContext).Result;
        }
    }
}
