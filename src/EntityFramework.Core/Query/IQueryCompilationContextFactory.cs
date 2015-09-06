// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public interface IQueryCompilationContextFactory
    {
        QueryCompilationContext Create([NotNull] IDatabase database, bool async);
    }
}
