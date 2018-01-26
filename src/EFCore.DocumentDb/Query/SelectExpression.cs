// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SelectExpression : SourceExpression
    {
        private ExpressionEqualityComparer _expressionEqualityComparer = new ExpressionEqualityComparer();
        public List<Expression> Projection { get; } = new List<Expression>();
        public List<SourceExpression> Source { get; } = new List<SourceExpression>();
        public Expression Predicate { get => _filterCondition; set => _filterCondition = value; }

        private Expression _filterCondition;
        //private List<Ordering> _sortSpecifications = new List<Ordering>();

        public SelectExpression(IQuerySource querySource, string alias)
            : base(querySource, alias)
        {
        }

        public void AddSource(SourceExpression fromExpression)
        {
            Source.Add(fromExpression);
        }

        public Expression BindProperty(
            IProperty property,
            IQuerySource querySource)
        {
            if (Source.Where(f => f.QuerySource == querySource)
                .FirstOrDefault() is CollectionExpression fromExpression)
            {
                return new ColumnExpression(property.Name, property, fromExpression);
            }

            return null;
        }

        public int AddToProjection(IProperty property, IQuerySource querySource)
        {
            var boundProperty = BindProperty(property, querySource);

            return AddToProjection(boundProperty);
        }

        public int AddToProjection(Expression projection)
        {
            var projectionIndex = Projection.FindIndex(e => _expressionEqualityComparer.Equals(e, projection));
            if (projectionIndex != -1)
            {
                return projectionIndex;
            }

            Projection.Add(projection);

            return Projection.Count - 1;
        }

        public void AddToPredicate(Expression filterCondition)
        {
            if (Predicate == null)
            {
                Predicate = filterCondition;
            }
            else
            {
                Predicate = AndAlso(Predicate, filterCondition);
            }
        }

        public override bool HandlesQuerySource(IQuerySource querySource)
        {
            return Source.Any(f => f.QuerySource == querySource || f.HandlesQuerySource(querySource));
        }

        public QuerySqlGenerator GetSqlGenerator()
        {
            return new QuerySqlGenerator(this);
        }

        public IEnumerable<IProperty> GetProjectedProperties()
        {
            return Projection.Select(e => e is ColumnExpression ce ? ce.Property : null);
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return GetSqlGenerator().GenerateSql();
        }
    }
}
