// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public abstract class AsyncQueryCompilationContext : QueryCompilationContext
    {
        protected AsyncQueryCompilationContext([NotNull] IModel model)
            : base(Check.NotNull(model, "model"), new AsyncLinqOperatorProvider(), new AsyncResultOperatorHandler())
        {
        }
    }
}
