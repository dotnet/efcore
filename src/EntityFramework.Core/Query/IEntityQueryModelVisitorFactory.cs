// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public interface IEntityQueryModelVisitorFactory
    {
        EntityQueryModelVisitor Create([NotNull] QueryCompilationContext queryCompilationContext, [NotNull] IDatabase database);

        EntityQueryModelVisitor Create(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IDatabase database,
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
