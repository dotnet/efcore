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