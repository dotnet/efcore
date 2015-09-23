// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class InMemoryQueryContext : QueryContext
    {
        public InMemoryQueryContext([NotNull] IQueryBuffer queryBuffer, [NotNull] IInMemoryStore store)
            : base(Check.NotNull(queryBuffer, nameof(queryBuffer)))
        {
            Store = store;
        }

        public virtual IInMemoryStore Store { get; }
    }
}
