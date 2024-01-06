// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public partial class NavigationExpandingExpressionVisitor
{
    private sealed class EntityReference : Expression, IPrintableExpression
    {
        public EntityReference(IEntityType entityType, EntityQueryRootExpression? entityQueryRootExpression)
        {
            EntityType = entityType;
            IncludePaths = new IncludeTreeNode(entityType, this, setLoaded: true);
            EntityQueryRootExpression = entityQueryRootExpression;
        }

        public IEntityType EntityType { get; }

        public Dictionary<(IForeignKey, bool), Expression> ForeignKeyExpansionMap { get; } = new();

        public bool IsOptional { get; private set; }
        public IncludeTreeNode IncludePaths { get; private set; }
        public IncludeTreeNode? LastIncludeTreeNode { get; private set; }
        public EntityQueryRootExpression? EntityQueryRootExpression { get; }

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public override Type Type
            => EntityType.ClrType;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        public EntityReference Snapshot()
        {
            var result = new EntityReference(EntityType, EntityQueryRootExpression) { IsOptional = IsOptional };
            result.IncludePaths = IncludePaths.Snapshot(result);

            return result;
        }

        public void SetLastInclude(IncludeTreeNode lastIncludeTree)
            => LastIncludeTreeNode = lastIncludeTree;

        public void MarkAsOptional()
            => IsOptional = true;

        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append($"{nameof(EntityReference)}: {EntityType.DisplayName()}");
            if (IsOptional)
            {
                expressionPrinter.Append("[Optional]");
            }

            if (IncludePaths.Count > 0)
            {
                expressionPrinter.AppendLine(" | IncludePaths: ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.AppendLine("Root");
                }

                PrintInclude(IncludePaths);
            }

            void PrintInclude(IncludeTreeNode currentNode)
            {
                if (currentNode.Count > 0)
                {
                    using (expressionPrinter.Indent())
                    {
                        foreach (var (navigationBase, includeTreeNode) in currentNode)
                        {
                            expressionPrinter.AppendLine(@"\-> " + navigationBase.Name);
                            PrintInclude(includeTreeNode);
                        }
                    }
                }
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

            nodeToAdd ??= new IncludeTreeNode(navigation.TargetEntityType, null, setLoaded);

            this[navigation] = nodeToAdd;

            return this[navigation];
        }

        public IncludeTreeNode Snapshot(EntityReference? entityReference)
        {
            var result = new IncludeTreeNode(EntityType, entityReference, SetLoaded) { FilterExpression = FilterExpression };

            foreach (var (navigationBase, includeTreeNode) in this)
            {
                result[navigationBase] = includeTreeNode.Snapshot(null);
            }

            return result;
        }

        public void Merge(IncludeTreeNode includeTreeNode)
        {
            // EntityReference is intentionally ignored
            FilterExpression = includeTreeNode.FilterExpression;
            foreach (var (navigationBase, value) in includeTreeNode)
            {
                AddNavigation(navigationBase, value.SetLoaded).Merge(value);
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

            foreach (var (navigationBase, value) in this)
            {
                if (!includeTreeNode.TryGetValue(navigationBase, out var otherIncludeTreeNode)
                    || !value.Equals(otherIncludeTreeNode))
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
        private readonly List<(MethodInfo OrderingMethod, Expression KeySelector)> _pendingOrderings = [];

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
        public List<Expression> CardinalityReducingMethodArguments { get; } = [];

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

        public void ConvertToSingleResult(MethodInfo genericMethod, params Expression[] arguments)
        {
            CardinalityReducingGenericMethodInfo = genericMethod;
            CardinalityReducingMethodArguments.AddRange(arguments);
        }

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public override Type Type
            => CardinalityReducingGenericMethodInfo == null
                ? typeof(IQueryable<>).MakeGenericType(PendingSelector.Type)
                : PendingSelector.Type;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
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

    private sealed class GroupByNavigationExpansionExpression : Expression, IPrintableExpression
    {
        public GroupByNavigationExpansionExpression(
            Expression source,
            ParameterExpression groupingParameter,
            NavigationTreeNode currentTree,
            Expression pendingSelector,
            string innerParameterName)
        {
            Source = source;
            CurrentParameter = groupingParameter;
            Type = source.Type;
            GroupingEnumerable = new NavigationExpansionExpression(
                Call(QueryableMethods.AsQueryable.MakeGenericMethod(CurrentParameter.Type.GetGenericArguments()[1]), CurrentParameter),
                currentTree,
                pendingSelector,
                innerParameterName);
        }

        public Expression Source { get; private set; }

        public ParameterExpression CurrentParameter { get; }

        public NavigationExpansionExpression GroupingEnumerable { get; }

        public Type SourceElementType
            => CurrentParameter.Type;

        public void UpdateSource(Expression source)
            => Source = source;

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine(nameof(GroupByNavigationExpansionExpression));
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("Source: ");
                expressionPrinter.Visit(Source);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("GroupingEnumerable: ");
                expressionPrinter.Visit(GroupingEnumerable);
                expressionPrinter.AppendLine();
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
            Value = visitor.Visit(Value);

            return this;
        }

        public override Type Type
            => Value.Type;

        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
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

            var parentExpression = _parent.GetExpression();
            return _parent.Left == this
                ? MakeMemberAccess(parentExpression, parentExpression.Type.GetMember("Outer")[0])
                : MakeMemberAccess(parentExpression, parentExpression.Type.GetMember("Inner")[0]);
        }
    }

    /// <summary>
    ///     Owned navigations are not expanded, since they map differently in different providers.
    ///     This remembers such references so that they can still be treated like navigations.
    /// </summary>
    private sealed class OwnedNavigationReference : Expression, IPrintableExpression
    {
        public OwnedNavigationReference(Expression parent, INavigation navigation, EntityReference entityReference)
        {
            Parent = parent;
            Navigation = navigation;
            EntityReference = entityReference;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
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

        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine(nameof(OwnedNavigationReference));
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("Parent: ");
                expressionPrinter.Visit(Parent);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("Navigation: " + Navigation.Name + " (OWNED)");
            }
        }
    }

    /// <summary>
    ///     Queryable properties are not expanded (similar to <see cref="OwnedNavigationReference" />.
    /// </summary>
    private sealed class PrimitiveCollectionReference : Expression, IPrintableExpression
    {
        public PrimitiveCollectionReference(Expression parent, IProperty property)
        {
            Parent = parent;
            Property = property;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Parent = visitor.Visit(Parent);

            return this;
        }

        public Expression Parent { get; private set; }
        public new IProperty Property { get; }

        public override Type Type
            => Property.ClrType;

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine(nameof(OwnedNavigationReference));
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("Parent: ");
                expressionPrinter.Visit(Parent);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("Property: " + Property.Name + " (QUERYABLE)");
            }
        }
    }
}
