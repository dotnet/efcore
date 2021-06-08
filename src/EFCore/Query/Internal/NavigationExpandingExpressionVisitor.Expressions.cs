// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class NavigationExpandingExpressionVisitor
    {
        private sealed class EntityReference : Expression, IPrintableExpression
        {
            public EntityReference(IEntityType entityType, QueryRootExpression? queryRootExpression)
            {
                EntityType = entityType;
                IncludePaths = new IncludeTreeNode(entityType, this, setLoaded: true);
                QueryRootExpression = queryRootExpression;
            }

            public IEntityType EntityType { get; }

            public IDictionary<(IForeignKey, bool), Expression> ForeignKeyExpansionMap { get; } =
                new Dictionary<(IForeignKey, bool), Expression>();

            public bool IsOptional { get; private set; }
            public IncludeTreeNode IncludePaths { get; private set; }
            public IncludeTreeNode? LastIncludeTreeNode { get; private set; }
            public QueryRootExpression? QueryRootExpression { get; private set; }

            public override ExpressionType NodeType
                => ExpressionType.Extension;

            public override Type Type
                => EntityType.ClrType;

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                Check.NotNull(visitor, nameof(visitor));

                return this;
            }

            public EntityReference Snapshot()
            {
                var result = new EntityReference(EntityType, QueryRootExpression) { IsOptional = IsOptional };
                result.IncludePaths = IncludePaths.Snapshot(result);

                return result;
            }

            public void SetLastInclude(IncludeTreeNode lastIncludeTree)
                => LastIncludeTreeNode = lastIncludeTree;

            public void MarkAsOptional()
                => IsOptional = true;

            void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
            {
                Check.NotNull(expressionPrinter, nameof(expressionPrinter));

                expressionPrinter.Append($"{nameof(EntityReference)}: {EntityType.DisplayName()}");
                if (IsOptional)
                {
                    expressionPrinter.Append("[Optional]");
                }

                if (IncludePaths.Count > 0)
                {
                    // TODO: fully render nested structure of include tree
                    expressionPrinter.Append(
                        " | IncludePaths: "
                        + string.Join(
                            " ", IncludePaths.Select(ip => ip.Value.Count() > 0 ? ip.Key.Name + "->..." : ip.Key.Name)));
                }
            }
        }

        /// <summary>
        ///     A tree structure of includes for a given entity type in <see cref="EntityReference" />.
        /// </summary>
        private sealed class IncludeTreeNode : Dictionary<INavigationBase, IncludeTreeNode>
        {
            private EntityReference? _entityReference;

            public IncludeTreeNode(IEntityType entityType)
                : this(entityType, null, setLoaded: true)
            {
            }

            public IncludeTreeNode(IEntityType entityType, EntityReference? entityReference, bool setLoaded)
            {
                EntityType = entityType;
                _entityReference = entityReference;
                SetLoaded = setLoaded;
            }

            public IEntityType EntityType { get; }
            public LambdaExpression? FilterExpression { get; private set; }
            public bool SetLoaded { get; private set; }

            public IncludeTreeNode AddNavigation(INavigationBase navigation, bool setLoaded)
            {
                if (TryGetValue(navigation, out var existingValue))
                {
                    if (setLoaded && !existingValue.SetLoaded)
                    {
                        existingValue.SetLoaded = true;
                    }

                    return existingValue;
                }

                IncludeTreeNode? nodeToAdd = null;
                if (_entityReference != null)
                {
                    if (navigation is INavigation concreteNavigation
                        && _entityReference.ForeignKeyExpansionMap.TryGetValue(
                            (concreteNavigation.ForeignKey, concreteNavigation.IsOnDependent), out var expansion))
                    {
                        // Value known to be non-null
                        nodeToAdd = UnwrapEntityReference(expansion)!.IncludePaths;
                    }
                    else if (navigation is ISkipNavigation skipNavigation
                        && _entityReference.ForeignKeyExpansionMap.TryGetValue(
                            (skipNavigation.ForeignKey, skipNavigation.IsOnDependent), out var firstExpansion)
                        // Value known to be non-null
                        && UnwrapEntityReference(firstExpansion)!.ForeignKeyExpansionMap.TryGetValue(
                            (skipNavigation.Inverse.ForeignKey, !skipNavigation.Inverse.IsOnDependent), out var secondExpansion))
                    {
                        // Value known to be non-null
                        nodeToAdd = UnwrapEntityReference(secondExpansion)!.IncludePaths;
                    }
                }

                if (nodeToAdd == null)
                {
                    nodeToAdd = new IncludeTreeNode(navigation.TargetEntityType, null, setLoaded);
                }

                this[navigation] = nodeToAdd;

                return this[navigation];
            }

            public IncludeTreeNode Snapshot(EntityReference? entityReference)
            {
                var result = new IncludeTreeNode(EntityType, entityReference, SetLoaded) { FilterExpression = FilterExpression };

                foreach (var kvp in this)
                {
                    result[kvp.Key] = kvp.Value.Snapshot(null);
                }

                return result;
            }

            public void Merge(IncludeTreeNode includeTreeNode)
            {
                // EntityReference is intentionally ignored
                FilterExpression = includeTreeNode.FilterExpression;
                foreach (var item in includeTreeNode)
                {
                    AddNavigation(item.Key, item.Value.SetLoaded).Merge(item.Value);
                }
            }

            public void AssignEntityReference(EntityReference entityReference)
                => _entityReference = entityReference;

            public void ApplyFilter(LambdaExpression filterExpression)
                => FilterExpression = filterExpression;

            public override bool Equals(object? obj)
                => obj != null
                    && (ReferenceEquals(this, obj)
                        || obj is IncludeTreeNode includeTreeNode
                        && Equals(includeTreeNode));

            private bool Equals(IncludeTreeNode includeTreeNode)
            {
                if (Count != includeTreeNode.Count)
                {
                    return false;
                }

                foreach (var kvp in this)
                {
                    if (!includeTreeNode.TryGetValue(kvp.Key, out var otherIncludeTreeNode)
                        || !kvp.Value.Equals(otherIncludeTreeNode))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
                => HashCode.Combine(base.GetHashCode(), EntityType);
        }

        /// <summary>
        ///     Stores information about the current queryable, its source, structure of projection, parameter type etc.
        ///     This is needed because once navigations are expanded we still remember these to avoid expanding again.
        /// </summary>
        private sealed class NavigationExpansionExpression : Expression, IPrintableExpression
        {
            private readonly List<(MethodInfo OrderingMethod, Expression KeySelector)> _pendingOrderings = new();

            private readonly string _parameterName;

            private NavigationTreeNode? _currentTree;

            public NavigationExpansionExpression(
                Expression source,
                NavigationTreeNode currentTree,
                Expression pendingSelector,
                string parameterName)
            {
                Source = source;
                _parameterName = parameterName;
                CurrentTree = currentTree;
                PendingSelector = pendingSelector;
            }

            public Expression Source { get; private set; }

            public ParameterExpression CurrentParameter
                // CurrentParameter would be non-null if CurrentTree is non-null
                => CurrentTree.CurrentParameter!;

            public NavigationTreeNode CurrentTree
            {
                // _currentTree is always non-null. Field is to override the setter to set parameter
                get => _currentTree!;
                private set
                {
                    _currentTree = value;
                    _currentTree.SetParameter(_parameterName);
                }
            }

            public Expression PendingSelector { get; private set; }
            public MethodInfo? CardinalityReducingGenericMethodInfo { get; private set; }

            public Type SourceElementType
                => CurrentParameter.Type;

            public IReadOnlyList<(MethodInfo OrderingMethod, Expression KeySelector)> PendingOrderings
                => _pendingOrderings;

            public void UpdateSource(Expression source)
                => Source = source;

            public void UpdateCurrentTree(NavigationTreeNode currentTree)
                => CurrentTree = currentTree;

            public void ApplySelector(Expression selector)
                => PendingSelector = selector;

            public void AddPendingOrdering(MethodInfo orderingMethod, Expression keySelector)
            {
                _pendingOrderings.Clear();
                _pendingOrderings.Add((orderingMethod, keySelector));
            }

            public void AppendPendingOrdering(MethodInfo orderingMethod, Expression keySelector)
                => _pendingOrderings.Add((orderingMethod, keySelector));

            public void ClearPendingOrderings()
                => _pendingOrderings.Clear();

            public void ConvertToSingleResult(MethodInfo genericMethod)
                => CardinalityReducingGenericMethodInfo = genericMethod;

            public override ExpressionType NodeType
                => ExpressionType.Extension;

            public override Type Type
                => CardinalityReducingGenericMethodInfo == null
                    ? typeof(IQueryable<>).MakeGenericType(PendingSelector.Type)
                    : PendingSelector.Type;

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                Check.NotNull(visitor, nameof(visitor));

                return this;
            }

            void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
            {
                Check.NotNull(expressionPrinter, nameof(expressionPrinter));

                expressionPrinter.AppendLine(nameof(NavigationExpansionExpression));
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Append("Source: ");
                    expressionPrinter.Visit(Source);
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append("PendingSelector: ");
                    expressionPrinter.Visit(Lambda(PendingSelector, CurrentParameter));
                    expressionPrinter.AppendLine();
                    if (CardinalityReducingGenericMethodInfo != null)
                    {
                        expressionPrinter.AppendLine("CardinalityReducingMethod: " + CardinalityReducingGenericMethodInfo.Name);
                    }
                }
            }
        }

        /// <summary>
        ///     A leaf node on navigation tree, representing projection structures of
        ///     <see cref="NavigationExpansionExpression" />. Contains <see cref="Value" />,
        ///     which can be <see cref="NewExpression" /> or <see cref="EntityReference" />.
        /// </summary>
        private sealed class NavigationTreeExpression : NavigationTreeNode, IPrintableExpression
        {
            public NavigationTreeExpression(Expression value)
                : base(null, null)
            {
                Value = value;
            }

            /// <summary>
            ///     Either <see cref="NewExpression" /> or <see cref="EntityReference" />.
            /// </summary>
            public Expression Value { get; private set; }

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                Check.NotNull(visitor, nameof(visitor));

                Value = visitor.Visit(Value);

                return this;
            }

            public override Type Type
                => Value.Type;

            void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
            {
                Check.NotNull(expressionPrinter, nameof(expressionPrinter));

                expressionPrinter.AppendLine(nameof(NavigationTreeExpression));
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Append("Value: ");
                    expressionPrinter.Visit(Value);
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append("Expression: ");
                    expressionPrinter.Visit(GetExpression());
                }
            }
        }

        /// <summary>
        ///     A node in navigation binary tree. A navigation tree is a structure of the current parameter, which
        ///     would be transparent identifier (hence it's a binary structure). This allows us to easily condense to
        ///     inner/outer member access.
        /// </summary>
        private class NavigationTreeNode : Expression
        {
            private NavigationTreeNode? _parent;

            public NavigationTreeNode(NavigationTreeNode? left, NavigationTreeNode? right)
            {
                Left = left;
                Right = right;
                if (left != null
                    && right != null)
                {
                    left._parent = this;
                    left.CurrentParameter = null;
                    right._parent = this;
                    right.CurrentParameter = null;
                }
            }

            public NavigationTreeNode? Left { get; }
            public NavigationTreeNode? Right { get; }
            public ParameterExpression? CurrentParameter { get; private set; }

            public void SetParameter(string parameterName)
                => CurrentParameter = Parameter(Type, parameterName);

            public override ExpressionType NodeType
                => ExpressionType.Extension;

            public override Type Type
                // Left/Right could be null for NavigationTreeExpression (derived type) but it overrides this property.
                => TransparentIdentifierFactory.Create(Left!.Type, Right!.Type);

            public Expression GetExpression()
            {
                if (_parent == null)
                {
                    // If parent is null and CurrentParameter is non-null & vice-versa
                    return CurrentParameter!;
                }

                var parentExperssion = _parent.GetExpression();
                return _parent.Left == this
                    ? MakeMemberAccess(parentExperssion, parentExperssion.Type.GetMember("Outer")[0])
                    : MakeMemberAccess(parentExperssion, parentExperssion.Type.GetMember("Inner")[0]);
            }
        }

        /// <summary>
        ///     Owned navigations are not expanded, since they map differently in different providers.
        ///     This remembers such references so that they can still be treated like navigations.
        /// </summary>
        private sealed class OwnedNavigationReference : Expression
        {
            public OwnedNavigationReference(Expression parent, INavigation navigation, EntityReference entityReference)
            {
                Parent = parent;
                Navigation = navigation;
                EntityReference = entityReference;
            }

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                Check.NotNull(visitor, nameof(visitor));

                Parent = visitor.Visit(Parent);

                return this;
            }

            public Expression Parent { get; private set; }
            public INavigation Navigation { get; }
            public EntityReference EntityReference { get; }

            public override Type Type
                => Navigation.ClrType;

            public override ExpressionType NodeType
                => ExpressionType.Extension;
        }
    }
}
