// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
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
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public EntityShaper(IEntityType entityType, IEntityMaterializerSource entityMaterializerSource)
        {
            _entityType = entityType;
            _entityMaterializerSource = entityMaterializerSource;
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

            return Expression.Lambda(
                Expression.Call(
                    typeof(EntityShaper).GetTypeInfo().GetDeclaredMethod(nameof(_Shape))
                       .MakeGenericMethod(_entityType.ClrType),
                    jObjectParameter,
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(_entityType),
                    valueBufferFactory,
                    materializer),
                jObjectParameter);
        }

        private static T _Shape<T>(
            JObject innerObject,
            QueryContext queryContext,
            IEntityType entityType,
            Func<JObject, object[]> valueBufferFactory,
            Func<MaterializationContext, T> materializer)
        {
            var valueBuffer = new ValueBuffer(valueBufferFactory(innerObject));

            var entity = materializer(new MaterializationContext(valueBuffer, queryContext.Context));

            queryContext.StateManager.StartTrackingFromQuery(entityType, entity, valueBuffer, handledForeignKeys: null);

            return entity;
        }

        public virtual Type Type => _entityType.ClrType;
    }
}
