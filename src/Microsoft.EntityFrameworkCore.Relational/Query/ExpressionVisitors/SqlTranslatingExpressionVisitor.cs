// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

// ReSharper disable AssignNullToNotNullAttribute
namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public class SqlTranslatingExpressionVisitor : ThrowingExpressionVisitor
    {
        private static readonly Dictionary<ExpressionType, ExpressionType> _inverseOperatorMap 
            = new Dictionary<ExpressionType, ExpressionType>
        {
            { ExpressionType.LessThan, ExpressionType.GreaterThanOrEqual },
            { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThan },
            { ExpressionType.GreaterThan, ExpressionType.LessThanOrEqual },
            { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan },
            { ExpressionType.Equal, ExpressionType.NotEqual },
            { ExpressionType.NotEqual, ExpressionType.Equal }
        };

        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IExpressionFragmentTranslator _compositeExpressionFragmentTranslator;
        private readonly IMethodCallTranslator _methodCallTranslator;
        private readonly IMemberTranslator _memberTranslator;
        private readonly RelationalQueryModelVisitor _queryModelVisitor;
        private readonly IRelationalTypeMapper _relationalTypeMapper;
        private readonly SelectExpression _targetSelectExpression;
        private readonly Expression _topLevelPredicate;

        private readonly bool _bindParentQueries;
        private readonly bool _inProjection;

        public SqlTranslatingExpressionVisitor(
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IExpressionFragmentTranslator compositeExpressionFragmentTranslator,
            [NotNull] IMethodCallTranslator methodCallTranslator,
            [NotNull] IMemberTranslator memberTranslator,
            [NotNull] IRelationalTypeMapper relationalTypeMapper,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool bindParentQueries = false,
            bool inProjection = false)
        {
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(compositeExpressionFragmentTranslator, nameof(compositeExpressionFragmentTranslator));
            Check.NotNull(methodCallTranslator, nameof(methodCallTranslator));
            Check.NotNull(memberTranslator, nameof(memberTranslator));
            Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper));
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _relationalAnnotationProvider = relationalAnnotationProvider;
            _compositeExpressionFragmentTranslator = compositeExpressionFragmentTranslator;
            _methodCallTranslator = methodCallTranslator;
            _memberTranslator = memberTranslator;
            _relationalTypeMapper = relationalTypeMapper;
            _queryModelVisitor = queryModelVisitor;
            _targetSelectExpression = targetSelectExpression;
            _topLevelPredicate = topLevelPredicate;
            _bindParentQueries = bindParentQueries;
            _inProjection = inProjection;
        }

        public virtual Expression ClientEvalPredicate { get; private set; }

        public override Expression Visit(Expression expression)
        {
            var translatedExpression = _compositeExpressionFragmentTranslator.Translate(expression);

            if (translatedExpression != null
                && translatedExpression != expression)
            {
                return Visit(translatedExpression);
            }

            return base.Visit(expression);
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (expression.NodeType)
            {
                case ExpressionType.Coalesce:
                {
                    var left = Visit(expression.Left);
                    var right = Visit(expression.Right);

                    return left != null && right != null
                        ? new AliasExpression(expression.Update(left, expression.Conversion, right))
                        : null;
                }

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                {
                    var structuralComparisonExpression
                        = UnfoldStructuralComparison(
                            expression.NodeType,
                            ProcessComparisonExpression(expression));

                    return structuralComparisonExpression;
                }

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                {
                    return ProcessComparisonExpression(expression);
                }

                case ExpressionType.AndAlso:
                {
                    var left = Visit(expression.Left);
                    var right = Visit(expression.Right);

                    if (expression == _topLevelPredicate)
                    {
                        if (left != null
                            && right != null)
                        {
                            return Expression.AndAlso(left, right);
                        }

                        if (left != null)
                        {
                            ClientEvalPredicate = expression.Right;
                            return left;
                        }

                        if (right != null)
                        {
                            ClientEvalPredicate = expression.Left;
                            return right;
                        }

                        return null;
                    }

                    return left != null && right != null
                        ? Expression.AndAlso(left, right)
                        : null;
                }

                case ExpressionType.OrElse:
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.Or:
                    {
                    var leftExpression = Visit(expression.Left);
                    var rightExpression = Visit(expression.Right);

                    return leftExpression != null && rightExpression != null
                        ? Expression.MakeBinary(
                            expression.NodeType, 
                            leftExpression, 
                            rightExpression,
                            expression.IsLiftedToNull,
                            expression.Method)
                        : null;
                }
            }

            return null;
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var nullCheckRemoved = TryRemoveNullCheck(expression);
            if (nullCheckRemoved != null)
            {
                return Visit(nullCheckRemoved);
            }

            var test = Visit(expression.Test);
            if (test.IsSimpleExpression())
            {
                test = Expression.Equal(test, Expression.Constant(true, typeof(bool)));
            }

            var ifTrue = Visit(expression.IfTrue);
            var ifFalse = Visit(expression.IfFalse);

            if (test != null
                && ifTrue != null
                && ifFalse != null)
            {
                if (ifTrue.IsComparisonOperation()
                    || ifFalse.IsComparisonOperation())
                {
                    return Expression.OrElse(
                        Expression.AndAlso(test, ifTrue),
                        Expression.AndAlso(Invert(test), ifFalse));
                }

                return expression.Update(test, ifTrue, ifFalse);
            }

            return null;
        }

        private static Expression Invert(Expression test)
        {
            if (test.IsComparisonOperation())
            {
                var binaryOperation = test as BinaryExpression;
                if (binaryOperation != null)
                {
                    var nodeType = binaryOperation.NodeType;
                    if (!_inverseOperatorMap.ContainsKey(nodeType))
                    {
                        return null;
                    }
                    return Expression.MakeBinary(_inverseOperatorMap[nodeType], binaryOperation.Left, binaryOperation.Right);
                }

                return Expression.Not(test);
            }

            return null;
        }

        private static Expression TryRemoveNullCheck(ConditionalExpression node)
        {
            var binaryTest = node.Test as BinaryExpression;
            if (binaryTest == null
                || !(binaryTest.NodeType == ExpressionType.Equal 
                    || binaryTest.NodeType == ExpressionType.NotEqual))
            {
                return null;
            }

            var leftConstant = binaryTest.Left as ConstantExpression;
            var isLeftNullConstant = leftConstant != null && leftConstant.Value == null;

            var rightConstant = binaryTest.Right as ConstantExpression;
            var isRightNullConstant = rightConstant != null && rightConstant.Value == null;

            if (isLeftNullConstant == isRightNullConstant) 
            {
                return null;
            }

            if (binaryTest.NodeType == ExpressionType.Equal)
            {
                var ifTrueConstant = node.IfTrue as ConstantExpression;
                if (ifTrueConstant == null || ifTrueConstant.Value != null)
                {
                    return null;
                }
            }
            else
            {
                var ifFalseConstant = node.IfFalse as ConstantExpression;
                if (ifFalseConstant == null || ifFalseConstant.Value != null)
                {
                    return null;
                }
            }

            var testExpression = isLeftNullConstant ? binaryTest.Right : binaryTest.Left;
            var resultExpression = binaryTest.NodeType == ExpressionType.Equal ? node.IfFalse : node.IfTrue;

            var nullCheckRemovalTestingVisitor = new NullCheckRemovalTestingVisitor();
            if (nullCheckRemovalTestingVisitor.CanRemoveNullCheck(testExpression, resultExpression))
            {
                return resultExpression;
            }

            return null;
        }

        private class NullCheckRemovalTestingVisitor : ExpressionVisitorBase
        {
            private IQuerySource _querySource;
            private string _propertyName;
            private bool? _canRemoveNullCheck;

            public bool CanRemoveNullCheck(Expression testExpression, Expression resultExpression)
            {
                AnalyzeTestExpression(testExpression);
                if (_querySource == null)
                {
                    return false;
                }

                Visit(resultExpression);

                return _canRemoveNullCheck ?? false;
            }

            public override Expression Visit([CanBeNull] Expression node)
            {
                return _canRemoveNullCheck == false ? node : base.Visit(node);
            }

            private void AnalyzeTestExpression(Expression expression)
            {
                var querySourceReferenceExpression = expression as QuerySourceReferenceExpression;
                if (querySourceReferenceExpression != null)
                {
                    _querySource = querySourceReferenceExpression.ReferencedQuerySource;
                    _propertyName = null;

                    return;
                }

                var memberExpression = expression as MemberExpression;
                if (memberExpression != null)
                {
                    var querySourceInstance = memberExpression.Expression as QuerySourceReferenceExpression;
                    if (querySourceInstance != null)
                    {
                        _querySource = querySourceInstance.ReferencedQuerySource;
                        _propertyName = memberExpression.Member.Name;

                        return;
                    }
                }

                var methodCallExpression = expression as MethodCallExpression;
                if (methodCallExpression != null && EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
                {
                    var querySourceCaller = methodCallExpression.Arguments[0] as QuerySourceReferenceExpression;
                    if (querySourceCaller != null)
                    {
                        var propertyNameExpression = methodCallExpression.Arguments[1] as ConstantExpression;
                        if (propertyNameExpression != null)
                        {
                            _querySource = querySourceCaller.ReferencedQuerySource;
                            _propertyName = (string)propertyNameExpression.Value;

                            return;
                        }
                    }
                }
            }

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                _canRemoveNullCheck = (expression.ReferencedQuerySource == _querySource && _propertyName == null);

                return expression;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.Name == _propertyName)
                {
                    var querySource = node.Expression as QuerySourceReferenceExpression;
                    if (querySource != null)
                    {
                        _canRemoveNullCheck = querySource.ReferencedQuerySource == _querySource;

                        return node;
                    }
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (EntityQueryModelVisitor.IsPropertyMethod(node.Method))
                {
                    var propertyNameExpression = node.Arguments[1] as ConstantExpression;
                    if (propertyNameExpression != null && (string)propertyNameExpression.Value == _propertyName)
                    {
                        var querySource = node.Arguments[0] as QuerySourceReferenceExpression;
                        if (querySource != null)
                        {
                            _canRemoveNullCheck = querySource.ReferencedQuerySource == _querySource;

                            return node;
                        }
                    }
                }

                return base.VisitMethodCall(node);
            }
        }

        private static Expression UnfoldStructuralComparison(ExpressionType expressionType, Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            var leftConstantExpression = binaryExpression?.Left as ConstantExpression;
            var leftExpressions = leftConstantExpression?.Value as Expression[];
            var rightConstantExpression = binaryExpression?.Right as ConstantExpression;
            var rightExpressions = rightConstantExpression?.Value as Expression[];

            if (leftExpressions != null
                && rightConstantExpression != null
                && rightConstantExpression.Value == null)
            {
                rightExpressions
                    = Enumerable
                        .Repeat<Expression>(rightConstantExpression, leftExpressions.Length)
                        .ToArray();
            }

            if (rightExpressions != null
                && leftConstantExpression != null
                && leftConstantExpression.Value == null)
            {
                leftExpressions
                    = Enumerable
                        .Repeat<Expression>(leftConstantExpression, rightExpressions.Length)
                        .ToArray();
            }

            if (leftExpressions != null
                && rightExpressions != null
                && leftExpressions.Length == rightExpressions.Length)
            {
                return leftExpressions
                    .Zip(rightExpressions, (l, r) =>
                        TransformNullComparison(l, r, binaryExpression.NodeType)
                        ?? Expression.MakeBinary(expressionType, l, r))
                    .Aggregate((e1, e2) =>
                        expressionType == ExpressionType.Equal
                            ? Expression.AndAlso(e1, e2)
                            : Expression.OrElse(e1, e2));
            }

            return expression;
        }

        private Expression ProcessComparisonExpression(BinaryExpression binaryExpression)
        {
            var leftExpression = Visit(binaryExpression.Left);
            var rightExpression = Visit(binaryExpression.Right);

            if (leftExpression == null
                || rightExpression == null)
            {
                return null;
            }

            var nullExpression
                = TransformNullComparison(leftExpression, rightExpression, binaryExpression.NodeType);

            if (nullExpression != null)
            {
                return nullExpression;
            }

            if (leftExpression.Type != rightExpression.Type
                && leftExpression.Type.UnwrapNullableType() == rightExpression.Type.UnwrapNullableType())
            {
                if (leftExpression.Type.IsNullableType())
                {
                    rightExpression = Expression.Convert(rightExpression, leftExpression.Type);
                }
                else
                {
                    leftExpression = Expression.Convert(leftExpression, rightExpression.Type);
                }
            }

            return leftExpression.Type == rightExpression.Type
                ? Expression.MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression)
                : null;
        }

        private static Expression TransformNullComparison(
            Expression left, Expression right, ExpressionType expressionType)
        {
            if (expressionType == ExpressionType.Equal
                || expressionType == ExpressionType.NotEqual)
            {
                var constantExpression
                    = right.RemoveConvert() as ConstantExpression
                      ?? left.RemoveConvert() as ConstantExpression;

                if (constantExpression != null
                    && constantExpression.Value == null)
                {
                    var columnExpression
                        = left.RemoveConvert().TryGetColumnExpression()
                          ?? right.RemoveConvert().TryGetColumnExpression();

                    if (columnExpression != null)
                    {
                        return expressionType == ExpressionType.Equal
                            ? (Expression)new IsNullExpression(columnExpression)
                            : Expression.Not(new IsNullExpression(columnExpression));
                    }
                }
            }

            return null;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var operand = Visit(methodCallExpression.Object);

            if (operand != null
                || methodCallExpression.Object == null)
            {
                var arguments
                    = methodCallExpression.Arguments
                        .Where(e => !(e is QuerySourceReferenceExpression)
                                    && !(e is SubQueryExpression))
                        .Select(e => (e as ConstantExpression)?.Value is Array ? e : Visit(e))
                        .Where(e => e != null)
                        .ToArray();

                if (arguments.Length == methodCallExpression.Arguments.Count)
                {
                    var boundExpression
                        = operand != null
                            ? Expression.Call(operand, methodCallExpression.Method, arguments)
                            : Expression.Call(methodCallExpression.Method, arguments);

                    var translatedExpression = _methodCallTranslator.Translate(boundExpression);

                    if (translatedExpression != null)
                    {
                        return translatedExpression;
                    }
                }
            }

            var expression
                = _queryModelVisitor
                    .BindMethodCallExpression(methodCallExpression, CreateAliasedColumnExpression)
                    ?? _queryModelVisitor.BindLocalMethodCallExpression(methodCallExpression);

            if (expression == null
                && _bindParentQueries)
            {
                expression
                    = _queryModelVisitor?.ParentQueryModelVisitor
                        .BindMethodCallExpression(methodCallExpression, CreateAliasedColumnExpressionCore);
            }

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression.Expression is QuerySourceReferenceExpression)
                && !(expression.Expression is SubQueryExpression))
            {
                var newExpression = Visit(expression.Expression);

                if (newExpression != null
                    || expression.Expression == null)
                {
                    var newMemberExpression
                        = newExpression != expression.Expression
                            ? Expression.Property(newExpression, expression.Member.Name)
                            : expression;

                    var translatedExpression = _memberTranslator.Translate(newMemberExpression);

                    if (translatedExpression != null)
                    {
                        return translatedExpression;
                    }
                }
            }

            var aliasExpression
                = _queryModelVisitor
                    .BindMemberExpression(expression, CreateAliasedColumnExpression);

            if (aliasExpression == null
                && _bindParentQueries)
            {
                aliasExpression
                    = _queryModelVisitor?.ParentQueryModelVisitor
                        .BindMemberExpression(expression, CreateAliasedColumnExpressionCore);
            }

            if (aliasExpression == null)
            {
                var querySourceReferenceExpression
                    = expression.Expression as QuerySourceReferenceExpression;

                if (querySourceReferenceExpression != null)
                {
                    var selectExpression
                        = _queryModelVisitor.TryGetQuery(querySourceReferenceExpression.ReferencedQuerySource);

                    if (selectExpression != null)
                    {
                        aliasExpression
                            = selectExpression.Projection
                                .OfType<AliasExpression>()
                                .SingleOrDefault(ae => ae.SourceMember == expression.Member);
                    }
                }
            }

            return aliasExpression;
        }

        private AliasExpression CreateAliasedColumnExpression(
            IProperty property, IQuerySource querySource, SelectExpression selectExpression)
        {
            if (_targetSelectExpression != null
                && selectExpression != _targetSelectExpression)
            {
                selectExpression?.AddToProjection(
                    _relationalAnnotationProvider.For(property).ColumnName,
                    property,
                    querySource);

                return null;
            }

            return CreateAliasedColumnExpressionCore(property, querySource, selectExpression);
        }

        private AliasExpression CreateAliasedColumnExpressionCore(
            IProperty property, IQuerySource querySource, SelectExpression selectExpression)
            => new AliasExpression(
                new ColumnExpression(
                    _relationalAnnotationProvider.For(property).ColumnName,
                    property,
                    selectExpression.GetTableForQuerySource(querySource)));

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                {
                    var operand = Visit(expression.Operand);
                    if (operand != null)
                    {
                        return Expression.Not(operand);
                    }

                    break;
                }
                case ExpressionType.Convert:
                {
                    var operand = Visit(expression.Operand);
                    if (operand != null)
                    {
                        return Expression.Convert(operand, expression.Type);
                    }

                    break;
                }
            }

            return null;
        }

        protected override Expression VisitNew(NewExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.Members != null
                && expression.Arguments.Any()
                && expression.Arguments.Count == expression.Members.Count)
            {
                var memberBindings
                    = expression.Arguments
                        .Select(Visit)
                        .Where(e => e != null)
                        .ToArray();

                if (memberBindings.Length == expression.Arguments.Count)
                {
                    return Expression.Constant(memberBindings);
                }
            }
            else if (expression.Type == typeof(CompositeKey))
            {
                var propertyCallExpressions
                    = ((NewArrayExpression)expression.Arguments.Single()).Expressions;

                var memberBindings
                    = propertyCallExpressions
                        .Select(Visit)
                        .Where(e => e != null)
                        .ToArray();

                if (memberBindings.Length == propertyCallExpressions.Count)
                {
                    return Expression.Constant(memberBindings);
                }
            }

            return null;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var subQueryModel = expression.QueryModel;

            var subQueryOutputDataInfo = subQueryModel.GetOutputDataInfo();
            if (subQueryModel.IsIdentityQuery()
                && subQueryModel.ResultOperators.Count == 1
                && subQueryModel.ResultOperators.First() is ContainsResultOperator)
            {
                var contains = (ContainsResultOperator)subQueryModel.ResultOperators.First();
                var fromExpression = subQueryModel.MainFromClause.FromExpression;

                if (fromExpression.NodeType == ExpressionType.Parameter
                    || fromExpression.NodeType == ExpressionType.Constant
                    || fromExpression.NodeType == ExpressionType.ListInit
                    || fromExpression.NodeType == ExpressionType.NewArrayInit)
                {
                    var memberItem = contains.Item as MemberExpression;

                    if (memberItem != null)
                    {
                        var aliasExpression = VisitMember(memberItem) as AliasExpression;

                        return aliasExpression != null
                            ? new InExpression(aliasExpression, new[] { fromExpression })
                            : null;
                    }

                    var methodCallItem = contains.Item as MethodCallExpression;

                    if (methodCallItem != null
                        && EntityQueryModelVisitor.IsPropertyMethod(methodCallItem.Method))
                    {
                        var aliasExpression = (AliasExpression)VisitMethodCall(methodCallItem);

                        return new InExpression(aliasExpression, new[] { fromExpression });
                    }
                }
            }
            else if (!(subQueryOutputDataInfo is StreamedSequenceInfo))
            {
                var streamedSingleValueInfo = subQueryOutputDataInfo as StreamedSingleValueInfo;
                var streamedSingleValueSupportedType = streamedSingleValueInfo != null
                    && _relationalTypeMapper.FindMapping(streamedSingleValueInfo.DataType.UnwrapNullableType().UnwrapEnumType()) != null;

                if (_inProjection
                    && !(subQueryOutputDataInfo is StreamedScalarValueInfo) 
                    && !streamedSingleValueSupportedType)
                {
                    return null;
                }

                var querySourceReferenceExpression
                    = subQueryModel.SelectClause.Selector as QuerySourceReferenceExpression;

                if (querySourceReferenceExpression == null
                    || _inProjection
                    || !_queryModelVisitor.QueryCompilationContext
                        .QuerySourceRequiresMaterialization(querySourceReferenceExpression.ReferencedQuerySource))
                {
                    var queryModelVisitor
                        = (RelationalQueryModelVisitor)_queryModelVisitor.QueryCompilationContext
                            .CreateQueryModelVisitor(_queryModelVisitor);

                    queryModelVisitor.VisitSubQueryModel(subQueryModel);

                    if (queryModelVisitor.Queries.Count == 1
                        && !queryModelVisitor.RequiresClientFilter
                        && !queryModelVisitor.RequiresClientProjection
                        && !queryModelVisitor.RequiresClientResultOperator)
                    {
                        var selectExpression = queryModelVisitor.Queries.First();

                        selectExpression.Alias = string.Empty; // anonymous

                        return selectExpression;
                    }
                }
            }

            return null;
        }
        
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.Value == null)
            {
                return expression;
            }

            var underlyingType = expression.Type.UnwrapNullableType().UnwrapEnumType();

            return _relationalTypeMapper.FindMapping(underlyingType) != null
                ? expression
                : null;
        }

        protected override Expression VisitParameter(ParameterExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var underlyingType = expression.Type.UnwrapNullableType().UnwrapEnumType();

            return _relationalTypeMapper.FindMapping(underlyingType) != null
                ? expression
                : null;
        }

        protected override Expression VisitExtension(Expression expression)
        {
            var stringCompare = expression as StringCompareExpression;
            if (stringCompare != null)
            {
                var newLeft = Visit(stringCompare.Left);
                var newRight = Visit(stringCompare.Right);

                if (newLeft == null
                    || newRight == null)
                {
                    return null;
                }

                return newLeft != stringCompare.Left || newRight != stringCompare.Right
                    ? new StringCompareExpression(stringCompare.Operator, newLeft, newRight)
                    : expression;
            }

            var explicitCast = expression as ExplicitCastExpression;
            if (explicitCast != null)
            {
                var newOperand = Visit(explicitCast.Operand);

                return newOperand != explicitCast.Operand
                    ? new ExplicitCastExpression(newOperand, explicitCast.Type)
                    : expression;
            }

            return base.VisitExtension(expression);
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var selector
                = ((expression.ReferencedQuerySource as FromClauseBase)
                    ?.FromExpression as SubQueryExpression)
                    ?.QueryModel.SelectClause.Selector;

            return selector != null ? Visit(selector) : null;
        }

        protected override TResult VisitUnhandledItem<TItem, TResult>(
            TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
            => default(TResult);

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            => null; // Never called
    }
}
