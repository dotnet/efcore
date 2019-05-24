// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    /// <summary>
    /// Rewrites comparisons of entities (as opposed to comparisons of their properties) into comparison of their keys.
    /// </summary>
    /// <remarks>
    /// For example, an expression such as cs.Where(c => c == something) would be rewritten to cs.Where(c => c.Id == something.Id).
    /// </remarks>
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]  // TODO: Check
    public class EntityEqualityRewritingExpressionVisitor2 : ExpressionVisitor
    {
        protected QueryCompilationContext2 QueryCompilationContext { get; }
        protected IModel Model { get; }

        /// <summary>
        /// The current type being flowed through the expression tree. Can be either an EntityTypeAndNavigation (for
        /// entities) or a dictionary for projected anonymous types.
        /// </summary>
        protected EntityEqualityTypeInfo CurrentTypeInfo { get; set; }

        [NotNull]
        protected Dictionary<ParameterExpression, EntityEqualityTypeInfo> ParameterBindings { get; private set; }

        protected Stack<(EntityEqualityTypeInfo CurrentType, Dictionary<ParameterExpression, EntityEqualityTypeInfo> ParameterBindings)> Stack { get; }

        private static readonly MethodInfo _objectEqualsMethodInfo
            = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        public EntityEqualityRewritingExpressionVisitor2(QueryCompilationContext2 queryCompilationContext)
        {
            QueryCompilationContext = queryCompilationContext;
            Model = queryCompilationContext.Model;
            ParameterBindings = new Dictionary<ParameterExpression, EntityEqualityTypeInfo>();
            Stack = new Stack<(EntityEqualityTypeInfo, Dictionary<ParameterExpression, EntityEqualityTypeInfo>)>();
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.IsEntityQueryable())
            {
                CurrentTypeInfo = new EntityEqualityTypeInfo(
                    Model.FindEntityType(((IQueryable)constantExpression.Value).ElementType));
            }

            return constantExpression;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            if ((newExpression.Members?.Count ?? 0) == 0)
            {
                return base.VisitNew(newExpression);
            }

            var dict = new Dictionary<string, EntityEqualityTypeInfo>();
            Expression[] newArgs = null;
            for (var i = 0; i < newExpression.Members.Count; i++)
            {
                var arg = newExpression.Arguments[i];
                var newArg = VisitAndReturnTypeInfo(arg, out var argTypeInfo);
                dict[newExpression.Members[i].Name] = argTypeInfo;

                // Write the visited argument into a new arguments array, but only if any argument has already been modified
                if (newArg != arg && newArgs == null)
                {
                    newArgs = new Expression[newExpression.Arguments.Count];
                    newExpression.Arguments.CopyTo(newArgs, 0);
                }
                if (newArgs != null)
                {
                    newArgs[i] = newArg;
                }
            }

            CurrentTypeInfo = new EntityEqualityTypeInfo(dict);
            return newExpression.Update((IEnumerable<Expression>)newArgs ?? newExpression.Arguments);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = (MemberExpression)base.VisitMember(memberExpression);
            TraverseProperty(newExpression.Member.Name);
            return newExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            CurrentTypeInfo = ParameterBindings.TryGetValue(parameterExpression, out var parameterEntityType)
                ? parameterEntityType
                : EntityEqualityTypeInfo.None;

            return parameterExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            // Check if this is this Equals()
            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Object != null
                && methodCallExpression.Arguments.Count == 1)
            {
                CurrentTypeInfo = EntityEqualityTypeInfo.None;
                var left = methodCallExpression.Object;
                var right = methodCallExpression.Arguments[0];
                return RewriteEquality(true, ref left, ref right) is Expression rewritten
                    ? rewritten
                    : methodCallExpression.Update(left, new[] { right });
            }

            if (methodCallExpression.Method.Equals(_objectEqualsMethodInfo))
            {
                CurrentTypeInfo = EntityEqualityTypeInfo.None;
                var left = methodCallExpression.Arguments[0];
                var right = methodCallExpression.Arguments[1];
                return RewriteEquality(true, ref left, ref right) is Expression rewritten
                    ? rewritten
                    : methodCallExpression.Update(null, new[] { left, right });
            }

            // Navigation via EF.Property() or via an indexer property
            if (methodCallExpression.TryGetEFPropertyArguments(out _, out var propertyName)
                || methodCallExpression.TryGetEFIndexerArguments(out _, out propertyName))
            {
                var result = base.VisitMethodCall(methodCallExpression);
                TraverseProperty(propertyName);
                return result;
            }

            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                || methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                || methodCallExpression.Method.DeclaringType == typeof(EntityQueryableExtensions))
            {
                switch (methodCallExpression.Method.Name)
                {
                    // Cardinality-reducing methods, these will be translated into a subquery - we should not
                    // perform entity equality rewriting if the entity type has a composite key.
                    case nameof(Queryable.ElementAtOrDefault):
                    case nameof(Queryable.First):
                    case nameof(Queryable.FirstOrDefault):
                    case nameof(Queryable.Last):
                    case nameof(Queryable.LastOrDefault):
                    case nameof(Queryable.Single):
                    case nameof(Queryable.SingleOrDefault):
                    {
                        var visited = VisitSimpleMethodCall(methodCallExpression);
                        CurrentTypeInfo = CurrentTypeInfo.SubqueryHasOccurred();
                        return visited;
                    }

                    // The following are projecting methods, which flow the entity type from *within* the lambda outside.
                    // These are handled by dedicated methods
                    case nameof(Queryable.Select):
                    case nameof(Queryable.SelectMany):
                        return VisitSelectMethodCall(methodCallExpression);

                    case nameof(Queryable.GroupJoin):
                    case nameof(Queryable.Join):
                    case nameof(EntityQueryableExtensions.LeftJoin):
                        return VisitJoinMethodCall(methodCallExpression);

                    case nameof(Queryable.GroupBy):  // TODO: Implement
                        break;
                }

                // Attempt to automatically handle simple cases (methods accepting one lambda with one parameter)
                if (VisitSimpleMethodCall(methodCallExpression) is Expression newMethodCall)
                {
                    // Flow type info forward only if the method returns the type it accepts as first param
                    // (e.g. Where(), FromSql(), but not Count())
                    if (!methodCallExpression.Method.ReturnType.IsAssignableFrom(methodCallExpression.Arguments[0].Type))
                    {
                        CurrentTypeInfo = EntityEqualityTypeInfo.None;
                    }

                    return newMethodCall;
                }
            }

            // If we're here, this is an unknown method call.
            // TODO: Need an extension point that can be overridden by subclassing visitors to recognize additional methods and flow through the entity type.
            CurrentTypeInfo = EntityEqualityTypeInfo.None;
            return methodCallExpression;
        }

        // If the method accepts a single lambda with a single parameter, assumes the parameter corresponds to the first argument
        // and visits the lambda, performing the necessary rewriting.
        protected virtual Expression VisitSimpleMethodCall(MethodCallExpression methodCallExpression)
        {
            var foundLambda = false;
            var args = methodCallExpression.Arguments;
            Expression[] newArgs = null;
            for (var i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                Expression newArg;
                switch (arg)
                {
                    case UnaryExpression quote when quote.NodeType == ExpressionType.Quote:
                    {
                        var lambda = quote.UnwrapQuote();
                        if (foundLambda || lambda.Parameters.Count != 1)
                        {
                            // Method is complex and needs manual handling
                            return null;
                        }

                        PushStackFrame(lambda.Parameters[0], CurrentTypeInfo);
                        newArg = Visit(quote);
                        PopStackFrame();

                        foundLambda = true;
                        break;
                    }

                    default:
                        newArg = Visit(arg);
                        break;
                }

                // Write the visited argument into a new arguments array, but only if any argument has already been modified
                if (newArg != arg && newArgs == null)
                {
                    newArgs = new Expression[args.Count];
                    args.CopyTo(newArgs, 0);
                }

                if (newArgs != null)
                {
                    newArgs[i] = newArg;
                }
            }

            return methodCallExpression.Update(null, (IEnumerable<Expression>)newArgs ?? args);
        }

        protected virtual Expression VisitSelectMethodCall(MethodCallExpression methodCallExpression)
        {
            var args = methodCallExpression.Arguments;

            var newSource = VisitAndReturnTypeInfo(args[0], out var sourceTypeInfo);

            if (args.Count == 2)
            {
                PushStackFrame(args[1].UnwrapQuote().Parameters[0], sourceTypeInfo);
                var newSelector = Visit(args[1]);
                PopStackFrame(preserveCurrentType: true);

                return methodCallExpression.Update(null, new[]
                {
                    newSource, newSelector
                });
            }

            if (args.Count == 3)
            {
                PushStackFrame(args[1].UnwrapQuote().Parameters[0], sourceTypeInfo);
                var newCollectionSelector = Visit(args[1]);
                PopStackFrame(preserveCurrentType: true);
                var collectionType = CurrentTypeInfo;

                var resultSelectorParams = args[2].UnwrapQuote().Parameters;
                PushStackFrame(resultSelectorParams[0], sourceTypeInfo, resultSelectorParams[1], collectionType);
                var newResultSelector = Visit(args[2]);
                PopStackFrame(preserveCurrentType: true);

                return methodCallExpression.Update(null, new[]
                {
                    newSource, newCollectionSelector, newResultSelector
                });
            }

            return methodCallExpression;
        }

        protected virtual Expression VisitJoinMethodCall(MethodCallExpression methodCallExpression)
        {
            var args = methodCallExpression.Arguments;

            if (args.Count != 5)
            {
                return methodCallExpression;
            }

            var newOuter = VisitAndReturnTypeInfo(args[0], out var outerTypeInfo);
            var newInner = VisitAndReturnTypeInfo(args[1], out var innerTypeInfo);

            PushStackFrame(args[2].UnwrapQuote().Parameters.Single(), outerTypeInfo);
            var newOuterKeySelector = Visit(args[2]);
            PopStackFrame();

            PushStackFrame(args[3].UnwrapQuote().Parameters.Single(), innerTypeInfo);
            var newInnerKeySelector = Visit(args[3]);
            PopStackFrame();

            var resultSelectorParams = args[4].UnwrapQuote().Parameters;
            PushStackFrame(resultSelectorParams[0], outerTypeInfo, resultSelectorParams[1], innerTypeInfo);
            var newResultSelector = Visit(args[4]);
            PopStackFrame(preserveCurrentType: true);

            return methodCallExpression.Update(null, new[]
            {
                newOuter, newInner, newOuterKeySelector, newInnerKeySelector, newResultSelector
            });
        }

        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            // The default implementation of VisitLambda visits the body, and then the parameters - this
            // can overwrite the type coming out of a Select lambda.
            // In general, don't visit parameters - this visitor has nothing to do there.
            var newBody = Visit(lambdaExpression.Body);
            return newBody == lambdaExpression.Body
                ? lambdaExpression
                : Expression.Lambda(newBody, lambdaExpression.TailCall, lambdaExpression.Parameters);
        }

        protected  void PushStackFrame(
            ParameterExpression newParam1, EntityEqualityTypeInfo newParamTypeInfo1,
            ParameterExpression newParam2, EntityEqualityTypeInfo newParamTypeInfo2)
        {
            PushStackFrame();
            ParameterBindings[newParam1] = newParamTypeInfo1;
            ParameterBindings[newParam2] = newParamTypeInfo2;
        }

        protected void PushStackFrame(ParameterExpression newParam1, EntityEqualityTypeInfo newParamTypeInfo)
        {
            PushStackFrame();
            ParameterBindings[newParam1] = newParamTypeInfo;
        }

        protected void PushStackFrame()
        {
            Stack.Push((CurrentTypeInfo, ParameterBindings));
            CurrentTypeInfo = EntityEqualityTypeInfo.None;
            ParameterBindings = new Dictionary<ParameterExpression, EntityEqualityTypeInfo>(ParameterBindings);
        }

        protected  void PopStackFrame(bool preserveCurrentType = false)
        {
            var frame = Stack.Pop();
            ParameterBindings = frame.ParameterBindings;
            if (!preserveCurrentType)
            {
                CurrentTypeInfo = frame.CurrentType;
            }
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            // TODO: This is a safety measure for now - not sure if any other binary expressions can occur with entity types directly
            // as their operands. But just in case we don't flow.
            CurrentTypeInfo = EntityEqualityTypeInfo.None;

            if (binaryExpression.NodeType == ExpressionType.Equal || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var left = binaryExpression.Left;
                var right = binaryExpression.Right;
                return RewriteEquality(binaryExpression.NodeType == ExpressionType.Equal, ref left, ref right) is Expression rewritten
                    ? rewritten
                    : binaryExpression.Update(left, binaryExpression.Conversion, right);
            }

            return base.VisitBinary(binaryExpression);
        }

        /// <summary>
        /// Attempts to perform entity equality rewriting. If successful, returns the rewritten expression. Otherwise, returns null
        /// and <paramref name="left"/>  and <paramref name="right"/> contain the visited operands.
        /// </summary>
        protected virtual Expression RewriteEquality(bool isEqual, ref Expression left, ref Expression right)
        {
            // Visit children and get their respective entity types
            // TODO: Consider throwing if a child has no flowed entity type, but has a Type that corresponds to an entity type on the model.
            // TODO: This would indicate an issue in our flowing logic, and would help the user (and us) understand what's going on.
            left = VisitAndReturnTypeInfo(left, out var leftTypeInfo);
            right = VisitAndReturnTypeInfo(right, out var rightTypeInfo);

            // If one of the sides is an anonymous object, or both sides are unknown, abort
            if (leftTypeInfo.IsAnonymousType || rightTypeInfo.IsAnonymousType || leftTypeInfo.IsUnknown && rightTypeInfo.IsUnknown)
            {
                return null;
            }

            // Handle null constants
            if (left.IsNullConstantExpression())
            {
                if (right.IsNullConstantExpression())
                {
                    return isEqual ? Expression.Constant(true) : Expression.Constant(false);
                }

                return rightTypeInfo.IsEntityType
                    ? RewriteNullEquality(isEqual, right, rightTypeInfo.EntityType, rightTypeInfo.LastNavigation)
                    : null;
            }

            if (right.IsNullConstantExpression())
            {
                return leftTypeInfo.IsEntityType
                    ? RewriteNullEquality(isEqual, left, leftTypeInfo.EntityType, leftTypeInfo.LastNavigation)
                    : null;
            }

            // No null constants

            if (leftTypeInfo.IsEntityType
                && rightTypeInfo.IsEntityType
                && leftTypeInfo.EntityType.RootType() != rightTypeInfo.EntityType.RootType())
            {
                return Expression.Constant(false);
            }

            // One side of the comparison may have an unknown type (closure parameter, inline instantiation)
            var typeInfo = leftTypeInfo.IsUnknown ? rightTypeInfo : leftTypeInfo;

            return RewriteEntityEquality(
                isEqual,
                typeInfo.EntityType,
                left, leftTypeInfo.LastNavigation,
                right, rightTypeInfo.LastNavigation,
                typeInfo.HasSubqueryOccurred);
        }

        private Expression RewriteNullEquality(
            bool isEqual,
            [NotNull] Expression nonNullExpression,
            [NotNull] IEntityType entityType,
            [CanBeNull] INavigation lastNavigation)
        {
            if (lastNavigation?.IsCollection() == true)
            {
                // collection navigation is only null if its parent entity is null (null propagation thru navigation)
                // it is probable that user wanted to see if the collection is (not) empty
                // log warning suggesting to use Any() instead.
                QueryCompilationContext.Logger.PossibleUnintendedCollectionNavigationNullComparisonWarning(lastNavigation);

                return RewriteNullEquality(isEqual, UnwrapLastNavigation(nonNullExpression), lastNavigation.DeclaringEntityType, null);
            }

            var keyProperties = entityType.FindPrimaryKey().Properties;

            // TODO: bring back foreign key comparison optimization (#15826)

            // When comparing an entity to null, it's sufficient to simply compare its first primary key column to null.
            // (this is also why we can do it even over a subquery with a composite key)
            return Expression.MakeBinary(
                isEqual ? ExpressionType.Equal : ExpressionType.NotEqual,
                nonNullExpression.CreateEFPropertyExpression(keyProperties[0]),
                Expression.Constant(null));
        }

        private Expression RewriteEntityEquality(
            bool isEqual,
            [NotNull] IEntityType entityType,
            [NotNull] Expression left,
            [CanBeNull] INavigation leftNavigation,
            [NotNull] Expression right,
            [CanBeNull] INavigation rightNavigation,
            bool hasSubqueryOccurred)
        {
            if (leftNavigation?.IsCollection() == true || rightNavigation?.IsCollection() == true)
            {
                if (leftNavigation?.Equals(rightNavigation) == true)
                {
                    // Log a warning that comparing 2 collections causes reference comparison
                    QueryCompilationContext.Logger.PossibleUnintendedReferenceComparisonWarning(left, right);

                    return RewriteEntityEquality(
                        isEqual,
                        leftNavigation.DeclaringEntityType,
                        UnwrapLastNavigation(left),
                        null,
                        UnwrapLastNavigation(right),
                        null,
                        hasSubqueryOccurred);
                }

                return Expression.Constant(!isEqual);
            }

            var keyProperties = entityType.FindPrimaryKey().Properties;

            if (hasSubqueryOccurred && keyProperties.Count > 1)
            {
                // One side of the comparison is the result of a subquery, and we have a composite key.
                // Rewriting this would mean evaluating the subquery more than once, so we don't do it.
                return null;
            }

            return Expression.MakeBinary(
                isEqual ? ExpressionType.Equal : ExpressionType.NotEqual,
                CreateKeyAccessExpression(left, keyProperties, nullComparison: false),
                CreateKeyAccessExpression(right, keyProperties, nullComparison: false));
        }

        private static Expression CreateKeyAccessExpression(
            Expression target,
            IReadOnlyList<IProperty> properties,
            bool nullComparison)
        {
            // If comparing with null then we need only first PK property
            return properties.Count == 1 || nullComparison
                ? target.CreateEFPropertyExpression(properties[0])
                : target.CreateKeyAccessExpression(properties);
        }

        private static Expression UnwrapLastNavigation(Expression expression)
            => (expression as MemberExpression)?.Expression
               ?? (expression is MethodCallExpression methodCallExpression
                   && methodCallExpression.TryGetEFPropertyArguments(out var source, out _)
                   ? source
                   : null);

        protected Expression VisitAndReturnTypeInfo(Expression expression, out EntityEqualityTypeInfo typeInfo)
        {
            var currentTypeInfo = CurrentTypeInfo;

            var visited = Visit(expression);
            typeInfo = CurrentTypeInfo;

            CurrentTypeInfo = currentTypeInfo;
            return visited;
        }

        protected virtual void TraverseProperty(string propertyName)
        {
            if (CurrentTypeInfo.IsEntityType)
            {
                var navigation = CurrentTypeInfo.EntityType.FindNavigation(propertyName);
                CurrentTypeInfo = navigation == null
                    ? EntityEqualityTypeInfo.None
                    : CurrentTypeInfo.NavigateTo(navigation?.GetTargetType(), navigation);
                return;
            }

            if (CurrentTypeInfo.IsAnonymousType)
            {
                CurrentTypeInfo = CurrentTypeInfo.AnonymousTypeInfo.TryGetValue(propertyName, out var typeInfo)
                    ? typeInfo
                    : EntityEqualityTypeInfo.None;
                return;
            }

            if (CurrentTypeInfo.IsUnknown)
            {
                return;
            }

            throw new NotSupportedException("Unknown type info");
        }

        protected readonly struct EntityEqualityTypeInfo
        {
            public EntityEqualityTypeInfo(Dictionary<string, EntityEqualityTypeInfo> anonymousTypeInfo)
            {
                AnonymousTypeInfo = anonymousTypeInfo;
                EntityType = null;
                _lastNavigation = null;
                HasSubqueryOccurred = false;
            }

            public EntityEqualityTypeInfo(IEntityType entityType, INavigation lastNavigation)
                : this(entityType, lastNavigation, false)
            {
            }

            public EntityEqualityTypeInfo(IEntityType entityType)
                : this(entityType, null, false)
            {
            }

            private EntityEqualityTypeInfo(IEntityType entityType, INavigation lastNavigation, bool hasSubqueryOccurred)
            {
                EntityType = entityType;
                _lastNavigation = lastNavigation;
                AnonymousTypeInfo = null;
                HasSubqueryOccurred = hasSubqueryOccurred;
            }

            public bool IsEntityType => EntityType != null;
            public bool IsAnonymousType => AnonymousTypeInfo != null;
            public bool IsUnknown => EntityType == null && AnonymousTypeInfo == null;

            public static EntityEqualityTypeInfo None = default;

            [CanBeNull]
            public IEntityType EntityType { get; }

            [CanBeNull]
            public INavigation LastNavigation => EntityType == null ? null : _lastNavigation;

            [CanBeNull]
            private readonly INavigation _lastNavigation;

            [CanBeNull]
            public Dictionary<string, EntityEqualityTypeInfo> AnonymousTypeInfo { get; }

            public bool HasSubqueryOccurred { get; }

            public EntityEqualityTypeInfo SubqueryHasOccurred()
                => new EntityEqualityTypeInfo(EntityType, null, true);

            public EntityEqualityTypeInfo NavigateTo(IEntityType entityType, INavigation navigation)
                => new EntityEqualityTypeInfo(EntityType, navigation, HasSubqueryOccurred);

            public override string ToString()
                => IsEntityType
                    ? $"{EntityType}" + (_lastNavigation == null ? null : $" (last nav: {_lastNavigation})")
                    : IsAnonymousType
                        ? "AnonymousType"
                        : "Unknown";
        }
    }
}
