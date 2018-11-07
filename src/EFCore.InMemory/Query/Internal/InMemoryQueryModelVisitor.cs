// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryQueryModelVisitor : EntityQueryModelVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo EntityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(EntityQuery));

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityQuery<TEntity>(
            QueryContext queryContext,
            IEntityType entityType,
            IKey key,
            Func<IEntityType, MaterializationContext, object> materializer,
            bool queryStateManager)
            where TEntity : class
            => ((InMemoryQueryContext)queryContext).Store
                .GetTables(entityType)
                .SelectMany(
                    t =>
                        t.Rows.Select(
                            vs =>
                            {
                                var valueBuffer = new ValueBuffer(vs);

                                return (TEntity)queryContext
                                    .QueryBuffer
                                    .GetEntity(
                                        key,
                                        new EntityLoadInfo(
                                            new MaterializationContext(
                                                valueBuffer,
                                                queryContext.Context),
                                            c => materializer(t.EntityType, SnapshotValueBuffer(t.EntityType, c))),
                                        queryStateManager,
                                        throwOnNullKey: false);
                            }));

        private static MaterializationContext SnapshotValueBuffer(
            IEntityType entityType,
            MaterializationContext c)
        {
            var comparers = GetStructuralComparers(entityType.GetProperties());

            var copy = new object[comparers.Count];
            for (var index = 0; index < comparers.Count; index++)
            {
                copy[index] = SnapshotValue(comparers[index], c.ValueBuffer[index]);
            }

            return new MaterializationContext(new ValueBuffer(copy), c.Context);
        }

        private static object SnapshotValue(ValueComparer comparer, object value)
            => comparer == null ? value : comparer.Snapshot(value);

        private static List<ValueComparer> GetStructuralComparers(IEnumerable<IProperty> properties)
            => properties.Select(p => p.GetStructuralValueComparer() ?? p.FindMapping()?.StructuralComparer).ToList();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo ProjectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(ProjectionQuery));

        [UsedImplicitly]
        private static IEnumerable<ValueBuffer> ProjectionQuery(
            QueryContext queryContext,
            IEntityType entityType)
            => ((InMemoryQueryContext)queryContext).Store
                .GetTables(entityType)
                .SelectMany(t => t.Rows.Select(vs => new ValueBuffer(vs)));
    }
}
