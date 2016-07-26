// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityTrackingInfo
    {
        private static readonly IEnumerable<IncludedEntity> _emptyIncludedEntities
            = Enumerable.Empty<IncludedEntity>();

        private readonly IEntityType _entityType;
        private readonly IReadOnlyList<IReadOnlyList<INavigation>> _includedNavigationPaths;
        private readonly IDictionary<INavigation, IEntityType> _includedEntityTrackingInfos;
        private readonly ISet<IForeignKey> _handledForeignKeys;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityTrackingInfo(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QuerySourceReferenceExpression querySourceReferenceExpression,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(querySourceReferenceExpression, nameof(querySourceReferenceExpression));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            QuerySourceReferenceExpression = querySourceReferenceExpression;

            var sequenceType = querySourceReferenceExpression.Type.TryGetSequenceType();

            IsEnumerableTarget
                = sequenceType != null
                  && sequenceType == entityType.ClrType;

            _entityType = entityType;

            _includedNavigationPaths
                = queryCompilationContext
                    .GetTrackableIncludes(querySourceReferenceExpression.ReferencedQuerySource);

            if (_includedNavigationPaths != null)
            {
                _includedEntityTrackingInfos = new Dictionary<INavigation, IEntityType>();
                _handledForeignKeys = new HashSet<IForeignKey>();

                foreach (var navigation
                    in _includedNavigationPaths.SelectMany(ns => ns))
                {
                    if (!_includedEntityTrackingInfos.ContainsKey(navigation))
                    {
                        var targetEntityType = navigation.GetTargetType();

                        _includedEntityTrackingInfos.Add(navigation, targetEntityType);
                        _handledForeignKeys.Add(navigation.ForeignKey);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsEnumerableTarget { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IQuerySource QuerySource => QuerySourceReferenceExpression.ReferencedQuerySource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StartTracking(
            [NotNull] IStateManager stateManager, [NotNull] object entity, ValueBuffer valueBuffer)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entity, nameof(entity));

            stateManager.StartTrackingFromQuery(_entityType, entity, valueBuffer, _handledForeignKeys);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public struct IncludedEntity
        {
            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public IncludedEntity(
                [NotNull] object entity, [NotNull] IEntityType entityType, [CanBeNull] ISet<IForeignKey> handledForeignKeys)
            {
                EntityType = entityType;
                Entity = entity;
                HandledForeignKeys = handledForeignKeys;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public object Entity { get; }

            private IEntityType EntityType { get; }

            private ISet<IForeignKey> HandledForeignKeys { get; }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public void StartTracking([NotNull] IStateManager stateManager, ValueBuffer valueBuffer)
            {
                Check.NotNull(stateManager, nameof(stateManager));

                stateManager.StartTrackingFromQuery(
                    EntityType,
                    Entity,
                    valueBuffer,
                    HandledForeignKeys);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IncludedEntity> GetIncludedEntities(
            [NotNull] IStateManager stateManager, 
            [NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            if (_includedNavigationPaths == null)
            {
                return _emptyIncludedEntities;
            }

            return _includedNavigationPaths
                .SelectMany(navigations => GetIncludedEntities(stateManager, entity, navigations, index: 0));
        }

        private IEnumerable<IncludedEntity> GetIncludedEntities(
            IStateManager stateManager, object entity, IReadOnlyList<INavigation> navigationPath, int index)
        {
            if (index < navigationPath.Count)
            {
                var navigation = navigationPath[index];

                if (navigation.IsCollection())
                {
                    var propertyGetter = navigation.GetGetter();
                    var referencedEntities = (IEnumerable<object>)propertyGetter.GetClrValue(entity);

                    foreach (var referencedEntity
                        in referencedEntities.Where(referencedEntity => referencedEntity != null))
                    {
                        yield return new IncludedEntity(
                            referencedEntity, 
                            _includedEntityTrackingInfos[navigation],
                            _handledForeignKeys);

                        foreach (var includedEntity
                            in GetIncludedEntities(stateManager, referencedEntity, navigationPath, index + 1))
                        {
                            yield return includedEntity;
                        }
                    }
                }
                else
                {
                    var referencedEntity = navigation.GetGetter().GetClrValue(entity);

                    if (referencedEntity != null)
                    {
                        yield return new IncludedEntity(
                            referencedEntity, 
                            _includedEntityTrackingInfos[navigation],
                            _handledForeignKeys);

                        foreach (var includedEntity
                            in GetIncludedEntities(stateManager, referencedEntity, navigationPath, index + 1))
                        {
                            yield return includedEntity;
                        }
                    }
                }

                stateManager.TryGetEntry(entity)?.SetIsLoaded(navigation);
            }
        }
    }
}
