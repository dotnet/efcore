// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    /// <summary>
    ///     Rewrites comparisons of entities (as opposed to comparisons of their properties) into comparison of their keys.
    /// </summary>
    /// <remarks>
    ///     For example, an expression such as cs.Where(c => c == something) would be rewritten to cs.Where(c => c.Id == something.Id).
    /// </remarks>
    public class EntityEqualityRewritingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        ///     If the entity equality visitors introduces new runtime parameters (because it adds key access over existing parameters),
        ///     those parameters will have this prefix.
        /// </summary>
        private const string RuntimeParameterPrefix = CompiledQueryCache.CompiledQueryParameterPrefix + "entity_equality_";

        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        private static readonly MethodInfo _objectEqualsMethodInfo
            = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        public EntityEqualityRewritingExpressionVisitor(QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
            _logger = queryCompilationContext.Logger;
        }

        public Expression Rewrite(Expression expression) => Unwrap(Visit(expression));

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => constantExpression.IsEntityQueryable()
                ? new EntityReferenceExpression(
                    constantExpression,
                    _queryCompilationContext.Model.FindEntityType(((IQueryable)constantExpression.Value).ElementType))
                : (Expression)constantExpression;

        protected override Expression VisitNew(NewExpression newExpression)
        {
            var visitedArgs = Visit(newExpression.Arguments);
            var visitedExpression = newExpression.Update(visitedArgs.Select(Unwrap));

            return (newExpression.Members?.Count ?? 0) == 0
                ? (Expression)visitedExpression
                : new EntityReferenceExpression(visitedExpression, visitedExpression.Members
                    .Select((m, i) => (Member: m, Index: i))
                    .ToDictionary(
                        mi => mi.Member.Name,
                        mi => visitedArgs[mi.Index]));
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var visitedExpression = base.Visit(memberExpression.Expression);
            var visitedMemberExpression = memberExpression.Update(Unwrap(visitedExpression));
            return visitedExpression is EntityReferenceExpression entityWrapper
                ? entityWrapper.TraverseProperty(memberExpression.Member.Name, visitedMemberExpression)
                : visitedMemberExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var (newLeft, newRight) = (Visit(binaryExpression.Left), Visit(binaryExpression.Right));
            if (binaryExpression.NodeType == ExpressionType.Equal || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                if (RewriteEquality(binaryExpression.NodeType == ExpressionType.Equal, newLeft, newRight) is Expression result)
                {
                    return result;
                }
            }

            return binaryExpression.Update(Unwrap(newLeft), binaryExpression.Conversion, Unwrap(newRight));
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            // This is needed for Convert but is generalized
            var newOperand = Visit(unaryExpression.Operand);
            var newUnary = unaryExpression.Update(Unwrap(newOperand));
            return newOperand is EntityReferenceExpression entityWrapper
                ? entityWrapper.Update(newUnary)
                : (Expression)newUnary;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            // This is for "x is y"
            var visitedExpression = Visit(typeBinaryExpression.Expression);
            var visitedTypeBinary = typeBinaryExpression.Update(Unwrap(visitedExpression));
            return visitedExpression is EntityReferenceExpression entityWrapper
                ? entityWrapper.Update(visitedTypeBinary)
                : (Expression)visitedTypeBinary;
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var newTest = Visit(conditionalExpression.Test);
            var newIfTrue = Visit(conditionalExpression.IfTrue);
            var newIfFalse = Visit(conditionalExpression.IfFalse);

            var newConditional = conditionalExpression.Update(Unwrap(newTest), Unwrap(newIfTrue), Unwrap(newIfFalse));

            // TODO: the true and false sides may refer different entity types which happen to have the same
            // CLR type (e.g. shared entities)
            var wrapper = newIfTrue as EntityReferenceExpression ?? newIfFalse as EntityReferenceExpression;

            return wrapper == null ? (Expression)newConditional : wrapper.Update(newConditional);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var arguments = methodCallExpression.Arguments;
            Expression newSource;

            // Check if this is this Equals()
            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Object != null
                && methodCallExpression.Arguments.Count == 1)
            {
                var (newLeft, newRight) = (Visit(methodCallExpression.Object), Visit(arguments[0]));
                return RewriteEquality(true, newLeft, newRight)
                       ?? methodCallExpression.Update(Unwrap(newLeft), new[] { Unwrap(newRight) });
            }

            if (methodCallExpression.Method.Equals(_objectEqualsMethodInfo))
            {
                var (newLeft, newRight) = (Visit(arguments[0]), Visit(arguments[1]));
                return RewriteEquality(true, newLeft, newRight)
                       ?? methodCallExpression.Update(null, new[] { Unwrap(newLeft), Unwrap(newRight) });
            }

            // Navigation via EF.Property() or via an indexer property
            if (methodCallExpression.TryGetEFPropertyArguments(out _, out var propertyName)
                || methodCallExpression.TryGetEFIndexerArguments(out _, out propertyName))
            {
                newSource = Visit(arguments[0]);
                var newMethodCall = methodCallExpression.Update(null, new[] { Unwrap(newSource), arguments[1] });
                return newSource is EntityReferenceExpression entityWrapper
                       ? entityWrapper.TraverseProperty(propertyName, newMethodCall)
                       : newMethodCall;
            }

            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                || methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                || methodCallExpression.Method.DeclaringType == typeof(QueryableExtensions))
            {
                switch (methodCallExpression.Method.Name)
                {
                    // These are methods that require special handling
                    case nameof(Queryable.Contains) when arguments.Count == 2:
                        return VisitContainsMethodCall(methodCallExpression);

                    case nameof(Queryable.OrderBy) when arguments.Count == 2:
                    case nameof(Queryable.OrderByDescending) when arguments.Count == 2:
                    case nameof(Queryable.ThenBy) when arguments.Count == 2:
                    case nameof(Queryable.ThenByDescending) when arguments.Count == 2:
                        return VisitOrderingMethodCall(methodCallExpression);

                    // The following are projecting methods, which flow the entity type from *within* the lambda outside.
                    case nameof(Queryable.Select):
                    case nameof(Queryable.SelectMany):
                        return VisitSelectMethodCall(methodCallExpression);

                    case nameof(Queryable.GroupJoin):
                    case nameof(Queryable.Join):
                    case nameof(QueryableExtensions.LeftJoin):
                        return VisitJoinMethodCall(methodCallExpression);

                    case nameof(Queryable.GroupBy): // TODO: Implement
                        break;
                }
            }

            // TODO: Can add an extension point that can be overridden by subclassing visitors to recognize additional methods and flow through the entity type.
            // Do this here, since below we visit the arguments (avoid double visitation)

            if (arguments.Count == 0)
            {
                return methodCallExpression.Update(
                    Unwrap(Visit(methodCallExpression.Object)), Array.Empty<Expression>());
            }

            // Methods with a typed first argument (source), and with no lambda arguments or a single lambda
            // argument that has one parameter are rewritten automatically (e.g. Where(), FromSql(), Average()
            var newArguments = new Expression[arguments.Count];
            var lambdaArgs = arguments.Select(a => a.GetLambdaOrNull()).Where(l => l != null).ToArray();
            newSource = Visit(arguments[0]);
            newArguments[0] = Unwrap(newSource);
            if (methodCallExpression.Object == null
                && newSource is EntityReferenceExpression newSourceWrapper
                && (lambdaArgs.Length == 0
                    || lambdaArgs.Length == 1 && lambdaArgs[0].Parameters.Count == 1))
            {
                for (var i = 1; i < arguments.Count; i++)
                {
                    // Visit all arguments, rewriting the single lambda to replace its parameter expression
                    newArguments[i] = arguments[i].GetLambdaOrNull() is LambdaExpression lambda
                        ? Unwrap(RewriteAndVisitLambda(lambda, newSourceWrapper))
                        : Unwrap(Visit(arguments[i]));
                }

                var sourceParamType = methodCallExpression.Method.GetParameters()[0].ParameterType;
                var sourceElementType = sourceParamType.TryGetSequenceType();
                if (sourceElementType != null
                    || sourceParamType == typeof(IQueryable))   // OfType
                {
                    // If the method returns the element same type as the source, flow the type information
                    // (e.g. Where)
                    if (methodCallExpression.Method.ReturnType.TryGetSequenceType() is Type returnElementType
                        && (returnElementType == sourceElementType || sourceElementType == null))
                    {
                        return newSourceWrapper.Update(
                            methodCallExpression.Update(null, newArguments));
                    }

                    // If the source type is an IQueryable over the return type, this is a cardinality-reducing method (e.g. First).
                    // These don't flow the last navigation. In addition, these will be translated into a subquery, and we should not
                    // perform entity equality rewriting if the entity type has a composite key.
                    if (methodCallExpression.Method.ReturnType == sourceElementType)
                    {
                        return new EntityReferenceExpression(
                            methodCallExpression.Update(null, newArguments),
                            newSourceWrapper.EntityType,
                            lastNavigation: null,
                            newSourceWrapper.AnonymousType,
                            subqueryTraversed: true);
                    }
                }

                // Method does not flow entity type (e.g. Average)
                return methodCallExpression.Update(null, newArguments);
            }

            // Unknown method - still need to visit all arguments
            for (var i = 1; i < arguments.Count; i++)
            {
                newArguments[i] = Unwrap(Visit(arguments[i]));
            }

            return methodCallExpression.Update(Unwrap(Visit(methodCallExpression.Object)), newArguments);
        }

        protected virtual Expression VisitContainsMethodCall(MethodCallExpression methodCallExpression)
        {
            var arguments = methodCallExpression.Arguments;
            var newSource = Visit(arguments[0]);
            var newItem = Visit(arguments[1]);

            var sourceEntityType = (newSource as EntityReferenceExpression)?.EntityType;
            var itemEntityType = (newItem as EntityReferenceExpression)?.EntityType;

            if (sourceEntityType == null && itemEntityType == null)
            {
                return methodCallExpression.Update(null, new[] { newSource, newItem });
            }

            if (sourceEntityType != null && itemEntityType != null
                && sourceEntityType.RootType() != itemEntityType.RootType())
            {
                return Expression.Constant(false);
            }

            // One side of the comparison may have an unknown entity type (closure parameter, inline instantiation)
            var entityType = sourceEntityType ?? itemEntityType;

            var keyProperties = entityType.FindPrimaryKey().Properties;
            var keyProperty = keyProperties.Count == 1
                ? keyProperties.Single()
                : throw new NotSupportedException(CoreStrings.EntityEqualityContainsWithCompositeKeyNotSupported(entityType.DisplayName()));

            // Wrap the source with a projection to its primary key, and the item with a primary key access expression
            var param = Expression.Parameter(entityType.ClrType, "v");
            var keySelector = Expression.Lambda(CreatePropertyAccessExpression(param, keyProperty), param);
            var keyProjection = Expression.Call(
                LinqMethodHelpers.QueryableSelectMethodInfo.MakeGenericMethod(entityType.ClrType, keyProperty.ClrType),
                Unwrap(newSource),
                keySelector);

            var rewrittenItem = newItem.IsNullConstantExpression()
                ? Expression.Constant(null)
                : CreatePropertyAccessExpression(Unwrap(newItem), keyProperty);

            return Expression.Call(
                LinqMethodHelpers.QueryableContainsMethodInfo.MakeGenericMethod(keyProperty.ClrType),
                keyProjection,
                rewrittenItem
            );
        }

        protected virtual Expression VisitOrderingMethodCall(MethodCallExpression methodCallExpression)
        {
            var arguments = methodCallExpression.Arguments;
            var newSource = Visit(arguments[0]);

            if (!(newSource is EntityReferenceExpression sourceWrapper))
            {
                return methodCallExpression.Update(null, new[] { newSource, Unwrap(Visit(arguments[1])) });
            }

            var newKeySelector = RewriteAndVisitLambda(arguments[1].UnwrapLambdaFromQuote(), sourceWrapper);

            if (!(newKeySelector.Body is EntityReferenceExpression keySelectorWrapper)
                || !(keySelectorWrapper.EntityType is IEntityType entityType))
            {
                return sourceWrapper.Update(
                    methodCallExpression.Update(null, new[] { Unwrap(newSource), Unwrap(newKeySelector) }));
            }

            var genericMethodDefinition = methodCallExpression.Method.GetGenericMethodDefinition();
            var firstOrdering =
                genericMethodDefinition == LinqMethodHelpers.QueryableOrderByMethodInfo
                || genericMethodDefinition == LinqMethodHelpers.QueryableOrderByDescendingMethodInfo;
            var isAscending =
                genericMethodDefinition == LinqMethodHelpers.QueryableOrderByMethodInfo
                || genericMethodDefinition == LinqMethodHelpers.QueryableThenByMethodInfo;

            var keyProperties = entityType.FindPrimaryKey().Properties;
            var expression = Unwrap(newSource);
            var body = Unwrap(newKeySelector.Body);
            var oldParam = newKeySelector.Parameters.Single();

            foreach (var keyProperty in keyProperties)
            {
                var param = Expression.Parameter(entityType.ClrType, "v");
                var rewrittenKeySelector = Expression.Lambda(
                    ReplacingExpressionVisitor.Replace(
                        oldParam, param,
                        CreatePropertyAccessExpression(body, keyProperty)),
                    param);

                var orderingMethodInfo = GetOrderingMethodInfo(firstOrdering, isAscending);

                expression = Expression.Call(
                    orderingMethodInfo.MakeGenericMethod(entityType.ClrType, keyProperty.ClrType),
                    expression,
                    rewrittenKeySelector
                );

                firstOrdering = false;
            }

            return expression;

            static MethodInfo GetOrderingMethodInfo(bool firstOrdering, bool ascending)
            {
                if (firstOrdering)
                {
                    return ascending
                        ? LinqMethodHelpers.QueryableOrderByMethodInfo
                        : LinqMethodHelpers.QueryableOrderByDescendingMethodInfo;
                }
                return ascending
                    ? LinqMethodHelpers.QueryableThenByMethodInfo
                    : LinqMethodHelpers.QueryableThenByDescendingMethodInfo;
            }
        }

        protected virtual Expression VisitSelectMethodCall(MethodCallExpression methodCallExpression)
        {
            var arguments = methodCallExpression.Arguments;
            var newSource = Visit(arguments[0]);

            if (!(newSource is EntityReferenceExpression sourceWrapper))
            {
                return arguments.Count == 2
                    ? methodCallExpression.Update(null, new[] { newSource, Unwrap(Visit(arguments[1])) })
                    : arguments.Count == 3
                        ? methodCallExpression.Update(null, new[] { newSource, Unwrap(Visit(arguments[1])), Unwrap(Visit(arguments[2])) })
                        : throw new NotSupportedException();
            }

            MethodCallExpression newMethodCall;

            if (arguments.Count == 2)
            {
                var selector = arguments[1].UnwrapLambdaFromQuote();
                var newSelector = RewriteAndVisitLambda(selector, sourceWrapper);

                newMethodCall = methodCallExpression.Update(null, new[] { Unwrap(newSource), Unwrap(newSelector) });
                return newSelector.Body is EntityReferenceExpression entityWrapper
                    ? entityWrapper.Update(newMethodCall)
                    : (Expression)newMethodCall;
            }

            if (arguments.Count == 3)
            {
                var collectionSelector = arguments[1].UnwrapLambdaFromQuote();
                var newCollectionSelector = RewriteAndVisitLambda(collectionSelector, sourceWrapper);

                var resultSelector = arguments[2].UnwrapLambdaFromQuote();
                var newResultSelector = newCollectionSelector.Body is EntityReferenceExpression newCollectionSelectorWrapper
                    ? RewriteAndVisitLambda(resultSelector, sourceWrapper, newCollectionSelectorWrapper)
                    : (LambdaExpression)Visit(resultSelector);

                newMethodCall = methodCallExpression.Update(null, new[] { Unwrap(newSource), Unwrap(newCollectionSelector), Unwrap(newResultSelector) });
                return newResultSelector.Body is EntityReferenceExpression entityWrapper
                    ? entityWrapper.Update(newMethodCall)
                    : (Expression)newMethodCall;
            }

            throw new NotSupportedException();
        }

        protected virtual Expression VisitJoinMethodCall(MethodCallExpression methodCallExpression)
        {
            var arguments = methodCallExpression.Arguments;

            if (arguments.Count != 5)
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            var newOuter = Visit(arguments[0]);
            var newInner = Visit(arguments[1]);
            var outerKeySelector = arguments[2].UnwrapLambdaFromQuote();
            var innerKeySelector = arguments[3].UnwrapLambdaFromQuote();
            var resultSelector = arguments[4].UnwrapLambdaFromQuote();

            if (!(newOuter is EntityReferenceExpression outerWrapper && newInner is EntityReferenceExpression innerWrapper))
            {
                return methodCallExpression.Update(null, new[]
                {
                    Unwrap(newOuter), Unwrap(newInner), Unwrap(Visit(outerKeySelector)), Unwrap(Visit(innerKeySelector)), Unwrap(Visit(resultSelector))
                });
            }

            var newOuterKeySelector = RewriteAndVisitLambda(outerKeySelector, outerWrapper);
            var newInnerKeySelector = RewriteAndVisitLambda(innerKeySelector, innerWrapper);
            var newResultSelector = RewriteAndVisitLambda(resultSelector, outerWrapper, innerWrapper);

            MethodCallExpression newMethodCall;

            // If both outer and inner key selectors project to the same entity type, that's an entity equality
            // we need to rewrite.
            if (newOuterKeySelector.Body is EntityReferenceExpression outerKeySelectorWrapper
                && newInnerKeySelector.Body is EntityReferenceExpression innerKeySelectorWrapper
                && outerKeySelectorWrapper.IsEntityType && innerKeySelectorWrapper.IsEntityType
                && outerKeySelectorWrapper.EntityType.RootType() == innerKeySelectorWrapper.EntityType.RootType())
            {
                var entityType = outerKeySelectorWrapper.EntityType;
                var keyProperties = entityType.FindPrimaryKey().Properties;

                if (keyProperties.Count > 1
                    && (outerKeySelectorWrapper.SubqueryTraversed || innerKeySelectorWrapper.SubqueryTraversed))
                {
                    // One side of the comparison is the result of a subquery, and we have a composite key.
                    // Rewriting this would mean evaluating the subquery more than once, so we don't do it.
                    throw new NotSupportedException(CoreStrings.SubqueryWithCompositeKeyNotSupported(entityType.DisplayName()));
                }

                // Rewrite the lambda bodies, adding the key access on top of whatever is there, and then
                // produce a new MethodInfo and MethodCallExpression
                var origGenericArguments = methodCallExpression.Method.GetGenericArguments();

                var outerKeyAccessExpression = CreateKeyAccessExpression(Unwrap(outerKeySelectorWrapper), keyProperties);
                var outerKeySelectorType = typeof(Func<,>).MakeGenericType(origGenericArguments[0], outerKeyAccessExpression.Type);
                newOuterKeySelector = Expression.Lambda(
                    outerKeySelectorType,
                    outerKeyAccessExpression,
                    newOuterKeySelector.TailCall,
                    newOuterKeySelector.Parameters);

                var innerKeyAccessExpression = CreateKeyAccessExpression(Unwrap(innerKeySelectorWrapper), keyProperties);
                var innerKeySelectorType = typeof(Func<,>).MakeGenericType(origGenericArguments[1], innerKeyAccessExpression.Type);
                newInnerKeySelector = Expression.Lambda(
                    innerKeySelectorType,
                    innerKeyAccessExpression,
                    newInnerKeySelector.TailCall,
                    newInnerKeySelector.Parameters);

                var newMethod = methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(
                    origGenericArguments[0], origGenericArguments[1], outerKeyAccessExpression.Type, origGenericArguments[3]);

                newMethodCall = Expression.Call(
                    newMethod,
                    Unwrap(newOuter), Unwrap(newInner),
                    newOuterKeySelector, newInnerKeySelector,
                    Unwrap(newResultSelector));
            }
            else
            {
                newMethodCall = methodCallExpression.Update(null, new[]
                {
                    Unwrap(newOuter), Unwrap(newInner), Unwrap(newOuterKeySelector), Unwrap(newInnerKeySelector), Unwrap(newResultSelector)
                });
            }

            return newResultSelector.Body is EntityReferenceExpression wrapper
                ? wrapper.Update(newMethodCall)
                : (Expression)newMethodCall;
        }

        /// <summary>
        ///     Replaces the lambda's single parameter with a type wrapper based on the given source, and then visits
        ///     the lambda's body.
        /// </summary>
        protected LambdaExpression RewriteAndVisitLambda(LambdaExpression lambda, EntityReferenceExpression source)
            => Expression.Lambda(
                lambda.Type,
                Visit(ReplacingExpressionVisitor.Replace(
                    lambda.Parameters.Single(),
                    source.Update(lambda.Parameters.Single()),
                    lambda.Body)),
                lambda.TailCall,
                lambda.Parameters);

        /// <summary>
        ///     Replaces the lambda's two parameters with type wrappers based on the given sources, and then visits
        ///     the lambda's body.
        /// </summary>
        protected LambdaExpression RewriteAndVisitLambda(LambdaExpression lambda,
            EntityReferenceExpression source1,
            EntityReferenceExpression source2)
            => Expression.Lambda(
                lambda.Type,
                Visit(ReplacingExpressionVisitor.Replace(
                    lambda.Parameters[0], source1.Update(lambda.Parameters[0]),
                    lambda.Parameters[1], source2.Update(lambda.Parameters[1]),
                    lambda.Body)),
                lambda.TailCall,
                lambda.Parameters);

        /// <summary>
        ///     Receives already-visited left and right operands of an equality expression and applies entity equality rewriting to them,
        ///     if possible.
        /// </summary>
        /// <returns> The rewritten entity equality expression, or null if rewriting could not occur for some reason. </returns>
        protected virtual Expression RewriteEquality(bool equality, Expression left, Expression right)
        {
            // TODO: Consider throwing if a child has no flowed entity type, but has a Type that corresponds to an entity type on the model.
            // TODO: This would indicate an issue in our flowing logic, and would help the user (and us) understand what's going on.

            var leftTypeWrapper = left as EntityReferenceExpression;
            var rightTypeWrapper = right as EntityReferenceExpression;

            // If one of the sides is an anonymous object, or both sides are unknown, abort
            if (leftTypeWrapper == null && rightTypeWrapper == null
                || leftTypeWrapper?.IsAnonymousType == true
                || rightTypeWrapper?.IsAnonymousType == true)
            {
                return null;
            }

            // Handle null constants
            if (left.IsNullConstantExpression())
            {
                if (right.IsNullConstantExpression())
                {
                    return equality ? Expression.Constant(true) : Expression.Constant(false);
                }

                return rightTypeWrapper?.IsEntityType == true
                    ? RewriteNullEquality(equality, rightTypeWrapper.EntityType, rightTypeWrapper.Underlying, rightTypeWrapper.LastNavigation)
                    : null;
            }

            if (right.IsNullConstantExpression())
            {
                return leftTypeWrapper?.IsEntityType == true
                    ? RewriteNullEquality(equality, leftTypeWrapper.EntityType, leftTypeWrapper.Underlying, leftTypeWrapper.LastNavigation)
                    : null;
            }

            if (leftTypeWrapper != null
                && rightTypeWrapper != null
                && leftTypeWrapper.EntityType.RootType() != rightTypeWrapper.EntityType.RootType())
            {
                return Expression.Constant(!equality);
            }

            // One side of the comparison may have an unknown entity type (closure parameter, inline instantiation)
            var entityType = (leftTypeWrapper ?? rightTypeWrapper).EntityType;

            return RewriteEntityEquality(
                equality, entityType,
                Unwrap(left), leftTypeWrapper?.LastNavigation,
                Unwrap(right), rightTypeWrapper?.LastNavigation,
                leftTypeWrapper?.SubqueryTraversed == true || rightTypeWrapper?.SubqueryTraversed == true);
        }

        private Expression RewriteNullEquality(
            bool equality,
            [NotNull] IEntityType entityType,
            [NotNull] Expression nonNullExpression,
            [CanBeNull] INavigation lastNavigation)
        {
            if (lastNavigation?.IsCollection() == true)
            {
                // collection navigation is only null if its parent entity is null (null propagation thru navigation)
                // it is probable that user wanted to see if the collection is (not) empty
                // log warning suggesting to use Any() instead.
                _logger.PossibleUnintendedCollectionNavigationNullComparisonWarning(lastNavigation);
                return RewriteNullEquality(equality, lastNavigation.DeclaringEntityType, UnwrapLastNavigation(nonNullExpression), null);
            }

            var keyProperties = entityType.FindPrimaryKey().Properties;

            // TODO: bring back foreign key comparison optimization (#15826)

            // When comparing an entity to null, it's sufficient to simply compare its first primary key column to null.
            // (this is also why we can do it even over a subquery with a composite key)
            return Expression.MakeBinary(
                equality ? ExpressionType.Equal : ExpressionType.NotEqual,
                CreatePropertyAccessExpression(nonNullExpression, keyProperties[0], makeNullable: true),
                Expression.Constant(null));
        }

        private Expression RewriteEntityEquality(
            bool equality,
            [NotNull] IEntityType entityType,
            [NotNull] Expression left, [CanBeNull] INavigation leftNavigation,
            [NotNull] Expression right, [CanBeNull] INavigation rightNavigation,
            bool subqueryTraversed)
        {
            if (leftNavigation?.IsCollection() == true || rightNavigation?.IsCollection() == true)
            {
                if (leftNavigation?.Equals(rightNavigation) == true)
                {
                    // Log a warning that comparing 2 collections causes reference comparison
                    _logger.PossibleUnintendedReferenceComparisonWarning(left, right);
                    return RewriteEntityEquality(
                        equality, leftNavigation.DeclaringEntityType,
                        UnwrapLastNavigation(left), null,
                        UnwrapLastNavigation(right), null,
                        subqueryTraversed);
                }

                return Expression.Constant(!equality);
            }

            var keyProperties = entityType.FindPrimaryKey().Properties;

            if (subqueryTraversed && keyProperties.Count > 1)
            {
                // One side of the comparison is the result of a subquery, and we have a composite key.
                // Rewriting this would mean evaluating the subquery more than once, so we don't do it.
                throw new NotSupportedException(CoreStrings.SubqueryWithCompositeKeyNotSupported(entityType.DisplayName()));
            }

            return Expression.MakeBinary(
                equality ? ExpressionType.Equal : ExpressionType.NotEqual,
                CreateKeyAccessExpression(Unwrap(left), keyProperties),
                CreateKeyAccessExpression(Unwrap(right), keyProperties));
        }

        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case EntityReferenceExpression _:
                    // If the expression is an EntityReferenceExpression, simply returns it as all rewriting has already occurred.
                    // This is necessary when traversing wrapping expressions that have been injected into the lambda for parameters.
                    return expression;

                case NullConditionalExpression nullConditionalExpression:
                    return VisitNullConditional(nullConditionalExpression);

                default:
                    return base.VisitExtension(expression);
            }
        }

        protected virtual Expression VisitNullConditional(NullConditionalExpression expression)
        {
            var newCaller = Visit(expression.Caller);
            var newAccessOperation = Visit(expression.AccessOperation);
            var visitedExpression = expression.Update(Unwrap(newCaller), Unwrap(newAccessOperation));

            // TODO: Can the access operation be anything else than a MemberExpression?
            return newCaller is EntityReferenceExpression wrapper
                   && expression.AccessOperation is MemberExpression memberExpression
                ? wrapper.TraverseProperty(memberExpression.Member.Name, visitedExpression)
                : visitedExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        // TODO: DRY with NavigationExpansionHelpers
        protected Expression CreateKeyAccessExpression(
            [NotNull] Expression target,
            [NotNull] IReadOnlyList<IProperty> properties)
            => properties.Count == 1
                ? CreatePropertyAccessExpression(target, properties[0])
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(
                                p =>
                                    Expression.Convert(
                                        CreatePropertyAccessExpression(target, p),
                                        typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));

        private Expression CreatePropertyAccessExpression(Expression target, IProperty property, bool makeNullable = false)
        {
            // The target is a constant - evaluate the property immediately and return the result
            if (target is ConstantExpression constantExpression)
            {
                return Expression.Constant(property.GetGetter().GetClrValue(constantExpression.Value), property.ClrType);
            }

            // If the target is a query parameter, we can't simply add a property access over it, but must instead cause a new
            // parameter to be added at runtime, with the value of the property on the base parameter.
            if (target is ParameterExpression baseParameterExpression
                && baseParameterExpression.Name.StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix, StringComparison.Ordinal))
            {
                // Generate an expression to get the base parameter from the query context's parameter list, and extract the
                // property from that
                var lambda = Expression.Lambda(
                    Expression.Call(
                        _parameterValueExtractor,
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(baseParameterExpression.Name, typeof(string)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter
                );

                var newParameterName = $"{RuntimeParameterPrefix}{baseParameterExpression.Name.Substring(CompiledQueryCache.CompiledQueryParameterPrefix.Length)}_{property.Name}";
                _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                return Expression.Parameter(property.ClrType, newParameterName);
            }

            return target.CreateEFPropertyExpression(property, makeNullable);

        }

        private static object ParameterValueExtractor(QueryContext context, string baseParameterName, IProperty property)
        {
            var baseParameter = context.ParameterValues[baseParameterName];
            return baseParameter == null ? null : property.GetGetter().GetClrValue(baseParameter);
        }

        private static readonly MethodInfo _parameterValueExtractor
                = typeof(EntityEqualityRewritingExpressionVisitor)
                    .GetTypeInfo()
                    .GetDeclaredMethod(nameof(ParameterValueExtractor));

        protected static Expression UnwrapLastNavigation(Expression expression)
            => (expression as MemberExpression)?.Expression
               ?? (expression is MethodCallExpression methodCallExpression
                   && methodCallExpression.IsEFProperty()
                   ? methodCallExpression.Arguments[0]
                   : null);

        protected static Expression Unwrap(Expression expression)
            => expression switch
            {
                EntityReferenceExpression wrapper => wrapper.Underlying,
                LambdaExpression lambda when lambda.Body is EntityReferenceExpression wrapper =>
                    Expression.Lambda(
                        lambda.Type,
                        wrapper.Underlying,
                        lambda.TailCall,
                        lambda.Parameters),
                _ => expression
            };

        protected class EntityReferenceExpression : Expression
        {
            public override ExpressionType NodeType => ExpressionType.Extension;

            /// <summary>
            ///     The underlying expression being wrapped.
            /// </summary>
            [NotNull]
            public Expression Underlying { get; }

            public override Type Type => Underlying.Type;

            [CanBeNull]
            public IEntityType EntityType { get; }

            [CanBeNull]
            public INavigation LastNavigation => EntityType == null ? null : _lastNavigation;

            [CanBeNull]
            private readonly INavigation _lastNavigation;

            [CanBeNull]
            public Dictionary<string, Expression> AnonymousType { get; }

            public bool SubqueryTraversed { get; }

            public bool IsAnonymousType => AnonymousType != null;
            public bool IsEntityType => EntityType != null;

            public EntityReferenceExpression(Expression underlying, Dictionary<string, Expression> anonymousType)
            {
                Underlying = underlying;
                AnonymousType = anonymousType;
            }

            public EntityReferenceExpression(Expression underlying, IEntityType entityType)
                : this(underlying, entityType, null, false)
            {
            }

            private EntityReferenceExpression(Expression underlying, IEntityType entityType, INavigation lastNavigation, bool subqueryTraversed)
            {
                Underlying = underlying;
                EntityType = entityType;
                _lastNavigation = lastNavigation;
                SubqueryTraversed = subqueryTraversed;
            }

            public EntityReferenceExpression(
                Expression underlying,
                IEntityType entityType,
                INavigation lastNavigation,
                Dictionary<string, Expression> anonymousType,
                bool subqueryTraversed)
            {
                Underlying = underlying;
                EntityType = entityType;
                _lastNavigation = lastNavigation;
                AnonymousType = anonymousType;
                SubqueryTraversed = subqueryTraversed;
            }

            /// <summary>
            ///     Attempts to find <paramref name="propertyName"/> as a navigation from the current node,
            ///     and if successful, returns a new <see cref="EntityReferenceExpression"/> wrapping the
            ///     given expression. Otherwise returns the given expression without wrapping it.
            /// </summary>
            public virtual Expression TraverseProperty(string propertyName, Expression destinationExpression)
            {
                if (IsEntityType)
                {
                    return EntityType.FindNavigation(propertyName) is INavigation navigation
                        ? new EntityReferenceExpression(
                            destinationExpression,
                            navigation.GetTargetType(),
                            navigation,
                            SubqueryTraversed)
                        : destinationExpression;
                }

                if (IsAnonymousType)
                {
                    if (AnonymousType.TryGetValue(propertyName, out var expression)
                        && expression is EntityReferenceExpression wrapper)
                    {
                        return wrapper.IsEntityType
                            ? new EntityReferenceExpression(destinationExpression, wrapper.EntityType)
                            : new EntityReferenceExpression(destinationExpression, wrapper.AnonymousType);
                    }

                    return destinationExpression;
                }

                throw new NotSupportedException("Unknown type info");
            }

            public EntityReferenceExpression Update(Expression newUnderlying)
                => new EntityReferenceExpression(newUnderlying, EntityType, _lastNavigation, AnonymousType, SubqueryTraversed);

            protected override Expression VisitChildren(ExpressionVisitor visitor)
                => Update(visitor.Visit(Underlying));

            public virtual void Print(ExpressionPrinter expressionPrinter)
            {
                expressionPrinter.Visit(Underlying);

                if (IsEntityType)
                {
                    expressionPrinter.StringBuilder.Append($".EntityType({EntityType})");
                }
                else if (IsAnonymousType)
                {
                    expressionPrinter.StringBuilder.Append(".AnonymousObject");
                }

                if (SubqueryTraversed)
                {
                    expressionPrinter.StringBuilder.Append(".SubqueryTraversed");
                }
            }

            public override string ToString() => $"{Underlying}[{(IsEntityType ? EntityType.ShortName() : "AnonymousObject")}{(SubqueryTraversed ? ", Subquery" : "")}]";
        }
    }
}
