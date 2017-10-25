// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

// ReSharper disable AssignNullToNotNullAttribute
namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     The default relational LINQ translating expression visitor.
    /// </summary>
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

        private readonly IExpressionFragmentTranslator _compositeExpressionFragmentTranslator;
        private readonly ICompositeMethodCallTranslator _methodCallTranslator;
        private readonly IMemberTranslator _memberTranslator;
        private readonly RelationalQueryModelVisitor _queryModelVisitor;
        private readonly IRelationalTypeMapper _relationalTypeMapper;
        private readonly SelectExpression _targetSelectExpression;
        private readonly Expression _topLevelPredicate;

        private readonly bool _inProjection;
        private readonly NullCheckRemovalTestingVisitor _nullCheckRemovalTestingVisitor;

        private bool _isTopLevelProjection;

        /// <summary>
        ///     Creates a new instance of <see cref="SqlTranslatingExpressionVisitor" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="targetSelectExpression"> The target select expression. </param>
        /// <param name="topLevelPredicate"> The top level predicate. </param>
        /// <param name="inProjection"> true if the expression to be translated is a LINQ projection. </param>
        public SqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _compositeExpressionFragmentTranslator = dependencies.CompositeExpressionFragmentTranslator;
            _methodCallTranslator = dependencies.MethodCallTranslator;
            _memberTranslator = dependencies.MemberTranslator;
            _relationalTypeMapper = dependencies.RelationalTypeMapper;
            _queryModelVisitor = queryModelVisitor;
            _targetSelectExpression = targetSelectExpression;
            _topLevelPredicate = topLevelPredicate;
            _inProjection = inProjection;
            _nullCheckRemovalTestingVisitor = new NullCheckRemovalTestingVisitor(_queryModelVisitor);
            _isTopLevelProjection = inProjection;
        }

        /// <summary>
        ///     When translating a predicate expression, returns a client expression corresponding
        ///     to the part of the target expression that should be evaluated locally.
        /// </summary>
        /// <value>
        ///     The client eval predicate.
        /// </value>
        public virtual Expression ClientEvalPredicate { get; private set; }

        /// <summary>
        ///     Visits the given expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression Visit(Expression expression)
        {
            var translatedExpression = _compositeExpressionFragmentTranslator.Translate(expression);

            if (translatedExpression != null
                && translatedExpression != expression)
            {
                return Visit(translatedExpression);
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (expression != null
                && (expression.NodeType == ExpressionType.Convert
                    || expression.NodeType == ExpressionType.Negate
                    || expression.NodeType == ExpressionType.New))
            {
                return base.Visit(expression);
            }

            var isTopLevelProjection = _isTopLevelProjection;
            _isTopLevelProjection = false;

            try
            {
                return base.Visit(expression);
            }
            finally
            {
                _isTopLevelProjection = isTopLevelProjection;
            }
        }

        /// <summary>
        ///     Visit a binary expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
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

                    return left != null
                           && right != null
                           && left.Type != typeof(Expression[])
                           && right.Type != typeof(Expression[])
                        ? expression.Update(left, expression.Conversion, right)
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

                    return leftExpression != null
                           && rightExpression != null
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

        /// <summary>
        ///     Visits a conditional expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.IsNullPropagationCandidate(out var testExpression, out var resultExpression)
                && _nullCheckRemovalTestingVisitor.CanRemoveNullCheck(testExpression, resultExpression))
            {
                return Visit(resultExpression);
            }

            var test = Visit(expression.Test);
            if (test?.IsSimpleExpression() == true)
            {
                test = Expression.Equal(test, Expression.Constant(true, typeof(bool)));
            }

            var ifTrue = Visit(expression.IfTrue);
            var ifFalse = Visit(expression.IfFalse);

            if (test != null
                && ifTrue != null
                && ifFalse != null)
            {
                // 'test ? new { ... } : null' case can't be translated
                if (ifTrue.Type == typeof(Expression[])
                    || ifFalse.Type == typeof(Expression[]))
                {
                    return null;
                }

                if (ifTrue.IsComparisonOperation()
                    || ifFalse.IsComparisonOperation())
                {
                    var invertedTest = Invert(test);
                    if (invertedTest != null)
                    {
                        return Expression.OrElse(
                            Expression.AndAlso(test, ifTrue),
                            Expression.AndAlso(invertedTest, ifFalse));
                    }
                }

                return expression.Update(test, ifTrue, ifFalse);
            }

            return null;
        }

        private static Expression Invert(Expression test)
        {
            if (test.IsComparisonOperation()
                || test is IsNullExpression)
            {
                if (test is BinaryExpression binaryOperation)
                {
                    var nodeType = binaryOperation.NodeType;

                    return
                        !_inverseOperatorMap.ContainsKey(nodeType)
                            ? null
                            : Expression.MakeBinary(
                                _inverseOperatorMap[nodeType],
                                binaryOperation.Left,
                                binaryOperation.Right);
                }

                return Expression.Not(test);
            }

            return null;
        }

        private class NullCheckRemovalTestingVisitor : ExpressionVisitorBase
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private IQuerySource _querySource;
            private string _propertyName;
            private bool? _canRemoveNullCheck;

            public NullCheckRemovalTestingVisitor(RelationalQueryModelVisitor queryModelVisitor)
                => _queryModelVisitor = queryModelVisitor;

            public bool CanRemoveNullCheck(
                Expression testExpression,
                Expression resultExpression)
            {
                AnalyzeTestExpression(testExpression);
                if (_querySource == null)
                {
                    return false;
                }

                Visit(resultExpression);

                return _canRemoveNullCheck ?? false;
            }

            public override Expression Visit(Expression node)
                => _canRemoveNullCheck == false ? node : base.Visit(node);

            private void AnalyzeTestExpression(Expression expression)
            {
                var processedExpression = expression.RemoveConvert();
                if (processedExpression is NullConditionalExpression nullConditionalExpression)
                {
                    processedExpression = nullConditionalExpression.AccessOperation.RemoveConvert();
                }

                if (processedExpression is QuerySourceReferenceExpression querySourceReferenceExpression)
                {
                    _querySource = querySourceReferenceExpression.ReferencedQuerySource;
                    _propertyName = null;

                    return;
                }

                if (processedExpression is MemberExpression memberExpression
                    && memberExpression.Expression.RemoveConvert() is QuerySourceReferenceExpression querySourceInstance)
                {
                    _querySource = querySourceInstance.ReferencedQuerySource;
                    // ReSharper disable once PossibleNullReferenceException
                    _propertyName = memberExpression.Member.Name;

                    return;
                }

                if (processedExpression is MethodCallExpression methodCallExpression)
                {
                    _queryModelVisitor.BindMethodCallExpression(
                        methodCallExpression,
                        (p, qs) =>
                            {
                                _querySource = qs;
                                _propertyName = p.Name;

                                if ((_queryModelVisitor.QueryCompilationContext.FindEntityType(_querySource)
                                     ?? _queryModelVisitor.QueryCompilationContext.Model.FindEntityType(_querySource.ItemType))
                                    ?.FindProperty(_propertyName)?.IsPrimaryKey()
                                    ?? false)
                                {
                                    _propertyName = null;
                                }
                            });
                }
            }

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                _canRemoveNullCheck
                    = expression.ReferencedQuerySource == _querySource
                      && _propertyName == null;

                return expression;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.Name == _propertyName)
                {
                    if (node.Expression.RemoveConvert() is QuerySourceReferenceExpression querySource)
                    {
                        _canRemoveNullCheck = querySource.ReferencedQuerySource == _querySource;

                        return node;
                    }
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Convert)
                {
                    Visit(node.Operand);

                    return node;
                }

                _canRemoveNullCheck = false;

                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.IsEFPropertyMethod())
                {
                    if (node.Arguments[0].RemoveConvert() is QuerySourceReferenceExpression querySource
                        && node.Arguments[1] is ConstantExpression propertyNameExpression
                        && (string)propertyNameExpression.Value == _propertyName)
                    {
                        _canRemoveNullCheck = querySource.ReferencedQuerySource == _querySource;
                    }

                    return node;
                }

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                // not safe to make the optimization due to null semantics
                // e.g. a != null ? a.Name == null : null cant be translated to a.Name == null
                // because even if value of 'a' is null, the result of the optimization would be 'true', not the expected 'null'
                if (node.NodeType == ExpressionType.Equal
                    || node.NodeType == ExpressionType.NotEqual)
                {
                    _canRemoveNullCheck = false;

                    return node;
                }

                return base.VisitBinary(node);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is NullConditionalExpression nullConditionalExpression)
                {
                    Visit(nullConditionalExpression.AccessOperation);

                    return extensionExpression;
                }

                _canRemoveNullCheck = false;

                return extensionExpression;
            }
        }

        private static Expression UnfoldStructuralComparison(ExpressionType expressionType, Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            var leftConstantExpression = binaryExpression?.Left as ConstantExpression;
#pragma warning disable IDE0019 // Use pattern matching
            var leftExpressions = leftConstantExpression?.Value as Expression[];
#pragma warning restore IDE0019 // Use pattern matching
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
                if (leftExpressions.Length == 1
                    && expressionType == ExpressionType.Equal)
                {
                    var translatedExpression = TransformNullComparison(leftExpressions[0], rightExpressions[0], expressionType)
                                               ?? Expression.Equal(leftExpressions[0], rightExpressions[0]);
                    return Expression.AndAlso(translatedExpression, Expression.Constant(true, typeof(bool)));
                }

                return leftExpressions
                    .Zip(
                        rightExpressions, (l, r) =>
                            TransformNullComparison(l, r, expressionType)
                            ?? Expression.MakeBinary(expressionType, l, r))
                    .Aggregate(
                        (e1, e2) =>
                            expressionType == ExpressionType.Equal
                                ? Expression.AndAlso(e1, e2)
                                : Expression.OrElse(e1, e2));
            }

            return expression;
        }

        private Expression ProcessComparisonExpression(BinaryExpression binaryExpression)
        {
            var leftExpression = Visit(binaryExpression.Left);

            if (leftExpression == null)
            {
                return null;
            }

            var rightExpression = Visit(binaryExpression.Right);

            if (rightExpression == null)
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
                var isLeftNullConstant = left.IsNullConstantExpression();
                var isRightNullConstant = right.IsNullConstantExpression();

                if (isLeftNullConstant || isRightNullConstant)
                {
                    var nonNullExpression = (isLeftNullConstant ? right : left).RemoveConvert();

                    if (nonNullExpression is NullableExpression nullableExpression)
                    {
                        nonNullExpression = nullableExpression.Operand.RemoveConvert();
                    }

                    return expressionType == ExpressionType.Equal
                        ? (Expression)new IsNullExpression(nonNullExpression)
                        : Expression.Not(new IsNullExpression(nonNullExpression));
                }
            }

            return null;
        }

        /// <summary>
        ///     Visits a method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var operand = _queryModelVisitor.QueryCompilationContext.Model.Relational().FindDbFunction(methodCallExpression.Method) != null
                ? methodCallExpression.Object
                : Visit(methodCallExpression.Object);

            if (operand != null
                || methodCallExpression.Object == null)
            {
                var arguments
                    = methodCallExpression.Arguments
                        .Where(
                            e => !(e.RemoveConvert() is QuerySourceReferenceExpression)
                                 && !IsNonTranslatableSubquery(e.RemoveConvert()))
                        .Select(
                            e => (e.RemoveConvert() as ConstantExpression)?.Value is Array || e.RemoveConvert().Type == typeof(DbFunctions)
                                ? e
                                : Visit(e))
                        .Where(e => e != null)
                        .ToArray();

                if (arguments.Length == methodCallExpression.Arguments.Count)
                {
                    var boundExpression
                        = operand != null
                            ? Expression.Call(operand, methodCallExpression.Method, arguments)
                            : Expression.Call(methodCallExpression.Method, arguments);

                    var translatedExpression = _methodCallTranslator.Translate(boundExpression, _queryModelVisitor.QueryCompilationContext.Model);

                    if (translatedExpression != null)
                    {
                        return translatedExpression;
                    }
                }
            }

            if (AnonymousObject.IsGetValueExpression(methodCallExpression, out var querySourceReferenceExpression)
                || MaterializedAnonymousObject.IsGetValueExpression(methodCallExpression, out querySourceReferenceExpression))
            {
                var selectExpression
                    = _queryModelVisitor.TryGetQuery(querySourceReferenceExpression.ReferencedQuerySource);

                if (selectExpression != null)
                {
                    var projectionIndex
                        = (int)((ConstantExpression)methodCallExpression.Arguments.Single()).Value;

                    return selectExpression.BindSubqueryProjectionIndex(
                        projectionIndex,
                        querySourceReferenceExpression.ReferencedQuerySource);
                }
            }

            return TryBindMemberOrMethodToSelectExpression(
                       methodCallExpression, (expression, visitor, binder)
                           => visitor.BindMethodCallExpression(expression, binder))
                   ?? _queryModelVisitor.BindLocalMethodCallExpression(methodCallExpression)
                   ?? _queryModelVisitor.BindMethodToOuterQueryParameter(methodCallExpression);
        }

        private bool IsNonTranslatableSubquery(Expression expression)
            => expression is SubQueryExpression subQueryExpression
               && !(subQueryExpression.QueryModel.GetOutputDataInfo() is StreamedScalarValueInfo
                    || subQueryExpression.QueryModel.GetOutputDataInfo() is StreamedSingleValueInfo streamedSingleValueInfo
                    && IsStreamedSingleValueSupportedType(streamedSingleValueInfo));

        /// <summary>
        ///     Visit a member expression.
        /// </summary>
        /// <param name="memberExpression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            if (!(memberExpression.Expression.RemoveConvert() is QuerySourceReferenceExpression)
                && !(memberExpression.Expression.RemoveConvert() is SubQueryExpression))
            {
                var newExpression = Visit(memberExpression.Expression);

                if (newExpression != null
                    || memberExpression.Expression == null)
                {
                    var newMemberExpression
                        = newExpression != memberExpression.Expression
                            ? Expression.Property(newExpression, memberExpression.Member.Name)
                            : memberExpression;

                    var translatedExpression = _memberTranslator.Translate(newMemberExpression);

                    if (translatedExpression != null)
                    {
                        return translatedExpression;
                    }
                }
            }

            return TryBindMemberOrMethodToSelectExpression(
                       memberExpression, (expression, visitor, binder)
                           => visitor.BindMemberExpression(expression, binder))
                   ?? TryBindQuerySourcePropertyExpression(memberExpression)
                   ?? _queryModelVisitor.BindMemberToOuterQueryParameter(memberExpression);
        }

        private Expression TryBindQuerySourcePropertyExpression(MemberExpression memberExpression)
        {
            if (memberExpression.Expression is QuerySourceReferenceExpression qsre)
            {
                var selectExpression = _queryModelVisitor.TryGetQuery(qsre.ReferencedQuerySource);

                return selectExpression?.GetProjectionForMemberInfo(memberExpression.Member);
            }

            return null;
        }

        private Expression TryBindMemberOrMethodToSelectExpression<TExpression>(
            TExpression sourceExpression,
            Func<TExpression, RelationalQueryModelVisitor, Func<IProperty, IQuerySource, SelectExpression, Expression>, Expression> binder)
        {
            Expression BindPropertyToSelectExpression(
                IProperty property, IQuerySource querySource, SelectExpression selectExpression)
                => selectExpression.BindProperty(
                    property,
                    querySource);

            var boundExpression = binder(
                sourceExpression, _queryModelVisitor, (property, querySource, selectExpression) =>
                    {
                        var boundPropertyExpression = BindPropertyToSelectExpression(property, querySource, selectExpression);

                        if (_targetSelectExpression != null
                            && selectExpression != _targetSelectExpression)
                        {
                            selectExpression.AddToProjection(boundPropertyExpression);
                            return null;
                        }

                        return boundPropertyExpression;
                    });

            if (boundExpression != null)
            {
                return boundExpression;
            }

            var outerQueryModelVisitor = _queryModelVisitor.ParentQueryModelVisitor;
            var canBindToOuterQueryModelVisitor = _queryModelVisitor.CanBindToParentQueryModel;

            while (outerQueryModelVisitor != null && canBindToOuterQueryModelVisitor)
            {
                boundExpression = binder(sourceExpression, outerQueryModelVisitor, BindPropertyToSelectExpression);

                if (boundExpression != null)
                {
                    return boundExpression;
                }

                canBindToOuterQueryModelVisitor = outerQueryModelVisitor.CanBindToParentQueryModel;
                outerQueryModelVisitor = outerQueryModelVisitor.ParentQueryModelVisitor;
            }

            return null;
        }

        /// <summary>
        ///     Visit a unary expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                {
                    var operand = Visit(expression.Operand);
                    if (operand != null)
                    {
                        return Expression.Negate(operand);
                    }

                    break;
                }
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
                    var isTopLevelProjection = _isTopLevelProjection;
                    _isTopLevelProjection = false;
                    var operand = Visit(expression.Operand);
                    _isTopLevelProjection = isTopLevelProjection;

                    if (operand != null)
                    {
                        return _isTopLevelProjection
                               && operand.Type.IsValueType
                               && expression.Type.IsValueType
                               && expression.Type.UnwrapNullableType() != operand.Type.UnwrapNullableType()
                               && expression.Type.UnwrapEnumType() != operand.Type.UnwrapEnumType()
                            ? (Expression)new ExplicitCastExpression(operand, expression.Type)
                            : Expression.Convert(operand, expression.Type);
                    }

                    break;
                }
            }

            return null;
        }

        /// <summary>
        ///     Visits a new expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
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
            else if (expression.Type == typeof(AnonymousObject)
                || expression.Type == typeof(MaterializedAnonymousObject))
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

        /// <summary>
        ///     Visits a sub-query expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
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
                    var containsItem = Visit(contains.Item);
                    if (containsItem != null
                        && containsItem.Type == contains.Item.Type)
                    {
                        return new InExpression(containsItem, new[] { fromExpression });
                    }
                }
            }
            else if (!(subQueryOutputDataInfo is StreamedSequenceInfo))
            {
                if (_inProjection
                    && !(subQueryOutputDataInfo is StreamedScalarValueInfo)
                    && !IsStreamedSingleValueSupportedType(subQueryOutputDataInfo))
                {
                    return null;
                }

                var referencedQuerySource = subQueryModel.SelectClause.Selector.TryGetReferencedQuerySource();

                if (referencedQuerySource == null
                    || _inProjection
                    || !_queryModelVisitor.QueryCompilationContext
                        .QuerySourceRequiresMaterialization(referencedQuerySource))
                {
                    var subQueryModelVisitor
                        = (RelationalQueryModelVisitor)_queryModelVisitor.QueryCompilationContext
                            .CreateQueryModelVisitor(_queryModelVisitor);

                    var queriesProjectionCountMapping
                        = _queryModelVisitor.Queries
                            .ToDictionary(k => k, s => s.Projection.Count);

                    var queryModelMapping = new Dictionary<QueryModel, QueryModel>();
                    subQueryModel.PopulateQueryModelMapping(queryModelMapping);

                    var groupByTranslation = expression.QueryModel.MainFromClause.FromExpression.Type.IsGrouping();
                    if (groupByTranslation)
                    {
                        var outerSelectExpression = _targetSelectExpression.Clone();
                        subQueryModelVisitor.AddQuery(subQueryModel.MainFromClause, outerSelectExpression);
                    }

                    subQueryModelVisitor.VisitSubQueryModel(subQueryModel);

                    if (subQueryModelVisitor.IsLiftable)
                    {
                        var selectExpression = subQueryModelVisitor.Queries.First();

                        selectExpression.Alias = string.Empty; // anonymous

                        foreach (var mappingElement in queriesProjectionCountMapping)
                        {
                            mappingElement.Key.RemoveRangeFromProjection(mappingElement.Value);
                        }

                        _queryModelVisitor.LiftInjectedParameters(subQueryModelVisitor);

                        if (groupByTranslation
                            && selectExpression.Projection.Count == 1)
                        {
                            return selectExpression.Projection.Single();
                        }

                        return selectExpression;
                    }

                    subQueryModel.RecreateQueryModelFromMapping(queryModelMapping);
                }
            }

            return null;
        }

        private bool IsStreamedSingleValueSupportedType(IStreamedDataInfo outputDataInfo)
            => outputDataInfo is StreamedSingleValueInfo streamedSingleValueInfo
               && _relationalTypeMapper.FindMapping(
                   streamedSingleValueInfo.DataType
                       .UnwrapNullableType()
                       .UnwrapEnumType()) != null;

        /// <summary>
        ///     Visits a constant expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.Value == null)
            {
                return expression;
            }

            var underlyingType = expression.Type.UnwrapNullableType().UnwrapEnumType();

            if (underlyingType == typeof(Enum))
            {
                underlyingType = expression.Value.GetType();
            }

            return _relationalTypeMapper.FindMapping(underlyingType) != null
                ? expression
                : null;
        }

        /// <summary>
        ///     Visits a parameter expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var underlyingType = expression.Type.UnwrapNullableType().UnwrapEnumType();

            return _relationalTypeMapper.FindMapping(underlyingType) != null
                ? expression
                : null;
        }

        /// <summary>
        ///     Visits an extension expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case StringCompareExpression stringCompare:
                {
                    var newLeft = Visit(stringCompare.Left);
                    var newRight = Visit(stringCompare.Right);
                    if (newLeft == null
                        || newRight == null)
                    {
                        return null;
                    }

                    return newLeft != stringCompare.Left
                           || newRight != stringCompare.Right
                        ? new StringCompareExpression(stringCompare.Operator, newLeft, newRight)
                        : expression;
                }
                case ExplicitCastExpression explicitCast:
                {
                    var newOperand = Visit(explicitCast.Operand);
                    if (newOperand == null)
                    {
                        return null;
                    }

                    return newOperand != explicitCast.Operand
                        ? new ExplicitCastExpression(newOperand, explicitCast.Type)
                        : expression;
                }
                case NullConditionalExpression nullConditionalExpression:
                {
                    var newAccessOperation = Visit(nullConditionalExpression.AccessOperation);
                    if (newAccessOperation == null)
                    {
                        return null;
                    }

                    if (newAccessOperation.Type != nullConditionalExpression.Type)
                    {
                        newAccessOperation = Expression.Convert(newAccessOperation, nullConditionalExpression.Type);
                    }

                    return new NullableExpression(newAccessOperation);
                }
                case NullConditionalEqualExpression nullConditionalEqualExpression:
                {
                    var equalityExpression
                        = new NullCompensatedExpression(
                            Expression.Equal(
                                nullConditionalEqualExpression.OuterKey,
                                nullConditionalEqualExpression.InnerKey));

                    return Visit(equalityExpression);
                }
                case NullCompensatedExpression nullCompensatedExpression:
                {
                    var newOperand = Visit(nullCompensatedExpression.Operand);
                    if (newOperand == null)
                    {
                        return null;
                    }

                    return newOperand != nullCompensatedExpression.Operand
                        ? new NullCompensatedExpression(newOperand)
                        : nullCompensatedExpression;
                }
                default:
                    return base.VisitExtension(expression);
            }
        }

        /// <summary>
        ///     Visits a query source reference expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!_inProjection)
            {
                if (expression.ReferencedQuerySource is JoinClause joinClause)
                {
                    var entityType
                        = _queryModelVisitor.QueryCompilationContext.FindEntityType(joinClause)
                          ?? _queryModelVisitor.QueryCompilationContext.Model
                              .FindEntityType(joinClause.ItemType);

                    if (entityType != null)
                    {
                        return Visit(
                            expression.CreateEFPropertyExpression(
                                entityType.FindPrimaryKey().Properties[0]));
                    }

                    return null;
                }
            }

            var type = expression.ReferencedQuerySource.ItemType.UnwrapNullableType().UnwrapEnumType();

            if (_relationalTypeMapper.FindMapping(type) != null)
            {
                var selectExpression = _queryModelVisitor.TryGetQuery(expression.ReferencedQuerySource);

                if (selectExpression != null)
                {
                    var subquery = selectExpression.Tables.FirstOrDefault() as SelectExpression;

                    var innerProjectionExpression = subquery?.Projection.FirstOrDefault();
                    if (innerProjectionExpression != null)
                    {
                        return innerProjectionExpression.LiftExpressionFromSubquery(subquery);
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Called when an unhandled item is visited. This method provides the item the visitor cannot handle (<paramref name="unhandledItem" />),
        ///     the <paramref name="visitMethod" /> that is not implemented in the visitor, and a delegate that can be used to invoke the
        ///     <paramref name="baseBehavior" /> of the <see cref="T:Remotion.Linq.Parsing.RelinqExpressionVisitor" /> class. The default behavior of
        ///     this method is to call the
        ///     <see cref="M:Remotion.Linq.Parsing.ThrowingExpressionVisitor.CreateUnhandledItemException``1(``0,System.String)" /> method, but it can
        ///     be overridden to do something else.
        /// </summary>
        /// <typeparam name="TItem">
        ///     The type of the item that could not be handled. Either an <see cref="T:System.Linq.Expressions.Expression" /> type, a
        ///     <see cref="T:System.Linq.Expressions.MemberBinding" />
        ///     type, or <see cref="T:System.Linq.Expressions.ElementInit" />.
        /// </typeparam>
        /// <typeparam name="TResult">The result type expected for the visited <paramref name="unhandledItem" />.</typeparam>
        /// <param name="unhandledItem">The unhandled item.</param>
        /// <param name="visitMethod">The visit method that is not implemented.</param>
        /// <param name="baseBehavior">The behavior exposed by <see cref="T:Remotion.Linq.Parsing.RelinqExpressionVisitor" /> for this item type.</param>
        /// <returns>An object to replace <paramref name="unhandledItem" /> in the expression tree. Alternatively, the method can throw any exception.</returns>
        protected override TResult VisitUnhandledItem<TItem, TResult>(
            TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
            => default;

        /// <summary>
        ///     Creates an unhandled item exception.
        /// </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="unhandledItem"> The unhandled item. </param>
        /// <param name="visitMethod"> The visit method that is not implemented. </param>
        /// <returns>
        ///     The new unhandled item exception.
        /// </returns>
        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            => null; // Never called
    }
}
