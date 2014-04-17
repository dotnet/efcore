// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.InMemory
{
    public partial class InMemoryDataStore
    {
        private class QueryModelVisitor : EntityQueryModelVisitor
        {
            private static readonly MethodInfo _entityScanMethodInfo
                = typeof(QueryModelVisitor).GetMethod("EntityScan", BindingFlags.NonPublic | BindingFlags.Static);

            public QueryModelVisitor()
                : base(_entityScanMethodInfo, p => new QueryModelVisitor(p))
            {
            }

            private QueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
                : base(parentQueryModelVisitor, _entityScanMethodInfo, p => new QueryModelVisitor(p))
            {
            }

            [UsedImplicitly]
            private static IEnumerable<TEntity> EntityScan<TEntity>(QueryContext queryContext)
            {
                var entityType = queryContext.Model.GetEntityType(typeof(TEntity));

                return ((InMemoryQueryContext)queryContext).Database.GetTable(entityType)
                    .Select(t => (TEntity)queryContext.StateManager
                        .GetOrMaterializeEntry(entityType, new ObjectArrayValueReader(t)).Entity);
            }
        }
    }
}
