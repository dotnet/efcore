// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         An expression that represents creation of an entity instance for Cosmos provider in
    ///         <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class FromSqlCosmosEntityShaperExpression : EntityShaperExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="FromSqlCosmosEntityShaperExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="valueBufferExpression"> An expression of ValueBuffer to get values for properties of the entity. </param>
        /// <param name="nullable"> A bool value indicating whether this entity instance can be null. </param>
        public FromSqlCosmosEntityShaperExpression(IEntityType entityType, Expression valueBufferExpression, bool nullable)
            : base(entityType, valueBufferExpression, nullable, null)
        {
        }

        /// <inheritdoc />
        protected override LambdaExpression GenerateMaterializationCondition(IEntityType entityType, bool nullable)
        {
            Check.NotNull(entityType, nameof(EntityType));

            var valueBufferParameter = Parameter(typeof(ValueBuffer));
            Expression body;
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToArray();
            
            body = Constant(concreteEntityTypes.Length == 1 ? concreteEntityTypes[0] : entityType, typeof(IEntityType));

            return Lambda(body, valueBufferParameter);
        }
    }
}
