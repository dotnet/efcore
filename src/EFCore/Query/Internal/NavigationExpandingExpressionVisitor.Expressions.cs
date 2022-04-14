// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public partial class NavigationExpandingExpressionVisitor
{
    private interface INode
    {
        public NavigationExpansionExpression? Owner { get; set; }
        public TransparentIdentifierNode? Parent { get; set; }
        public Type Type { get; }

        public void AttachOwner(NavigationExpansionExpression owner)
        {
            Owner = owner;
        }

        public Expression GetExpression()
        {
            if (Parent == null)
            {
                return Owner!.CurrentParameter;
            }

            var parentExpression = ((INode)Parent).GetExpression();
            return Parent.Outer == this
                ? Expression.MakeMemberAccess(parentExpression, parentExpression.Type.GetMember("Outer")[0])
                : Expression.MakeMemberAccess(parentExpression, parentExpression.Type.GetMember("Inner")[0]);
        }
    }

    private sealed class TransparentIdentifierNode : INode
    {
        public TransparentIdentifierNode(INode outer, INode inner)
        {
            Outer = outer;
            Inner = inner;
            outer.Parent = this;
            inner.Parent = this;
            Type = TransparentIdentifierFactory.Create(outer.Type, inner.Type);
        }

        public INode Outer { get; }
        public INode Inner { get; }
        public NavigationExpansionExpression? Owner { get; set; }

        public TransparentIdentifierNode? Parent { get; set; }
        public Type Type { get; }
    }

    private sealed class LeafNode : INode
    {
        public LeafNode(Type type)
        {
            Type = type;
            Parent = null;
        }

        public NavigationExpansionExpression? Owner { get; set; }

        public TransparentIdentifierNode? Parent { get; set; }

        public Type Type { get; }
    }

    private sealed class Element : Expression, IPrintableExpression
    {
        private readonly LeafNode _root;
        private readonly Type _type;
        private readonly List<MemberInfo> _memberChain;

        public Element(LeafNode root)
           : this(root, new(), root.Type)
        {
        }

        private Element(LeafNode root, List<MemberInfo> memberChain, Type type)
        {
            _root = root;
            _memberChain = memberChain;
            _type = type;
        }

        public Element AddMember(MemberInfo memberInfo)
        {
            var memberChain = _memberChain.ToList();
            memberChain.Add(memberInfo);

            return new Element(_root, memberChain, GetType(memberInfo));

            static Type GetType(MemberInfo member)
                => member switch
                {
                    PropertyInfo p => p.PropertyType,
                    FieldInfo f => f.FieldType,
                    _ => throw new InvalidOperationException()
                };
        }

        public Expression GetExpression()
        {
            var rootExpression = ((INode)_root).GetExpression();
            if (_memberChain.Count > 0)
            {
                for (var i = 0; i < _memberChain.Count; i++)
                {
                    rootExpression = MakeMemberAccess(rootExpression, _memberChain[i]);
                }
            }

            return rootExpression;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(((INode)_root).GetExpression());
            foreach (var memberInfo in _memberChain)
            {
                expressionPrinter.Append($".{memberInfo.Name}");
            }
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _type;
    }

    private class EntityExpression : Expression, IPrintableExpression
    {
        public EntityExpression(Element element, IEntityType entityType)
        {
            Element = element;
            EntityType = entityType;
            IncludePaths = new IncludeTreeNode(entityType, setLoaded: true);
        }

        public Dictionary<(IForeignKey, bool), Expression> ForeignKeyExpansionMap { get; } = new();
        public Element Element { get; }

        public IEntityType EntityType { get; }

        public IncludeTreeNode IncludePaths { get; private set; }
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Element.Type;

        public EntityExpression Clone(Element element)
        {
            return new EntityExpression(element, EntityType)
            {
                IncludePaths = IncludePaths
            };
        }

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Element);
            expressionPrinter.Append($"(EntityType: {EntityType.DisplayName()})");
        }
    }

    private sealed class IncludeTreeNode : Dictionary<INavigationBase, IncludeTreeNode>
    {
        public IncludeTreeNode(IEntityType entityType, bool setLoaded = true)
        {
            EntityType = entityType;
            SetLoaded = setLoaded;
        }

        public IEntityType EntityType { get; set; }
        public bool SetLoaded { get; set; }

        public IncludeTreeNode AddNavigation(INavigationBase navigation, bool setLoaded)
        {
            if (TryGetValue(navigation, out var existingValue))
            {
                existingValue.SetLoaded |= setLoaded;

                return existingValue;
            }

            var nodeToAdd = new IncludeTreeNode(navigation.TargetEntityType, setLoaded);
            this[navigation] = nodeToAdd;

            return this[navigation];
        }

        public void Merge(IncludeTreeNode includeTreeNode)
        {
            // FilterExpression = includeTreeNode.FilterExpression;
            foreach (var (navigationBase, value) in includeTreeNode)
            {
                AddNavigation(navigationBase, value.SetLoaded).Merge(value);
            }
        }
    }

    private sealed class NavigationExpansionExpression : Expression, IPrintableExpression
    {
        private readonly string _parameterName;
        private readonly LeafNode _selectorNode;
        private readonly List<(MethodInfo OrderingMethod, Expression KeySelector)> _pendingOrderings = new();

        public NavigationExpansionExpression(
            Expression source,
            LeafNode selectorNode,
            Expression selectorStructure,
            string parameterName)
        {
            Source = source;
            CurrentTree = selectorNode;
            _selectorNode = selectorNode;
            SelectorStructure = selectorStructure;
            _parameterName = parameterName;
            CurrentParameter = Parameter(selectorNode.Type, parameterName);
        }

        public Expression Source { get; private set; }
        public INode CurrentTree { get; private set; }
        public ParameterExpression CurrentParameter { get; private set; }

        public Type ElementType => CurrentParameter.Type;
        public Expression SelectorStructure { get; }
        public IReadOnlyList<(MethodInfo OrderingMethod, Expression KeySelector)> PendingOrderings
            => _pendingOrderings;

        public void AddPendingOrdering(MethodInfo orderingMethod, Expression keySelector)
        {
            _pendingOrderings.Clear();
            _pendingOrderings.Add((orderingMethod, keySelector));
        }
        public void AppendPendingOrdering(MethodInfo orderingMethod, Expression keySelector)
            => _pendingOrderings.Add((orderingMethod, keySelector));
        public void ClearPendingOrderings()
            => _pendingOrderings.Clear();

        public Expression GetSelector() => ((INode)_selectorNode).GetExpression();

        public MethodInfo? CardinalityReducingGenericMethodInfo { get; private set; }
        public void ConvertToSingleResult(MethodInfo genericMethod)
            => CardinalityReducingGenericMethodInfo = genericMethod;

        public void UpdateSource(Expression source)
        {
            Source = source;
        }

        public NavigationExpansionExpression UpdateSelector(
            Expression source,
            LeafNode selectorNode,
            Expression selectorStructure)
        {
            if (_pendingOrderings.Count != 0)
            {
                throw new InvalidFilterCriteriaException();
            }

            return new NavigationExpansionExpression(
                source, selectorNode, selectorStructure, _parameterName)
            {
                CardinalityReducingGenericMethodInfo = CardinalityReducingGenericMethodInfo
            };
        }

        public void UpdateCurrentTree(INode tree)
        {
            CurrentTree = tree;
            CurrentParameter = Parameter(tree.Type, _parameterName);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type =>
            CardinalityReducingGenericMethodInfo != null
            ? SelectorStructure.Type
            : Source.Type;

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine(nameof(NavigationExpansionExpression));
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("Source: ");
                expressionPrinter.Visit(Source);
                expressionPrinter.AppendLine();
                expressionPrinter.AppendLine($"CurrentTreeType: {CurrentTree.Type.ShortDisplayName()}");
                expressionPrinter.Append("SelectorStructure: ");
                expressionPrinter.Visit(Lambda(SelectorStructure, CurrentParameter));
                expressionPrinter.AppendLine();
                if (CardinalityReducingGenericMethodInfo != null)
                {
                    expressionPrinter.AppendLine("CardinalityReducingMethod: " + CardinalityReducingGenericMethodInfo.Name);
                }
            }
        }
    }
}
