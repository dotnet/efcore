// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal
{
    public class EntityShaper : IShaper
    {
        private readonly IEntityType _entityType;
        private readonly bool _trackingQuery;
        private readonly bool _useQueryBuffer;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public EntityShaper(
            IEntityType entityType,
            bool trackingQuery,
            bool useQueryBuffer,
            IEntityMaterializerSource entityMaterializerSource)
        {
            _entityType = entityType;
            _entityMaterializerSource = entityMaterializerSource;
            _trackingQuery = trackingQuery;
            _useQueryBuffer = useQueryBuffer;
        }

        public virtual LambdaExpression CreateShaperLambda()
        {
            var valueBufferFactory = ValueBufferFactoryFactory.Create(_entityType);

            var materializationContextParameter
                            = Expression.Parameter(typeof(MaterializationContext), "materializationContext");
            var materializer = Expression.Lambda(_entityMaterializerSource
                    .CreateMaterializeExpression(
                        _entityType, materializationContextParameter), materializationContextParameter);

            var jObjectParameter = Expression.Parameter(typeof(JObject), "jObject");
            var shapeMethodInfo = _useQueryBuffer ? _bufferedShapeMethodInfo : _shapeMethodInfo;

            return Expression.Lambda(
                Expression.Call(
                    shapeMethodInfo.MakeGenericMethod(_entityType.ClrType),
                    jObjectParameter,
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(_entityType.FindPrimaryKey()),
                    Expression.Constant(_trackingQuery),
                    valueBufferFactory,
                    materializer),
                jObjectParameter);
        }

        private static readonly MethodInfo _shapeMethodInfo
            = typeof(EntityShaper).GetTypeInfo().GetDeclaredMethod(nameof(Shape));

        [UsedImplicitly]
        private static T Shape<T>(
            JObject innerObject,
            QueryContext queryContext,
            IKey key,
            bool trackingQuery,
            Func<JObject, object[]> valueBufferFactory,
            Func<MaterializationContext, object> materializer)
        {
            var valueBuffer = new ValueBuffer(valueBufferFactory(innerObject));

            if (trackingQuery)
            {
                var entry = queryContext.StateManager.TryGetEntry(key, valueBuffer, throwOnNullKey: true);

                if (entry != null)
                {
                    return (T)entry.Entity;
                }
            }

            return (T)materializer(new MaterializationContext(valueBuffer, queryContext.Context));
        }

        private static readonly MethodInfo _bufferedShapeMethodInfo
            = typeof(EntityShaper).GetTypeInfo().GetDeclaredMethod(nameof(BufferedShape));

        [UsedImplicitly]
        private static T BufferedShape<T>(
            JObject innerObject,
            QueryContext queryContext,
            IKey key,
            bool trackingQuery,
            Func<JObject, object[]> valueBufferFactory,
            Func<MaterializationContext, object> materializer)
        {
            var valueBuffer = new ValueBuffer(valueBufferFactory(innerObject));

            return (T)queryContext.QueryBuffer
                    .GetEntity(
                        key,
                        new EntityLoadInfo(
                            new MaterializationContext(valueBuffer, queryContext.Context),
                            materializer),
                        queryStateManager: trackingQuery,
                        throwOnNullKey: true);
        }

        public virtual Type Type => _entityType.ClrType;
    }
}
