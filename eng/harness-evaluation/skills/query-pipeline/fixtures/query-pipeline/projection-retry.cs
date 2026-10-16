public class RelationalProjectionBindingExpressionVisitor : ExpressionVisitor
{
    private bool _indexBasedBinding;
    private bool _rootIsTransparentIdentifier;
    private readonly Stack<ProjectionMember> _projectionMembers = new();
    private readonly Dictionary<ProjectionMember, Expression> _projectionMapping = [];
    private SelectExpression _selectExpression = null!;

    public virtual Expression Translate(SelectExpression selectExpression, Expression expression)
    {
        _selectExpression = selectExpression;
        _indexBasedBinding = false;
        _rootIsTransparentIdentifier = IsTransparentIdentifierProjection(expression);
        _projectionMembers.Push(new ProjectionMember());

        var result = Visit(expression);
        if (result == QueryCompilationContext.NotTranslatedExpression)
        {
            _indexBasedBinding = true;
            result = Visit(expression);
        }

        _selectExpression = null!;
        _projectionMembers.Clear();
        return MatchTypes(result, expression.Type);
    }

    internal virtual Expression? TryTranslateToServerProjection(
        SelectExpression selectExpression,
        Expression expression)
    {
        _selectExpression = selectExpression;
        _indexBasedBinding = false;
        _projectionMembers.Push(new ProjectionMember());

        expression = new MarkerNullCheckSimplifyingExpressionVisitor().Visit(expression);
        var result = Visit(expression);
        if (result == QueryCompilationContext.NotTranslatedExpression)
        {
            result = null;
        }
        else
        {
            _selectExpression.ReplaceProjection(_projectionMapping);
            result = MatchTypes(result, expression.Type);
        }

        _selectExpression = null!;
        _projectionMapping.Clear();
        _projectionMembers.Clear();
        return result;
    }

    protected override Expression VisitNew(NewExpression newExpression)
    {
        var hasNullabilityMarker = _selectExpression.TryGetNonEntityNullabilityMarker(
            newExpression,
            out var markerBinding);
        var visited = newExpression.Update(newExpression.Arguments.Select(Visit));

        if (hasNullabilityMarker && _rootIsTransparentIdentifier)
        {
            var reboundMarker = BindNullabilityMarker(markerBinding!);
            _selectExpression.RemapNonEntityNullabilityMarker(newExpression, visited, reboundMarker);
        }

        return hasNullabilityMarker
            ? GateNonEntityOnNullabilityMarker(visited, markerBinding!, newExpression.Type)
            : visited;
    }

    private static bool IsTransparentIdentifierProjection(Expression expression)
        => expression is NewExpression newExpression
            && TransparentIdentifierFactory.IsTransparentIdentifierType(newExpression.Type);

    private sealed class MarkerNullCheckSimplifyingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node is { NodeType: ExpressionType.Equal or ExpressionType.NotEqual, Method: null }
                && ((IsNull(node.Left) && TryGetNullCheck(node.Right, out var nullCheck))
                    || (IsNull(node.Right) && TryGetNullCheck(node.Left, out nullCheck))))
            {
                nullCheck = Visit(nullCheck);
                return node.NodeType == ExpressionType.Equal
                    ? nullCheck
                    : Expression.Not(nullCheck);
            }

            return base.VisitBinary(node);
        }

        private static bool IsNull(Expression expression)
            => expression is ConstantExpression { Value: null }
                or DefaultExpression { Type.IsValueType: false };

        private static bool TryGetNullCheck(Expression expression, [NotNullWhen(true)] out Expression? nullCheck)
        {
            expression = expression.UnwrapTypeConversion(out _);
            if (expression is ConditionalExpression
                {
                    Test: var test,
                    IfTrue: var ifTrue,
                    IfFalse: NewExpression or MemberInitExpression
                }
                && IsNull(ifTrue)
                && IsMarkerNullCheck(test))
            {
                nullCheck = test;
                return true;
            }

            nullCheck = null;
            return false;
        }

        private static bool IsMarkerNullCheck(Expression expression)
        {
            expression = expression.UnwrapTypeConversion(out _);
            return expression is BinaryExpression
                {
                    NodeType: ExpressionType.Equal,
                    Method: null,
                    Left: var left,
                    Right: var right
                }
                && ((IsNull(left) && right.UnwrapTypeConversion(out _) is ProjectionBindingExpression)
                    || (IsNull(right) && left.UnwrapTypeConversion(out _) is ProjectionBindingExpression));
        }
    }

    private Expression GateNonEntityOnNullabilityMarker(
        Expression visited,
        Expression markerBinding,
        Type objectType)
    {
        if (_rootIsTransparentIdentifier)
        {
            return visited;
        }

        var boundMarker = BindNullabilityMarker(markerBinding);
        return Expression.Condition(
            Expression.Equal(boundMarker, Expression.Constant(null, boundMarker.Type)),
            Expression.Default(objectType),
            visited);
    }
}