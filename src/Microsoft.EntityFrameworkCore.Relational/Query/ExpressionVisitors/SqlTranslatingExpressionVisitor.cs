// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
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

        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IExpressionFragmentTranslator _compositeExpressionFragmentTranslator;
        private readonly IMethodCallTranslator _methodCallTranslator;
        private readonly IMemberTranslator _memberTranslator;
        private readonly RelationalQueryModelVisitor _queryModelVisitor;
        private readonly IRelationalTypeMapper _relationalTypeMapper;
        private readonly Expression _topLevelPredicate;

        private readonly bool _inProjection;
        private readonly bool _mutateProjections;

        /// <summary>
        ///     Creates a new instance of <see cref="SqlTranslatingExpressionVisitor" />.
        /// </summary>
        /// <param name="relationalAnnotationProvider"> The relational annotation provider. </param>
        /// <param name="compositeExpressionFragmentTranslator"> The composite expression fragment translator. </param>
        /// <param name="methodCallTranslator"> The method call translator. </param>
        /// <param name="memberTranslator"> The member translator. </param>
        /// <param name="relationalTypeMapper"> The relational type mapper. </param>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="topLevelPredicate"> The top level predicate. </param>
        /// <param name="inProjection"> true if the expression to be translated is a LINQ projection. </param>
        /// <param name="mutateProjections"> false to avoid adding columns to projections. </param>
        public SqlTranslatingExpressionVisitor(
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IExpressionFragmentTranslator compositeExpressionFragmentTranslator,
            [NotNull] IMethodCallTranslator methodCallTranslator,
            [NotNull] IMemberTranslator memberTranslator,
            [NotNull] IRelationalTypeMapper relationalTypeMapper,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false,
            bool mutateProjections = true)
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
            _topLevelPredicate = topLevelPredicate;
            _inProjection = inProjection;
            _mutateProjections = mutateProjections;
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

            return base.Visit(expression);
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

                        if (_inProjection
                            && right?.NodeType == ExpressionType.Constant
                            && right.Type == typeof(bool))
                        {
                            right = new ExplicitCastExpression(right, typeof(bool));
                        }

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
                        var leftExpression = Visit(expression.Left).MaybeAnonymousSubquery();
                        var rightExpression = Visit(expression.Right).MaybeAnonymousSubquery();

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

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
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

        private Expression TryRemoveNullCheck(ConditionalExpression node)
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
                if (ifTrueConstant == null
                    || ifTrueConstant.Value != null)
                {
                    return null;
                }
            }
            else
            {
                var ifFalseConstant = node.IfFalse as ConstantExpression;
                if (ifFalseConstant == null
                    || ifFalseConstant.Value != null)
                {
                    return null;
                }
            }

            var testExpression = isLeftNullConstant ? binaryTest.Right : binaryTest.Left;
            var resultExpression = binaryTest.NodeType == ExpressionType.Equal ? node.IfFalse : node.IfTrue;

            var nullCheckRemovalTestingVisitor = new NullCheckRemovalTestingVisitor(_queryModelVisitor.QueryCompilationContext.Model);

            return nullCheckRemovalTestingVisitor.CanRemoveNullCheck(testExpression, resultExpression)
                ? resultExpression
                : null;
        }

        private class NullCheckRemovalTestingVisitor : ExpressionVisitorBase
        {
            private IQuerySource _querySource;
            private readonly IModel _model;
            private string _propertyName;
            private bool? _canRemoveNullCheck;

            public NullCheckRemovalTestingVisitor(IModel model)
            {
                _model = model;
            }

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

            public override Expression Visit(Expression node)
                => _canRemoveNullCheck == false ? node : base.Visit(node);

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
                var querySourceInstance = memberExpression?.Expression as QuerySourceReferenceExpression;

                if (querySourceInstance != null)
                {
                    _querySource = querySourceInstance.ReferencedQuerySource;
                    _propertyName = memberExpression.Member.Name;

                    return;
                }

                var methodCallExpression = expression as MethodCallExpression;

                if (methodCallExpression != null
                    && EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
                {
                    var querySourceCaller = methodCallExpression.Arguments[0] as QuerySourceReferenceExpression;
                    if (querySourceCaller != null)
                    {
                        var propertyNameExpression = methodCallExpression.Arguments[1] as ConstantExpression;
                        if (propertyNameExpression != null)
                        {
                            _querySource = querySourceCaller.ReferencedQuerySource;
                            _propertyName = (string)propertyNameExpression.Value;
                            if (_model.FindEntityType(_querySource.ItemType)?.FindProperty(_propertyName)?.IsPrimaryKey() ?? false)
                            {
                                _propertyName = null;
                            }
                        }
                    }
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
                    if (propertyNameExpression != null
                        && (string)propertyNameExpression.Value == _propertyName)
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
            var leftExpressions = (binaryExpression?.Left as CompositeExpression)?.Expressions;
            var rightConstantExpression = binaryExpression?.Right as ConstantExpression;
            var rightExpressions = (binaryExpression?.Right as CompositeExpression)?.Expressions;

            if (leftExpressions != null
                && rightConstantExpression != null
                && rightConstantExpression.Value == null)
            {
                rightExpressions
                    = Enumerable
                        .Repeat<Expression>(rightConstantExpression, leftExpressions.Count)
                        .ToArray();
            }

            if (rightExpressions != null
                && leftConstantExpression != null
                && leftConstantExpression.Value == null)
            {
                leftExpressions
                    = Enumerable
                        .Repeat<Expression>(leftConstantExpression, rightExpressions.Count)
                        .ToArray();
            }

            if (leftExpressions != null
                && rightExpressions != null
                && leftExpressions.Count == rightExpressions.Count)
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
            var leftExpression = Visit(binaryExpression.Left).MaybeAnonymousSubquery();

            if (leftExpression == null)
            {
                return null;
            }

            var rightExpression = Visit(binaryExpression.Right).MaybeAnonymousSubquery();

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

        /// <summary>
        ///     Visits a method call expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var operand = Visit(expression.Object);

            var arguments = new List<Expression>();

            foreach (var argument in expression.Arguments)
            {
                if (argument is QuerySourceReferenceExpression || argument is SubQueryExpression)
                {
                    break;
                }
                else if (argument is ConstantExpression)
                {
                    arguments.Add(argument);
                }
                else
                {
                    var result = Visit(argument);

                    if (result != null)
                    {
                        arguments.Add(result);
                    }
                }
            }

            if ((operand != null || expression.Object == null) && arguments.Count == expression.Arguments.Count)
            {
                var newExpression
                    = operand != null
                        ? Expression.Call(operand, expression.Method, arguments)
                        : Expression.Call(expression.Method, arguments);

                var translatedExpression = _methodCallTranslator.Translate(newExpression);

                if (translatedExpression != null)
                {
                    return translatedExpression;
                }
            }

            return TryBindAliasExpression(expression, (visitor, binder)
                    => visitor.BindMethodCallExpression(expression, binder))
                ?? _queryModelVisitor.BindLocalMethodCallExpression(expression)
                ?? _queryModelVisitor.BindMethodCallToOuterQueryParameter(expression);
        }

        /// <summary>
        ///     Visit a member expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitMember(MemberExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var subQueryExpression = expression.Expression as SubQueryExpression;

            if (subQueryExpression != null)
            {
                var entityType
                    = _queryModelVisitor.QueryCompilationContext.Model
                        .FindEntityType(subQueryExpression.Type);

                var property = entityType?.FindProperty(expression.Member.Name);

                if (property != null)
                {
                    var name = _relationalAnnotationProvider.For(property).ColumnName;

                    var selectExpression = VisitSubQuery(subQueryExpression) as SelectExpression;

                    var isInlinable = selectExpression != null;

                    // This must be retrieved AFTER visiting the subquery model.
                    var selectorQuerySource
                        = (subQueryExpression.QueryModel.SelectClause.Selector
                            as QuerySourceReferenceExpression)?.ReferencedQuerySource;

                    if (selectExpression == null)
                    {
                        selectExpression
                            = _queryModelVisitor.QueryCompilationContext
                                .FindSelectExpression(selectorQuerySource);
                    }

                    if (selectExpression != null)
                    {
                        selectExpression.ClearProjection();
                        selectExpression.AddToProjection(name, property, selectorQuerySource);

                        if (isInlinable)
                        {
                            return selectExpression;
                        }
                    }
                }

                return null;
            }
            else if (!(expression.Expression is QuerySourceReferenceExpression))
            {
                var translatedExpression = Visit(expression.Expression);

                if (translatedExpression is CompositeExpression)
                {
                    return translatedExpression;
                }

                if (translatedExpression != null || expression.Expression == null)
                {
                    var newMemberExpression
                        = translatedExpression != expression.Expression
                            ? Expression.Property(translatedExpression, expression.Member.Name)
                            : expression;

                    translatedExpression = _memberTranslator.Translate(newMemberExpression);

                    if (translatedExpression != null)
                    {
                        return translatedExpression;
                    }
                }
            }

            return TryBindAliasExpression(expression, (visitor, binder)
                    => visitor.BindMemberExpression(expression, binder))
                ?? TryBindQuerySourceReferencePropertyExpression(expression)
                ?? _queryModelVisitor.BindMemberToOuterQueryParameter(expression);
        }

        private Expression TryBindQuerySourceReferencePropertyExpression(MemberExpression expression)
        {
            var referencedQuerySource
                = (expression.Expression as QuerySourceReferenceExpression)
                    ?.ReferencedQuerySource;

            if (referencedQuerySource != null)
            {
                var selectExpression = _queryModelVisitor.TryGetQuery(referencedQuerySource);

                if (selectExpression != null)
                {
                    var typeInfo = expression.Expression.Type.GetTypeInfo();

                    if (typeInfo.IsGrouping() && expression.Member.Name == "Key")
                    {
                        if (selectExpression.GroupBy.Any())
                        {
                            return new CompositeExpression(selectExpression.GroupBy);
                        }
                    }
                    else
                    {
                        return selectExpression.Projection
                            .OfType<AliasExpression>()
                            .SingleOrDefault(ae => ae.SourceMember == expression.Member);
                    }
                }
                else
                {
                    selectExpression
                        = _queryModelVisitor.QueryCompilationContext
                            .FindSelectExpression(referencedQuerySource);

                    if (selectExpression != null)
                    {
                        var aliasExpression =
                            selectExpression.Projection.OfType<AliasExpression>()
                                .SingleOrDefault(ae => ae.SourceMember == expression.Member);

                        if (aliasExpression != null)
                        {
                            return new OuterPropertyExpression(
                                expression,
                                aliasExpression.TryGetColumnExpression().Property,
                                referencedQuerySource,
                                aliasExpression);
                        }
                    }
                }
            }

            return null;
        }

        private Expression TryBindAliasExpression(
            Expression sourceExpression,
            Func<RelationalQueryModelVisitor, Func<IProperty, IQuerySource, SelectExpression, Expression>, Expression> binder)
        {
            var bound = binder(_queryModelVisitor, (property, querySource, selectExpression) =>
            {
                if (_mutateProjections)
                {
                    var columnName = _relationalAnnotationProvider.For(property).ColumnName;

                    selectExpression.AddToProjection(columnName, property, querySource);
                }

                if (!selectExpression.IsComposable())
                {
                    return null;
                }

                if (!_inProjection
                    && !selectExpression.HandlesQuerySource(querySource)
                    && !_queryModelVisitor.QueryCompilationContext.IsLateralJoinSupported)
                {
                    return null;
                }

                return CreateAliasExpression(property, querySource, selectExpression);
            });

            if (bound == null)
            {
                var ancestor = _queryModelVisitor.ParentQueryModelVisitor;
                var requiresOuterParameterInjection = _queryModelVisitor.RequiresOuterParameterInjection;

                while (ancestor != null)
                {
                    bound = binder(ancestor, (property, querySource, selectExpression) =>
                    {
                        if (requiresOuterParameterInjection)
                        {
                            if (_mutateProjections)
                            {
                                var columnName = _relationalAnnotationProvider.For(property).ColumnName;

                                selectExpression.AddToProjection(columnName, property, querySource);
                            }

                            return null;
                        }

                        if (_queryModelVisitor.CanBindOuterParameters)
                        {
                            var aliasExpression = CreateAliasExpression(property, querySource, selectExpression);

                            return new OuterPropertyExpression(sourceExpression, property, querySource, aliasExpression);
                        }

                        return null;
                    });

                    if (bound != null)
                    {
                        return bound;
                    }

                    requiresOuterParameterInjection |= ancestor.RequiresOuterParameterInjection;
                    ancestor = ancestor.ParentQueryModelVisitor;
                }
            }

            return bound;
        }

        private AliasExpression CreateAliasExpression(
            IProperty property,
            IQuerySource querySource,
            SelectExpression selectExpression)
        {
            var column = _relationalAnnotationProvider.For(property).ColumnName;
            var table = selectExpression.GetTableForQuerySource(querySource);

            return new AliasExpression(new ColumnExpression(column, property, table));
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
                case ExpressionType.Not:
                    {
                        var operand = Visit(expression.Operand).MaybeAnonymousSubquery();
                        if (operand != null)
                        {
                            return Expression.Not(operand);
                        }

                        break;
                    }
                case ExpressionType.Convert:
                    {
                        var operand = Visit(expression.Operand).MaybeAnonymousSubquery();
                        if (operand != null)
                        {
                            return Expression.Convert(operand, expression.Type);
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
                    return new CompositeExpression(memberBindings);
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
                    return new CompositeExpression(memberBindings);
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
            var compilationContext = _queryModelVisitor.QueryCompilationContext;
            var subQueryModelVisitor = compilationContext.GetQueryModelVisitor(subQueryModel);

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
                    var containsItem = Visit(contains.Item)?.RemoveConvert();
                    if (containsItem != null)
                    {
                        return new InExpression(containsItem, new[] { fromExpression });
                    }
                }
            }
            else if (subQueryOutputDataInfo is StreamedScalarValueInfo || subQueryOutputDataInfo is StreamedSingleValueInfo)
            {
                var lastResultOperator = subQueryModel.ResultOperators.LastOrDefault();

                if (IsTranslatableAggregate(lastResultOperator))
                {
                    var translated = TranslateAggregateSubquery(subQueryModel, lastResultOperator);

                    if (translated != null)
                    {
                        return translated;
                    }
                }

                var isRelationalType
                    = _relationalTypeMapper.FindMapping(
                        subQueryOutputDataInfo.DataType
                            .UnwrapNullableType()
                            .UnwrapEnumType()) != null;

                var isEntityType = compilationContext.Model.FindEntityType(subQueryOutputDataInfo.DataType) != null;

                var referencedQuerySource
                    = (subQueryModel.SelectClause.Selector as QuerySourceReferenceExpression)?
                        .ReferencedQuerySource;

                var requiresMaterialization
                    = referencedQuerySource != null
                        && compilationContext.QuerySourceRequiresMaterialization(referencedQuerySource);

                if (isRelationalType || (isEntityType && !requiresMaterialization))
                {
                    if (subQueryModelVisitor == null)
                    {
                        subQueryModelVisitor = compilationContext.CreateQueryModelVisitor(subQueryModel, _queryModelVisitor);
                        subQueryModelVisitor.RequiresOuterParameterInjection = _inProjection && _queryModelVisitor.RequiresClientProjection;
                        subQueryModelVisitor.VisitQueryModel(subQueryModel);
                    }

                    if (subQueryModelVisitor.IsInlinable)
                    {
                        return subQueryModelVisitor.Queries.SingleOrDefault();
                    }
                }
            }

            if (subQueryModelVisitor == null)
            {
                subQueryModelVisitor = compilationContext.CreateQueryModelVisitor(subQueryModel, _queryModelVisitor);
                subQueryModelVisitor.RequiresOuterParameterInjection = true;
                subQueryModelVisitor.VisitQueryModel(subQueryModel);
            }

            return null;
        }

        private Expression TranslateAggregateSubquery(QueryModel subQueryModel, ResultOperatorBase lastResultOperator)
        {
            var groupingQuerySource
                = (subQueryModel.MainFromClause.FromExpression
                    as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource;

            if (groupingQuerySource != null
                && groupingQuerySource.ItemType.IsGrouping()
                && subQueryModel.BodyClauses.Count == 0)
            {
                var selectExpression = _queryModelVisitor.TryGetQuery(groupingQuerySource);

                if (selectExpression != null)
                {
                    _queryModelVisitor.MapQuery(subQueryModel.MainFromClause, selectExpression);

                    if (lastResultOperator is CountResultOperator)
                    {
                        return new AliasExpression("Count",
                            new SqlFunctionExpression("COUNT", typeof(int),
                                new[] { new SqlFragmentExpression("*") }));
                    }
                    else if (lastResultOperator is LongCountResultOperator)
                    {
                        return new AliasExpression("LongCount",
                            new SqlFunctionExpression("COUNT", typeof(long),
                                new[] { new SqlFragmentExpression("*") }));
                    }

                    var selector = subQueryModel.SelectClause.Selector;

                    if (subQueryModel.IsIdentityQuery())
                    {
                        var groupResultOperator
                            = ((groupingQuerySource as FromClauseBase)
                                ?.FromExpression as SubQueryExpression)
                                    ?.QueryModel.ResultOperators.LastOrDefault() as GroupResultOperator;

                        if (groupResultOperator != null)
                        {
                            selector = groupResultOperator.ElementSelector;
                        }
                    }

                    var translatedSelector = Visit(selector);

                    if (translatedSelector != null)
                    {
                        if (lastResultOperator is AverageResultOperator)
                        {
                            var targetType
                                = selector.Type.UnwrapNullableType() == typeof(decimal)
                                    ? typeof(decimal)
                                    : typeof(double);

                            if (selector.Type.IsNullableType())
                            {
                                targetType = targetType.MakeNullable();
                            }

                            return new AliasExpression("Average",
                                new SqlFunctionExpression("AVG", targetType,
                                    new[] { new ExplicitCastExpression(translatedSelector, targetType) }));
                        }
                        else if (lastResultOperator is MaxResultOperator)
                        {
                            return new AliasExpression("Max",
                                new SqlFunctionExpression("MAX", selector.Type,
                                    new[] { translatedSelector }));
                        }
                        else if (lastResultOperator is MinResultOperator)
                        {
                            return new AliasExpression("Min",
                                new SqlFunctionExpression("MIN", selector.Type,
                                    new[] { translatedSelector }));
                        }
                        else if (lastResultOperator is SumResultOperator)
                        {
                            return new AliasExpression("Sum",
                                new SqlFunctionExpression("SUM", selector.Type,
                                    new[] { translatedSelector }));
                        }
                    }
                }
            }

            return null;
        }

        private static bool IsTranslatableAggregate(ResultOperatorBase resultOperator)
        {
            if (resultOperator == null)
            {
                return false;
            }

            return resultOperator is AverageResultOperator
                || resultOperator is CountResultOperator
                || resultOperator is LongCountResultOperator
                || resultOperator is MaxResultOperator
                || resultOperator is MinResultOperator
                || resultOperator is SumResultOperator;
        }

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

                return newLeft != stringCompare.Left
                       || newRight != stringCompare.Right
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

            var nullConditionalExpression
                = expression as NullConditionalExpression;

            if (nullConditionalExpression != null)
            {
                var newAccessOperation = Visit(nullConditionalExpression.AccessOperation);
                var columnExpression = newAccessOperation.TryGetColumnExpression();

                if (columnExpression != null)
                {
                    columnExpression.IsNullable = true;
                }

                if (newAccessOperation != null
                    && newAccessOperation.Type != nullConditionalExpression.Type)
                {
                    newAccessOperation
                        = Expression.Convert(newAccessOperation, nullConditionalExpression.Type);
                }

                return newAccessOperation;
            }

            return base.VisitExtension(expression);
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

            var referencedQuerySource = expression.ReferencedQuerySource;

            var requiresMaterialization =
                _queryModelVisitor.QueryCompilationContext
                    .QuerySourceRequiresMaterialization(referencedQuerySource);

            if (!_inProjection || (!requiresMaterialization && _queryModelVisitor.ParentQueryModelVisitor == null))
            {
                var itemType
                    = (referencedQuerySource as JoinClause)?.ItemType
                        ?? (referencedQuerySource as AdditionalFromClause)?.ItemType;

                if (itemType != null)
                {
                    var entityType
                        = _queryModelVisitor.QueryCompilationContext.Model
                            .FindEntityType(itemType);

                    if (entityType != null)
                    {
                        return Visit(
                            EntityQueryModelVisitor.CreatePropertyExpression(
                                expression, entityType.FindPrimaryKey().Properties[0]));
                    }
                }
            }

            var type = referencedQuerySource.ItemType.UnwrapNullableType().UnwrapEnumType();

            if (_relationalTypeMapper.FindMapping(type) != null)
            {
                var selectExpression = _queryModelVisitor.TryGetQuery(referencedQuerySource);

                if (selectExpression != null)
                {
                    var subquery = selectExpression.Tables.FirstOrDefault() as SelectExpression;

                    var innerProjectionExpression = subquery?.Projection.FirstOrDefault() as AliasExpression;
                    if (innerProjectionExpression != null)
                    {
                        if (innerProjectionExpression.Alias != null)
                        {
                            return new ColumnExpression(
                                innerProjectionExpression.Alias,
                                innerProjectionExpression.Type,
                                subquery);
                        }

                        var newExpression = selectExpression.UpdateColumnExpression(innerProjectionExpression.Expression, subquery);
                        return new AliasExpression(newExpression)
                        {
                            SourceMember = innerProjectionExpression.SourceMember
                        };
                    }
                }
                else
                {
                    selectExpression 
                        = _queryModelVisitor.QueryCompilationContext
                            .FindSelectExpression(referencedQuerySource);

                    if (selectExpression != null)
                    {
                        var projectionExpression = selectExpression.Projection.FirstOrDefault() as AliasExpression;

                        return new OuterPropertyExpression(
                            expression,
                            projectionExpression.TryGetColumnExpression().Property,
                            referencedQuerySource,
                            projectionExpression);
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
            => default(TResult);

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
