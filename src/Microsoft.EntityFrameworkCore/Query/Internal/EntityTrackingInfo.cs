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
    public class EntityTrackingInfo
    {
        private static readonly IEnumerable<IncludedEntity> _emptyIncludedEntities
            = Enumerable.Empty<IncludedEntity>();

        private readonly IEntityType _entityType;
        private readonly IReadOnlyList<IReadOnlyList<INavigation>> _includedNavigationPaths;
        private readonly IDictionary<INavigation, IEntityType> _includedEntityTrackingInfos;

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

                foreach (var navigation
                    in _includedNavigationPaths.SelectMany(ns => ns))
                {
                    if (!_includedEntityTrackingInfos.ContainsKey(navigation))
                    {
                        var targetEntityType = navigation.GetTargetType();

                        _includedEntityTrackingInfos.Add(navigation, targetEntityType);
                    }
                }
            }
        }

        public virtual bool IsEnumerableTarget { get; }

        public virtual QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }
        public virtual IQuerySource QuerySource => QuerySourceReferenceExpression.ReferencedQuerySource;

        public virtual void StartTracking(
            [NotNull] IStateManager stateManager, [NotNull] object entity, ValueBuffer valueBuffer)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entity, nameof(entity));

            stateManager.StartTrackingFromQuery(_entityType, entity, valueBuffer);
        }

        public struct IncludedEntity
        {
            public IncludedEntity(
                [NotNull] object entity, [NotNull] IEntityType entityType)
            {
                Check.NotNull(entity, nameof(entity));
                Check.NotNull(entityType, nameof(entityType));

                EntityType = entityType;
                Entity = entity;
            }

            public object Entity { get; }

            private IEntityType EntityType { get; }

            public void StartTracking([NotNull] IStateManager stateManager, ValueBuffer valueBuffer)
            {
                Check.NotNull(stateManager, nameof(stateManager));

                stateManager.StartTrackingFromQuery(
                    EntityType,
                    Entity,
                    valueBuffer);
            }
        }

        public virtual IEnumerable<IncludedEntity> GetIncludedEntities([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            if (_includedNavigationPaths == null)
            {
                return _emptyIncludedEntities;
            }

            return _includedNavigationPaths
                .SelectMany(navigations => GetIncludedEntities(entity, navigations, index: 0));
        }

        private IEnumerable<IncludedEntity> GetIncludedEntities(
            object entity, IReadOnlyList<INavigation> navigationPath, int index)
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
                        yield return new IncludedEntity(referencedEntity, _includedEntityTrackingInfos[navigation]);

                        foreach (var includedEntity
                            in GetIncludedEntities(referencedEntity, navigationPath, index + 1))
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
                        yield return new IncludedEntity(referencedEntity, _includedEntityTrackingInfos[navigation]);

                        foreach (var includedEntity
                            in GetIncludedEntities(referencedEntity, navigationPath, index + 1))
                        {
                            yield return includedEntity;
                        }
                    }
                }
            }
        }
    }
}
