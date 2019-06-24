// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class ShaperExpressionProcessingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _selectExpression;
        private readonly ParameterExpression _dataReaderParameter;
        private readonly ParameterExpression _resultCoordinatorParameter;
        private readonly ParameterExpression _indexMapParameter;
        private IDictionary<Expression, ParameterExpression> _mapping = new Dictionary<Expression, ParameterExpression>();
        private List<ParameterExpression> _variables = new List<ParameterExpression>();
        private List<Expression> _expressions = new List<Expression>();
        private List<CollectionPopulatingExpression> _collectionPopulatingExpressions = new List<CollectionPopulatingExpression>();

        public ShaperExpressionProcessingExpressionVisitor(
            SelectExpression selectExpression,
            ParameterExpression dataReaderParameter,
            ParameterExpression resultCoordinatorParameter,
            ParameterExpression indexMapParameter)
        {
            _selectExpression = selectExpression;
            _dataReaderParameter = dataReaderParameter;
            _resultCoordinatorParameter = resultCoordinatorParameter;
            _indexMapParameter = indexMapParameter;
        }

        public Expression Inject(Expression expression)
        {
            var result = Visit(expression);

            if (_collectionPopulatingExpressions.Count > 0)
            {
                _expressions.Add(result);
                result = Expression.Block(_variables, _expressions);
                _expressions.Clear();
                _variables.Clear();

                var resultParameter = Expression.Parameter(result.Type, "result");

                _expressions.Add(
                    Expression.IfThen(
                        Expression.Equal(resultParameter, Expression.Default(result.Type)),
                        Expression.Assign(resultParameter, result)));
                _expressions.AddRange(_collectionPopulatingExpressions);
                _expressions.Add(resultParameter);

                return ConvertToLambda(Expression.Block(_expressions), resultParameter);
            }
            else if (_expressions.All(e => e.NodeType == ExpressionType.Assign))
            {
                result = new ReplacingExpressionVisitor(_expressions.Cast<BinaryExpression>()
                    .ToDictionary(e => e.Left, e => e.Right)).Visit(result);
            }
            else
            {
                _expressions.Add(result);
                result = Expression.Block(_variables, _expressions);
            }

            return ConvertToLambda(result, Expression.Parameter(result.Type, "result"));
        }

        private LambdaExpression ConvertToLambda(Expression result, ParameterExpression resultParameter)
        {
            return _indexMapParameter != null
                ? Expression.Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    resultParameter,
                    _indexMapParameter,
                    _resultCoordinatorParameter)
                : Expression.Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    resultParameter,
                    _resultCoordinatorParameter);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                var key = GenerateKey(entityShaperExpression.ValueBufferExpression);
                if (!_mapping.TryGetValue(key, out var variable))
                {
                    variable = Expression.Parameter(entityShaperExpression.EntityType.ClrType);
                    _variables.Add(variable);
                    _expressions.Add(Expression.Assign(variable, entityShaperExpression));
                    _mapping[key] = variable;
                }

                return variable;
            }

            if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                var key = GenerateKey(projectionBindingExpression);
                if (!_mapping.TryGetValue(key, out var variable))
                {
                    variable = Expression.Parameter(projectionBindingExpression.Type);
                    _variables.Add(variable);
                    _expressions.Add(Expression.Assign(variable, projectionBindingExpression));
                    _mapping[key] = variable;
                }

                return variable;
            }

            if (extensionExpression is IncludeExpression includeExpression)
            {
                var entity = Visit(includeExpression.EntityExpression);
                if (includeExpression.NavigationExpression is RelationalCollectionShaperExpression relationalCollectionShaperExpression)
                {
                    var innerShaper = new ShaperExpressionProcessingExpressionVisitor(
                        _selectExpression, _dataReaderParameter, _resultCoordinatorParameter, null)
                                .Inject(relationalCollectionShaperExpression.InnerShaper);

                    _expressions.Add(new CollectionInitializingExperssion(
                        relationalCollectionShaperExpression.CollectionId,
                        entity,
                        relationalCollectionShaperExpression.OuterIdentifier,
                        includeExpression.Navigation));

                    _collectionPopulatingExpressions.Add(new CollectionPopulatingExpression(
                            relationalCollectionShaperExpression.Update(
                                relationalCollectionShaperExpression.ParentIdentifier,
                                relationalCollectionShaperExpression.OuterIdentifier,
                                relationalCollectionShaperExpression.SelfIdentifier,
                                innerShaper),
                            true));
                }
                else
                {
                    _expressions.Add(includeExpression.Update(
                        entity,
                        Visit(includeExpression.NavigationExpression)));
                }

                return entity;
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression GenerateKey(ProjectionBindingExpression projectionBindingExpression)
        {
            return projectionBindingExpression.ProjectionMember != null
                ? _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                : projectionBindingExpression;
        }
    }

    public class CollectionInitializingExperssion : Expression, IPrintable
    {
        public CollectionInitializingExperssion(int collectionId, Expression parent, Expression outerIdentifier, INavigation navigation)
        {
            CollectionId = collectionId;
            Parent = parent;
            OuterIdentifier = outerIdentifier;
            Navigation = navigation;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parent = visitor.Visit(Parent);
            var outerIdentifier = visitor.Visit(OuterIdentifier);

            return parent != Parent || outerIdentifier != OuterIdentifier
                ? new CollectionInitializingExperssion(CollectionId, parent, outerIdentifier, Navigation)
                : this;
        }

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("InitializeCollection:");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.StringBuilder.AppendLine($"CollectionId: {CollectionId}");
                expressionPrinter.StringBuilder.AppendLine($"Navigation: {Navigation.Name}");
                expressionPrinter.StringBuilder.Append("Parent:");
                expressionPrinter.Visit(Parent);
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("OuterIdentifier:");
                expressionPrinter.Visit(OuterIdentifier);
                expressionPrinter.StringBuilder.AppendLine();
            }
        }

        public override Type Type => Navigation.ClrType;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public int CollectionId { get; }
        public Expression Parent { get; }
        public Expression OuterIdentifier { get; }
        public INavigation Navigation { get; }
    }

    public class CollectionPopulatingExpression : Expression, IPrintable
    {
        public CollectionPopulatingExpression(RelationalCollectionShaperExpression parent, bool include)
        {
            Parent = parent;
            Include = include;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parent = (RelationalCollectionShaperExpression)visitor.Visit(Parent);

            return parent != Parent
                ? new CollectionPopulatingExpression(parent, Include)
                : this;
        }

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("PopulateCollection:");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.StringBuilder.Append("Parent:");
                expressionPrinter.Visit(Parent);
            }
        }

        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;
        public RelationalCollectionShaperExpression Parent { get; }
        public bool Include { get; }
    }
}
