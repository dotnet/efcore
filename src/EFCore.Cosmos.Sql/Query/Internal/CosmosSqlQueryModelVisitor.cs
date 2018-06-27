// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal
{
    public class CosmosSqlQueryModelVisitor : EntityQueryModelVisitor
    {
        public CosmosSqlQueryModelVisitor(EntityQueryModelVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
        }

        protected override void TrackEntitiesInResults<TResult>([NotNull] QueryModel queryModel)
        {
            // Disable tracking from here and enable that from EntityShaperExpression directly
            //base.TrackEntitiesInResults<TResult>(queryModel);
        }
    }
}
