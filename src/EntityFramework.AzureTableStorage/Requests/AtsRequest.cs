// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public abstract class AtsRequest<TResult>
    {
        public abstract string Name { get; }
        public abstract TResult Execute([NotNull] RequestContext requestContext);
    }
}
