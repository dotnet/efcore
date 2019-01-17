// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Sql;
using Microsoft.EntityFrameworkCore.Cosmos.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal
{
    public class SelectExpression : Expression
    {
        private const string _rootAlias = "c";
        private readonly IQuerySource _querySource;
        private readonly ISqlGeneratorFactory _querySqlGeneratorFactory;

        public EntityProjectionExpression Projection { get; }
        public Expression FromExpression { get; }
        public Expression FilterExpression { get; private set; }

        public SelectExpression(IEntityType entityType, IQuerySource querySource, ISqlGeneratorFactory querySqlGeneratorFactory)
        {
            Projection = new EntityProjectionExpression(entityType, _rootAlias);
            FromExpression = new RootReferenceExpression(entityType, _rootAlias);
            EntityType = entityType;
            FilterExpression = GetDiscriminatorPredicate(entityType);
            _querySource = querySource;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        public BinaryExpression GetDiscriminatorPredicate(IEntityType entityType)
        {
            if (!EntityType.IsAssignableFrom(entityType))
            {
                return null;
            }

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToList();

            var discriminatorProperty = entityType.Cosmos().DiscriminatorProperty;

            var discriminatorPredicate = Equal(
                new KeyAccessExpression(discriminatorProperty, FromExpression),
                Constant(concreteEntityTypes[0].Cosmos().DiscriminatorValue, discriminatorProperty.ClrType));

            if (concreteEntityTypes.Count > 1)
            {
                discriminatorPredicate
                    = concreteEntityTypes
                        .Skip(1)
                        .Select(
                            concreteEntityType
                                => Constant(concreteEntityType.Cosmos().DiscriminatorValue, discriminatorProperty.ClrType))
                        .Aggregate(
                            discriminatorPredicate, (current, discriminatorValue) =>
                                OrElse(
                                    Equal(new KeyAccessExpression(discriminatorProperty, FromExpression), discriminatorValue),
                                    current));
            }

            return discriminatorPredicate;
        }

        public Expression BindPropertyPath(
            QuerySourceReferenceExpression querySourceReferenceExpression, List<IPropertyBase> properties)
        {
            if (querySourceReferenceExpression == null
                || querySourceReferenceExpression.ReferencedQuerySource != _querySource)
            {
                return null;
            }

            var currentExpression = FromExpression;

            foreach (var property in properties)
            {
                currentExpression = new KeyAccessExpression(property, currentExpression);
            }

            return currentExpression;
        }

        public void AddToPredicate(Expression predicate)
        {
            FilterExpression = AndAlso(FilterExpression, predicate);
        }

        public override Type Type => typeof(JObject);
        public override ExpressionType NodeType => ExpressionType.Extension;

        public IEntityType EntityType { get; }

        /// <summary>
        ///     Creates the default query SQL generator.
        /// </summary>
        /// <returns>
        ///     The new default query SQL generator.
        /// </returns>
        public virtual ISqlGenerator CreateDefaultQuerySqlGenerator()
            => _querySqlGeneratorFactory.CreateDefault(this);

        /// <summary>
        ///     Creates the FromSql query SQL generator.
        /// </summary>
        /// <param name="sql"> The SQL. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <returns>
        ///     The new FromSql query SQL generator.
        /// </returns>
        public virtual ISqlGenerator CreateFromSqlQuerySqlGenerator(
            [NotNull] string sql,
            [NotNull] Expression arguments)
            => _querySqlGeneratorFactory.CreateFromSql(this, sql, arguments);

        public CosmosSqlQuery ToSqlQuery(IReadOnlyDictionary<string, object> parameterValues)
            => CreateDefaultQuerySqlGenerator().GenerateSqlQuery(parameterValues);

        public override string ToString()
            => ToSqlQuery(new Dictionary<string, object>()).Query;
    }
}
