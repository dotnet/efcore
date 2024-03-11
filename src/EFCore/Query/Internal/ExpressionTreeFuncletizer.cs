// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This visitor identifies subtrees in the query which can be evaluated client-side (i.e. no reference to server-side resources),
///     and evaluates those subtrees, integrating the result either as a constant (if the subtree contained no captured closure variables),
///     or as parameters.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class ExpressionTreeFuncletizer : ExpressionVisitor
{
    // The general algorithm here is the following.
    // 1. First, for each node type, visit that node's children and get their states (evaluatable, contains evaluatable, no evaluatable).
    // 2. Calculate the parent node's aggregate state from its children; a container node whose children are all evaluatable is itself
    //    evaluatable, etc.
    // 3. If the parent node is evaluatable (because all its children are), simply bubble that up - nothing more to do
    // 4. If the parent node isn't evaluatable but contains an evaluatable child, that child is an evaluatable root for its fragment.
    //    Evaluate it, making it either into a parameter (if it contains any captured variables), or into a constant (if not).
    // 5. If we're in path extraction mode (precompiled queries), build a path back up from the evaluatable roots to the query root; this
    //    is what later gets used to generate code to evaluate and extract those fragments as parameters. If we're in regular parameter
    //    parameter extraction (not precompilation), don't do this (not needed) and just return "not evaluatable".

    /// <summary>
    ///     Indicates whether we're calculating the paths to all parameterized evaluatable roots (precompilation mode), or doing regular,
    ///     non-precompiled parameter extraction.
    /// </summary>
    private bool _calculatingPath;

    /// <summary>
    ///     Indicates whether we should parameterize. Is false in compiled query mode, as well as when we're handling query filters from
    ///     NavigationExpandingExpressionVisitor.
    /// </summary>
    private bool _parameterize;

    /// <summary>
    ///     Indicates whether we're currently within a lambda. When not in a lambda, we evaluate evaluatables as constants even if they
    ///     don't contains a captured variable (Skip/Take case).
    /// </summary>
    private bool _inLambda;

    /// <summary>
    ///     A provider-facing extensibility hook to allow preventing certain expression nodes from being evaluated (typically specific
    ///     methods).
    /// </summary>
    private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;

    /// <summary>
    ///     <see cref="ParameterExpression" /> is generally considered as non-evaluatable, since it represents a lambda parameter and we
    ///     don't evaluate lambdas. The one exception is a Select operator over something evaluatable (e.g. a parameterized list) - this
    ///     does need to get evaluated. This list contains <see cref="ParameterExpression" /> instances for that case, to allow
    ///     evaluatability.
    /// </summary>
    private readonly HashSet<ParameterExpression> _evaluatableParameters = new();

    /// <summary>
    ///     A cache of tree fragments that have already been parameterized, along with their parameter. This allows us to reuse the same
    ///     query parameter twice when the same captured variable is referenced in the query.
    /// </summary>
    private readonly Dictionary<Expression, ParameterExpression> _parameterizedValues = new(ExpressionEqualityComparer.Instance);

    /// <summary>
    ///     Used only when evaluating arbitrary QueryRootExpressions (specifically SqlQueryRootExpression), to force any evaluatable nested
    ///     expressions to get evaluated as roots, since the query root itself is never evaluatable.
    /// </summary>
    private bool _evaluateRoot;

    /// <summary>
    ///     Enabled only when funcletization is invoked on query filters from within NavigationExpandingExpressionVisitor. Causes special
    ///     handling for DbContext when it's referenced from within the query filter (e.g. for the tenant ID).
    /// </summary>
    private readonly bool _generateContextAccessors;

    private IQueryProvider? _currentQueryProvider;
    private State _state;
    private IParameterValues _parameterValues = null!;

    private readonly IModel _model;
    private readonly ContextParameterReplacer _contextParameterReplacer;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

    private static readonly MethodInfo ReadOnlyCollectionIndexerGetter = typeof(ReadOnlyCollection<Expression>).GetProperties()
        .Single(p => p.GetIndexParameters() is { Length: 1 } indexParameters && indexParameters[0].ParameterType == typeof(int)).GetMethod!;

    private static readonly MethodInfo ReadOnlyMemberBindingCollectionIndexerGetter = typeof(ReadOnlyCollection<MemberBinding>)
        .GetProperties()
        .Single(p => p.GetIndexParameters() is { Length: 1 } indexParameters && indexParameters[0].ParameterType == typeof(int)).GetMethod!;

    private static readonly PropertyInfo MemberAssignmentExpressionProperty =
        typeof(MemberAssignment).GetProperty(nameof(MemberAssignment.Expression))!;

    private static readonly ArrayPool<State> StateArrayPool = ArrayPool<State>.Shared;

    private const string QueryFilterPrefix = "ef_filter";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ExpressionTreeFuncletizer(
        IModel model,
        IEvaluatableExpressionFilter evaluatableExpressionFilter,
        Type contextType,
        bool generateContextAccessors,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        _model = model;
        _evaluatableExpressionFilter = evaluatableExpressionFilter;
        _generateContextAccessors = generateContextAccessors;
        _contextParameterReplacer = _generateContextAccessors
            ? new ContextParameterReplacer(contextType)
            : null!;
        _logger = logger;
    }

    /// <summary>
    /// Processes an expression tree, extracting parameters and evaluating evaluatable fragments as part of the pass.
    /// Used for regular query execution (neither compiled nor pre-compiled).
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual Expression ExtractParameters(
        Expression expression,
        IParameterValues parameterValues,
        bool parameterize,
        bool clearParameterizedValues)
    {
        Reset(clearParameterizedValues);
        _parameterValues = parameterValues;
        _parameterize = parameterize;
        _calculatingPath = false;

        var root = Visit(expression, out var state);

        Check.DebugAssert(!state.ContainsEvaluatable, "In parameter extraction mode, end state should not contain evaluatable");

        // If the top-most node in the tree is evaluatable, evaluate it.
        if (state.IsEvaluatable)
        {
            root = ProcessEvaluatableRoot(root, ref state);
        }

        return root;
    }

    /// <summary>
    ///     Processes an expression tree, locates references to captured variables and returns information on how to extract them from
    ///     expression trees with the same shape. Used to generate C# code for query precompilation.
    /// </summary>
    /// <returns>A tree representing the path to each evaluatable root node in the tree.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual PathNode? CalculatePathsToEvaluatableRoots(Expression expression)
    {
        Reset();
        _calculatingPath = true;
        _parameterize = true;

        // In precompilation mode we don't actually extract parameter values; but we do need to generate the parameter names, using the
        // same logic (and via the same code) used in parameter extraction, and that logic requires _parameterValues.
        _parameterValues = new DummyParameterValues();

        _ = Visit(expression, out var state);

        return state.Path;
    }

    private void Reset(bool clearParameterizedValues = true)
    {
        _inLambda = false;
        _currentQueryProvider = null;
        _evaluateRoot = false;
        _evaluatableParameters.Clear();

        if (clearParameterizedValues)
        {
            _parameterizedValues.Clear();
        }
    }

    [return: NotNullIfNotNull("expression")]
    private Expression? Visit(Expression? expression, out State state)
    {
        _state = default;
        var result = base.Visit(expression);
        state = _state;
        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
    {
        _state = default;

        if (_evaluateRoot)
        {
            // This path is only called from VisitExtension for query roots, as a way of evaluating expressions inside query roots
            // (i.e. SqlQueryRootExpression.Arguments).
            _evaluateRoot = false;
            var result = base.Visit(expression);
            _evaluateRoot = true;

            if (_state.IsEvaluatable)
            {
                result = ProcessEvaluatableRoot(result, ref _state);
                // TODO: Test this scenario in path calculation mode (probably need to handle children path?)
            }

            return result;
        }

        return base.Visit(expression);
    }

    #region Visitation implementations

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binary)
    {
        var left = Visit(binary.Left, out var leftState);

        // Perform short-circuiting checks to avoid evaluating the right side if not necessary
        object? leftValue = null;
        if (leftState.IsEvaluatable)
        {
            switch (binary.NodeType)
            {
                case ExpressionType.Coalesce:
                    leftValue = Evaluate(left);

                    switch (leftValue)
                    {
                        case null:
                            return Visit(binary.Right, out _state);
                        case bool b:
                            _state = leftState with { StateType = StateType.EvaluatableWithoutCapturedVariable };
                            return Constant(b);
                        default:
                            return left;
                    }

                case ExpressionType.OrElse or ExpressionType.AndAlso when Evaluate(left) is bool leftBoolValue:
                {
                    left = Constant(leftBoolValue);
                    leftState = leftState with { StateType = StateType.EvaluatableWithoutCapturedVariable };

                    if (leftBoolValue && binary.NodeType is ExpressionType.OrElse
                        || !leftBoolValue && binary.NodeType is ExpressionType.AndAlso)
                    {
                        _state = leftState;
                        return left;
                    }

                    binary = binary.Update(left, binary.Conversion, binary.Right);
                    break;
                }
            }
        }

        var right = Visit(binary.Right, out var rightState);

        if (binary.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse)
        {
            if (leftState.IsEvaluatable && leftValue is bool leftBoolValue)
            {
                switch ((leftConstant: leftBoolValue, binary.NodeType))
                {
                    case (true, ExpressionType.AndAlso) or (false, ExpressionType.OrElse):
                        _state = rightState;
                        return right;
                    case (true, ExpressionType.OrElse) or (false, ExpressionType.AndAlso):
                        throw new UnreachableException(); // Already handled above before visiting the right side
                }
            }

            if (rightState.IsEvaluatable && Evaluate(right) is bool rightBoolValue)
            {
                switch ((binary.NodeType, rightConstant: rightBoolValue))
                {
                    case (ExpressionType.AndAlso, true) or (ExpressionType.OrElse, false):
                        _state = leftState;
                        return left;
                    case (ExpressionType.OrElse, true) or (ExpressionType.AndAlso, false):
                        _state = rightState with { StateType = StateType.EvaluatableWithoutCapturedVariable };
                        return Constant(rightBoolValue);
                }
            }
        }

        // We're done with simplification/short-circuiting checks specific to BinaryExpression.
        var state = CombineStateTypes(leftState.StateType, rightState.StateType);

        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
                if (IsGenerallyEvaluatable(binary))
                {
                    _state = State.CreateEvaluatable(typeof(BinaryExpression), state is StateType.EvaluatableWithCapturedVariable);
                    break;
                }

                goto case StateType.ContainsEvaluatable;

            case StateType.ContainsEvaluatable:
                if (leftState.IsEvaluatable)
                {
                    left = ProcessEvaluatableRoot(left, ref leftState);
                }

                if (rightState.IsEvaluatable)
                {
                    right = ProcessEvaluatableRoot(right, ref rightState);
                }

                List<PathNode>? children = null;

                if (_calculatingPath)
                {
                    if (leftState.ContainsEvaluatable)
                    {
                        children =
                        [
                            leftState.Path! with { PathFromParent = static e => Property(e, nameof(BinaryExpression.Left)) }
                        ];
                    }

                    if (rightState.ContainsEvaluatable)
                    {
                        children ??= new();
                        children.Add(rightState.Path! with { PathFromParent = static e => Property(e, nameof(BinaryExpression.Right)) });
                    }
                }

                _state = children is null
                    ? State.NoEvaluatability
                    : State.CreateContainsEvaluatable(typeof(BinaryExpression), children);
                break;

            default:
                throw new UnreachableException();
        }

        return binary.Update(left, binary.Conversion, right);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression conditional)
    {
        var test = Visit(conditional.Test, out var testState);

        // If the test evaluates, simplify the conditional away by bubbling up the leg that remains
        if (testState.IsEvaluatable && Evaluate(conditional.Test) is bool testBoolValue)
        {
            return testBoolValue
                ? Visit(conditional.IfTrue, out _state)
                : Visit(conditional.IfFalse, out _state);
        }

        var ifTrue = Visit(conditional.IfTrue, out var ifTrueState);
        var ifFalse = Visit(conditional.IfFalse, out var ifFalseState);

        var state = CombineStateTypes(testState.StateType, CombineStateTypes(ifTrueState.StateType, ifFalseState.StateType));

        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            // If all three children are evaluatable, so is this conditional expression; simply bubble up, we're part of an evaluatable
            // fragment that will get evaluated somewhere above.
            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
                if (IsGenerallyEvaluatable(conditional))
                {
                    _state = State.CreateEvaluatable(typeof(ConditionalExpression), state is StateType.EvaluatableWithCapturedVariable);
                    break;
                }

                goto case StateType.ContainsEvaluatable;

            case StateType.ContainsEvaluatable:
                // The case where the test is evaluatable has been handled above
                if (ifTrueState.IsEvaluatable)
                {
                    ifTrue = ProcessEvaluatableRoot(ifTrue, ref ifTrueState);
                }

                if (ifFalseState.IsEvaluatable)
                {
                    ifFalse = ProcessEvaluatableRoot(ifFalse, ref ifFalseState);
                }

                List<PathNode>? children = null;

                if (_calculatingPath)
                {
                    if (testState.ContainsEvaluatable)
                    {
                        children ??= new();
                        children.Add(
                            testState.Path! with { PathFromParent = static e => Property(e, nameof(ConditionalExpression.Test)) });
                    }

                    if (ifTrueState.ContainsEvaluatable)
                    {
                        children ??= new();
                        children.Add(
                            ifTrueState.Path! with { PathFromParent = static e => Property(e, nameof(ConditionalExpression.IfTrue)) });
                    }

                    if (ifFalseState.ContainsEvaluatable)
                    {
                        children ??= new();
                        children.Add(
                            ifFalseState.Path! with { PathFromParent = static e => Property(e, nameof(ConditionalExpression.IfFalse)) });
                    }
                }

                _state = children is null
                    ? State.NoEvaluatability
                    : State.CreateContainsEvaluatable(typeof(ConditionalExpression), children);
                break;

            default:
                throw new UnreachableException();
        }

        return conditional.Update(test, ifTrue, ifFalse);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConstant(ConstantExpression constant)
    {
        // Whether this constant represents a captured variable determines whether we'll evaluate it as a parameter (if yes) or as a
        // constant (if no).
        var isCapturedVariable =
            // This identifies compiler-generated closure types which contain captured variables.
            (constant.Type.Attributes.HasFlag(TypeAttributes.NestedPrivate)
                && Attribute.IsDefined(constant.Type, typeof(CompilerGeneratedAttribute), inherit: true))
            // The following is for supporting the Find method (we should look into this and possibly clean it up).
            || constant.Type == typeof(ValueBuffer);

        _state = constant.Value is IQueryable
            ? State.NoEvaluatability
            : State.CreateEvaluatable(typeof(ConstantExpression), isCapturedVariable);

        return constant;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitDefault(DefaultExpression node)
    {
        _state = State.CreateEvaluatable(typeof(DefaultExpression), containsCapturedVariable: false);
        return node;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extension)
    {
        if (extension is QueryRootExpression queryRoot)
        {
            var queryProvider = queryRoot.QueryProvider;
            if (_currentQueryProvider == null)
            {
                _currentQueryProvider = queryProvider;
            }
            else if (!ReferenceEquals(queryProvider, _currentQueryProvider))
            {
                throw new InvalidOperationException(CoreStrings.ErrorInvalidQueryable);
            }

            // Visit after detaching query provider since custom query roots can have additional components
            extension = queryRoot.DetachQueryProvider();

            // The following is somewhat hacky. We're going to visit the query root's children via VisitChildren - this is primarily for
            // FromSqlQueryRootExpression. Since the query root itself is never evaluatable, its children should all be handled as
            // evaluatable roots - we set _evaluateRoot and do that in Visit.
            // In addition, FromSqlQueryRootExpression's Arguments need to be a parameter rather than constant, so we set _inLambda to
            // make that happen (quite hacky, but was done this way in the old ParameterExtractingEV as well). Think about a better way.
            _evaluateRoot = true;
            var parentInLambda = _inLambda;
            _inLambda = false;
            var visitedExtension = base.VisitExtension(extension);
            _evaluateRoot = false;
            _inLambda = parentInLambda;
            _state = State.NoEvaluatability;
            return visitedExtension;
        }

        return base.VisitExtension(extension);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitInvocation(InvocationExpression invocation)
    {
        var expression = Visit(invocation.Expression, out var expressionState);
        var state = expressionState.StateType;
        var arguments = Visit(invocation.Arguments, ref state, out var argumentStates);

        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
                if (IsGenerallyEvaluatable(invocation))
                {
                    _state = State.CreateEvaluatable(typeof(InvocationExpression), state is StateType.EvaluatableWithCapturedVariable);
                    break;
                }

                goto case StateType.ContainsEvaluatable;

            case StateType.ContainsEvaluatable:
                List<PathNode>? children = null;

                if (expressionState.IsEvaluatable)
                {
                    expression = ProcessEvaluatableRoot(expression, ref expressionState);
                }

                if (expressionState.ContainsEvaluatable && _calculatingPath)
                {
                    children =
                    [
                        expressionState.Path! with { PathFromParent = static e => Property(e, nameof(InvocationExpression.Expression)) }
                    ];
                }

                arguments = EvaluateList(
                    ((IReadOnlyList<Expression>?)arguments) ?? invocation.Arguments,
                    argumentStates,
                    ref children,
                    static i => e =>
                        Call(
                            Property(e, nameof(InvocationExpression.Arguments)),
                            ReadOnlyCollectionIndexerGetter,
                            arguments: [Constant(i)]));

                _state = children is null
                    ? State.NoEvaluatability
                    : State.CreateContainsEvaluatable(typeof(InvocationExpression), children);
                break;

            default:
                throw new UnreachableException();
        }

        StateArrayPool.Return(argumentStates);
        return invocation.Update(expression, ((IReadOnlyList<Expression>?)arguments) ?? invocation.Arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitIndex(IndexExpression index)
    {
        var @object = Visit(index.Object, out var objectState);
        var state = objectState.StateType;
        var arguments = Visit(index.Arguments, ref state, out var argumentStates);

        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
                if (IsGenerallyEvaluatable(index))
                {
                    _state = State.CreateEvaluatable(typeof(IndexExpression), state is StateType.EvaluatableWithCapturedVariable);
                    break;
                }

                goto case StateType.ContainsEvaluatable;

            case StateType.ContainsEvaluatable:
                List<PathNode>? children = null;

                if (objectState.IsEvaluatable)
                {
                    @object = ProcessEvaluatableRoot(@object, ref objectState);
                }

                if (objectState.ContainsEvaluatable && _calculatingPath)
                {
                    children = [objectState.Path! with { PathFromParent = static e => Property(e, nameof(IndexExpression.Object)) }];
                }

                arguments = EvaluateList(
                    ((IReadOnlyList<Expression>?)arguments) ?? index.Arguments,
                    argumentStates,
                    ref children,
                    static i => e =>
                        Call(
                            Property(e, nameof(IndexExpression.Arguments)),
                            ReadOnlyCollectionIndexerGetter,
                            arguments: [Constant(i)]));

                _state = children is null
                    ? State.NoEvaluatability
                    : State.CreateContainsEvaluatable(typeof(IndexExpression), children);
                break;

            default:
                throw new UnreachableException();
        }

        StateArrayPool.Return(argumentStates);

        // TODO: https://github.com/dotnet/runtime/issues/96626
        return index.Update(@object!, ((IReadOnlyList<Expression>?)arguments) ?? index.Arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLambda<T>(Expression<T> lambda)
    {
        var oldInLambda = _inLambda;
        _inLambda = true;

        var body = Visit(lambda.Body, out _state);
        lambda = lambda.Update(body, lambda.Parameters);

        if (_state.StateType is StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable)
        {
            // The lambda body is evaluatable. If all lambda parameters are also in the _allowedParameters set (this happens for
            // Select() over an evaluatable source, see VisitMethodCall()), then the whole lambda is evaluatable. Otherwise, evaluate
            // the body.
            if (lambda.Parameters.All(parameter => _evaluatableParameters.Contains(parameter)))
            {
                _state = State.CreateEvaluatable(typeof(LambdaExpression), _state.ContainsCapturedVariable);
                return lambda;
            }

            lambda = lambda.Update(ProcessEvaluatableRoot(lambda.Body, ref _state), lambda.Parameters);
        }

        if (_state.ContainsEvaluatable)
        {
            _state = State.CreateContainsEvaluatable(
                typeof(LambdaExpression),
                [_state.Path! with { PathFromParent = static e => Property(e, nameof(Expression<T>.Body)) }]);
        }

        _inLambda = oldInLambda;

        return lambda;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMember(MemberExpression member)
    {
        // Static member access - notably required for EF.Functions, but also for various translations (DateTime.Now).
        // Note that this is treated as a captured variable (so will be parameterized), unless the captured variable is init-only.
        if (member.Expression is null)
        {
            _state = IsGenerallyEvaluatable(member)
                ? State.CreateEvaluatable(
                    typeof(MemberExpression),
                    containsCapturedVariable: member.Member is not FieldInfo { IsInitOnly: true })
                : State.NoEvaluatability;
            return member;
        }

        var expression = Visit(member.Expression, out _state);

        if (_state.IsEvaluatable)
        {
            // If the query contains a captured variable that's a nested IQueryable, inline it into the main query.
            // Otherwise, evaluation of a terminating operator up the call chain will cause us to execute the query and do another
            // roundtrip.
            // Note that we only do this when the MemberExpression is typed as IQueryable/IOrderedQueryable; this notably excludes
            // DbSet captured variables integrated directly into the query, as that also evaluates e.g. context.Order in
            // context.Order.FromSqlInterpolated(), which fails.
            if (member.Type.IsConstructedGenericType
                && member.Type.GetGenericTypeDefinition() is var genericTypeDefinition
                && (genericTypeDefinition == typeof(IQueryable<>) || genericTypeDefinition == typeof(IOrderedQueryable<>))
                && Evaluate(member) is IQueryable queryable)
            {
                return Visit(queryable.Expression);
            }

            if (IsGenerallyEvaluatable(member))
            {
                // Note that any evaluatable MemberExpression is treated as a captured variable.
                _state = State.CreateEvaluatable(typeof(MemberExpression), containsCapturedVariable: true);
                return member.Update(expression);
            }

            expression = ProcessEvaluatableRoot(expression, ref _state);
        }

        if (_state.ContainsEvaluatable && _calculatingPath)
        {
            _state = State.CreateContainsEvaluatable(
                typeof(MemberExpression),
                [_state.Path! with { PathFromParent = static e => Property(e, nameof(MemberExpression.Expression)) }]);
        }

        return member.Update(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCall)
    {
        var method = methodCall.Method;

        // Handle some special, well-known functions
        // If this is a call to EF.Constant(), or EF.Parameter(), then examine the operand; it it's isn't evaluatable (i.e. contains a
        // reference to a database table), throw immediately. Otherwise, evaluate the operand (either as a constant or as a parameter) and
        // return that.
        if (method.DeclaringType == typeof(EF))
        {
            switch (method.Name)
            {
                case nameof(EF.Constant):
                {
                    if (_calculatingPath)
                    {
                        throw new InvalidOperationException("EF.Constant is not supported when using precompiled queries");
                    }

                    var argument = Visit(methodCall.Arguments[0], out var argumentState);

                    if (!argumentState.IsEvaluatable)
                    {
                        throw new InvalidOperationException(CoreStrings.EFConstantWithNonEvaluatableArgument);
                    }

                    argumentState = argumentState with
                    {
                        StateType = StateType.EvaluatableWithoutCapturedVariable, ForceConstantization = true
                    };
                    var evaluatedArgument = ProcessEvaluatableRoot(argument, ref argumentState);
                    _state = argumentState;
                    return evaluatedArgument;
                }

                case nameof(EF.Parameter):
                {
                    var argument = Visit(methodCall.Arguments[0], out var argumentState);

                    if (!argumentState.IsEvaluatable)
                    {
                        throw new InvalidOperationException(CoreStrings.EFParameterWithNonEvaluatableArgument);
                    }

                    argumentState = argumentState with { StateType = StateType.EvaluatableWithCapturedVariable };
                    var evaluatedArgument = ProcessEvaluatableRoot(argument, ref argumentState);
                    _state = argumentState;
                    return evaluatedArgument;
                }
            }
        }

        // Regular/arbitrary method handling from here on

        // First, visit the object and all arguments, saving states as well
        var @object = Visit(methodCall.Object, out var objectState);
        var state = objectState.StateType;
        var arguments = Visit(methodCall.Arguments, ref state, out var argumentStates);

        // The following identifies Select(), and its lambda parameters in a special list which allows us to evaluate them.
        if (method.DeclaringType == typeof(Enumerable)
            && method.Name == nameof(Enumerable.Select)
            && argumentStates[0].IsEvaluatable
            && methodCall.Arguments[1] is LambdaExpression lambda)
        {
            foreach (var parameter in lambda.Parameters)
            {
                _evaluatableParameters.Add(parameter);
            }

            // Revisit with the updated _evaluatableParameters.
            state = objectState.StateType;
            arguments = Visit(methodCall.Arguments, ref state, out argumentStates);
        }

        // We've visited everything and know all the states.
        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
                if (IsGenerallyEvaluatable(methodCall))
                {
                    _state = State.CreateEvaluatable(typeof(MethodCallExpression), state is StateType.EvaluatableWithCapturedVariable);
                    break;
                }

                goto case StateType.ContainsEvaluatable;

            case StateType.ContainsEvaluatable:
                List<PathNode>? children = null;

                if (objectState.IsEvaluatable)
                {
                    @object = ProcessEvaluatableRoot(@object, ref objectState);
                }

                if (objectState.ContainsEvaluatable && _calculatingPath)
                {
                    children = [objectState.Path! with { PathFromParent = static e => Property(e, nameof(MethodCallExpression.Object)) }];
                }

                // To support [NotParameterized] and indexer method arguments - which force evaluation as constant - go over the parameters
                // and modify the states as needed
                ParameterInfo[]? parameterInfos = null;
                for (var i = 0; i < methodCall.Arguments.Count; i++)
                {
                    var argumentState = argumentStates[i];

                    if (argumentState.IsEvaluatable)
                    {
                        parameterInfos ??= methodCall.Method.GetParameters();
                        if (parameterInfos[i].GetCustomAttribute<NotParameterizedAttribute>() is not null
                            || _model.IsIndexerMethod(methodCall.Method))
                        {
                            argumentStates[i] = argumentState with
                            {
                                StateType = StateType.EvaluatableWithoutCapturedVariable, ForceConstantization = true
                            };
                        }
                    }
                }

                arguments = EvaluateList(
                    ((IReadOnlyList<Expression>?)arguments) ?? methodCall.Arguments,
                    argumentStates,
                    ref children,
                    static i => e =>
                        Call(
                            Property(e, nameof(MethodCallExpression.Arguments)),
                            ReadOnlyCollectionIndexerGetter,
                            arguments: [Constant(i)]));

                _state = children is null
                    ? State.NoEvaluatability
                    : State.CreateContainsEvaluatable(typeof(MethodCallExpression), children);
                break;

            default:
                throw new UnreachableException();
        }

        return methodCall.Update(@object, ((IReadOnlyList<Expression>?)arguments) ?? methodCall.Arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression newArray)
    {
        StateType state = default;
        var expressions = Visit(newArray.Expressions, ref state, out var expressionStates, poolExpressionStates: false);

        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
            {
                if (IsGenerallyEvaluatable(newArray))
                {
                    // Avoid allocating for the notEvaluatableAsRootHandler closure below unless we actually end up in the evaluatable case
                    var (newArray2, expressions2, expressionStates2) = (newArray, expressions, expressionStates);
                    _state = State.CreateEvaluatable(
                        typeof(NewExpression),
                        state is StateType.EvaluatableWithCapturedVariable,
                        // See note below on EvaluateChildren
                        notEvaluatableAsRootHandler: () => EvaluateChildren(newArray2, expressions2, expressionStates2));
                    break;
                }

                goto case StateType.ContainsEvaluatable;
            }

            case StateType.ContainsEvaluatable:
                return EvaluateChildren(newArray, expressions, expressionStates);

            default:
                throw new UnreachableException();
        }

        return newArray.Update(((IReadOnlyList<Expression>?)expressions) ?? newArray.Expressions);

        // We don't parameterize NewArrayExpression when its an evaluatable root, since we want to allow translating new[] { x, y } to
        // e.g. IN (x, y) rather than parameterizing the whole thing. But bubble up the evaluatable state so it may get evaluated at a
        // higher level.
        // To support that, when the NewArrayExpression is evaluatable, we include a nonEvaluatableAsRootHandler lambda in the returned
        // state, which gets invoked up the stack, calling this method. This evaluates the NewArrayExpression's children, but not the
        // NewArrayExpression.
        NewArrayExpression EvaluateChildren(NewArrayExpression newArray, Expression[]? expressions, State[] expressionStates)
        {
            List<PathNode>? children = null;

            expressions = EvaluateList(
                ((IReadOnlyList<Expression>?)expressions) ?? newArray.Expressions,
                expressionStates,
                ref children,
                i => e => Call(
                    Property(e, nameof(NewArrayExpression.Expressions)),
                    ReadOnlyCollectionIndexerGetter,
                    arguments: [Constant(i)]));

            _state = children is null
                ? State.NoEvaluatability
                : State.CreateContainsEvaluatable(typeof(NewArrayExpression), children);

            return newArray.Update(((IReadOnlyList<Expression>?)expressions) ?? newArray.Expressions);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression @new)
    {
        StateType state = default;
        var arguments = Visit(@new.Arguments, ref state, out var argumentStates, poolExpressionStates: false);

        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
            {
                if (IsGenerallyEvaluatable(@new))
                {
                    // Avoid allocating for the notEvaluatableAsRootHandler closure below unless we actually end up in the evaluatable case
                    var (new2, arguments2, argumentStates2) = (@new, arguments, argumentStates);
                    _state = State.CreateEvaluatable(
                        typeof(NewExpression),
                        state is StateType.EvaluatableWithCapturedVariable,
                        // See note below on EvaluateChildren
                        notEvaluatableAsRootHandler: () => EvaluateChildren(new2, arguments2, argumentStates2));
                    break;
                }

                goto case StateType.ContainsEvaluatable;
            }

            case StateType.ContainsEvaluatable:
                return EvaluateChildren(@new, arguments, argumentStates);

            default:
                throw new UnreachableException();
        }

        return @new.Update(((IReadOnlyList<Expression>?)arguments) ?? @new.Arguments);

        // Although we allow NewExpression to be evaluated within larger tree fragments, we don't constantize them when they're the
        // evaluatable root, since that would embed arbitrary user type instances in our shaper.
        // To support that, when the NewExpression is evaluatable, we include a nonEvaluatableAsRootHandler lambda in the returned state,
        // which gets invoked up the stack, calling this method. This evaluates the NewExpression's children, but not the NewExpression.
        NewExpression EvaluateChildren(NewExpression @new, Expression[]? arguments, State[] argumentStates)
        {
            List<PathNode>? children = null;

            arguments = EvaluateList(
                ((IReadOnlyList<Expression>?)arguments) ?? @new.Arguments,
                argumentStates,
                ref children,
                i => e => Call(
                    Property(e, nameof(NewExpression.Arguments)),
                    ReadOnlyCollectionIndexerGetter,
                    arguments: [Constant(i)]));

            _state = children is null
                ? State.NoEvaluatability
                : State.CreateContainsEvaluatable(typeof(NewExpression), children);

            return @new.Update(((IReadOnlyList<Expression>?)arguments) ?? @new.Arguments);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitParameter(ParameterExpression parameterExpression)
    {
        // ParameterExpressions are lambda parameters, which we cannot evaluate.
        // However, _allowedParameters is a mechanism to allow evaluating Select(), see VisitMethodCall.
        _state = _evaluatableParameters.Contains(parameterExpression)
            ? State.CreateEvaluatable(typeof(ParameterExpression), containsCapturedVariable: false)
            : State.NoEvaluatability;

        return parameterExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinary)
    {
        var expression = Visit(typeBinary.Expression, out _state);

        if (_state.IsEvaluatable)
        {
            if (IsGenerallyEvaluatable(typeBinary))
            {
                _state = State.CreateEvaluatable(typeof(TypeBinaryExpression), _state.ContainsCapturedVariable);
                return typeBinary.Update(expression);
            }

            expression = ProcessEvaluatableRoot(expression, ref _state);
        }

        if (_state.ContainsEvaluatable && _calculatingPath)
        {
            _state = State.CreateContainsEvaluatable(
                typeof(TypeBinaryExpression),
                [_state.Path! with { PathFromParent = static e => Property(e, nameof(TypeBinaryExpression.Expression)) }]);
        }

        return typeBinary.Update(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMemberInit(MemberInitExpression memberInit)
    {
        var @new = (NewExpression)Visit(memberInit.NewExpression, out var newState);
        var state = newState.StateType;
        var bindings = Visit(memberInit.Bindings, VisitMemberBinding, ref state, out var bindingStates, poolExpressionStates: false);

        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
            {
                if (IsGenerallyEvaluatable(memberInit))
                {
                    // Avoid allocating for the notEvaluatableAsRootHandler closure below unless we actually end up in the evaluatable case
                    var (memberInit2, new2, newState2, bindings2, bindingStates2) = (memberInit, @new, newState, bindings, bindingStates);
                    _state = State.CreateEvaluatable(
                        typeof(InvocationExpression),
                        state is StateType.EvaluatableWithCapturedVariable,
                        notEvaluatableAsRootHandler: () => EvaluateChildren(memberInit2, new2, newState2, bindings2, bindingStates2));
                    break;
                }

                goto case StateType.ContainsEvaluatable;
            }

            case StateType.ContainsEvaluatable:
                return EvaluateChildren(memberInit, @new, newState, bindings, bindingStates);

            default:
                throw new UnreachableException();
        }

        return memberInit.Update(@new, ((IReadOnlyList<MemberBinding>?)bindings) ?? memberInit.Bindings);

        // Although we allow MemberInitExpression to be evaluated within larger tree fragments, we don't constantize them when they're the
        // evaluatable root, since that would embed arbitrary user type instances in our shaper.
        // To support that, when the MemberInitExpression is evaluatable, we include a nonEvaluatableAsRootHandler lambda in the returned
        // state, which gets invoked up the stack, calling this method. This evaluates the MemberInitExpression's children, but not the
        // MemberInitExpression.
        MemberInitExpression EvaluateChildren(
            MemberInitExpression memberInit,
            NewExpression @new,
            State newState,
            MemberBinding[]? bindings,
            State[] bindingStates)
        {
            // If the NewExpression is evaluatable but one of the bindings isn't, we can't evaluate only the NewExpression
            // (MemberInitExpression requires a NewExpression and doesn't accept ParameterException). However, we may still need to
            // evaluate constructor arguments in the NewExpression.
            if (newState.IsEvaluatable)
            {
                @new = (NewExpression)newState.NotEvaluatableAsRootHandler!();
            }

            List<PathNode>? children = null;

            if (newState.ContainsEvaluatable && _calculatingPath)
            {
                children =
                [
                    newState.Path! with { PathFromParent = static e => Property(e, nameof(MemberInitExpression.NewExpression)) }
                ];
            }

            for (var i = 0; i < memberInit.Bindings.Count; i++)
            {
                var bindingState = bindingStates[i];

                if (bindingState.IsEvaluatable)
                {
                    bindings ??= memberInit.Bindings.ToArray();
                    var binding = (MemberAssignment)bindings[i];
                    bindings[i] = binding.Update(ProcessEvaluatableRoot(binding.Expression, ref bindingState));
                    bindingStates[i] = bindingState;
                }

                if (bindingState.ContainsEvaluatable && _calculatingPath)
                {
                    children ??= [];
                    var index = i; // i gets mutated so make a copy for capturing below
                    children.Add(
                        bindingState.Path! with
                        {
                            PathFromParent = e =>
                                Property(
                                    Convert(
                                        Call(
                                            Property(e, nameof(MemberInitExpression.Bindings)),
                                            ReadOnlyMemberBindingCollectionIndexerGetter,
                                            arguments: [Constant(index)]), typeof(MemberAssignment)),
                                    MemberAssignmentExpressionProperty)
                        });
                }
            }

            _state = children is null
                ? State.NoEvaluatability
                : State.CreateContainsEvaluatable(typeof(MemberInitExpression), children);

            return memberInit.Update(@new, ((IReadOnlyList<MemberBinding>?)bindings) ?? memberInit.Bindings);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitListInit(ListInitExpression listInit)
    {
        // First, visit the NewExpression and all initializers, saving states as well
        var @new = (NewExpression)Visit(listInit.NewExpression, out var newState);
        var state = newState.StateType;
        var initializers = listInit.Initializers;
        var initializerArgumentStates = new State[listInit.Initializers.Count][];

        IReadOnlyList<Expression>[]? visitedInitializersArguments = null;

        for (var i = 0; i < initializers.Count; i++)
        {
            var initializer = initializers[i];

            var visitedArguments = Visit(initializer.Arguments, ref state, out var argumentStates);
            if (visitedArguments is not null)
            {
                if (visitedInitializersArguments is null)
                {
                    visitedInitializersArguments = new IReadOnlyList<Expression>[initializers.Count];
                    for (var j = 0; j < i; j++)
                    {
                        visitedInitializersArguments[j] = initializers[j].Arguments;
                    }
                }
            }

            if (visitedInitializersArguments is not null)
            {
                visitedInitializersArguments[i] = (IReadOnlyList<Expression>?)visitedArguments ?? initializer.Arguments;
            }

            initializerArgumentStates[i] = argumentStates;
        }

        // We've visited everything and have both our aggregate state, and the states of all initializer expressions.
        switch (state)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
                if (IsGenerallyEvaluatable(listInit))
                {
                    _state = State.CreateEvaluatable(typeof(ListInitExpression), state is StateType.EvaluatableWithCapturedVariable);
                    break;
                }

                goto case StateType.ContainsEvaluatable;

            case StateType.ContainsEvaluatable:
                // If the NewExpression is evaluatable but one of the bindings isn't, we can't evaluate only the NewExpression
                // (ListInitExpression requires a NewExpression and doesn't accept ParameterException). However, we may still need to
                // evaluate constructor arguments in the NewExpression.
                if (newState.IsEvaluatable)
                {
                    @new = (NewExpression)newState.NotEvaluatableAsRootHandler!();
                }

                List<PathNode>? children = null;

                if (newState.ContainsEvaluatable)
                {
                    children =
                    [
                        newState.Path! with { PathFromParent = static e => Property(e, nameof(MethodCallExpression.Object)) }
                    ];
                }

                for (var i = 0; i < initializers.Count; i++)
                {
                    var initializer = initializers[i];

                    var visitedArguments = EvaluateList(
                        visitedInitializersArguments is null
                            ? initializer.Arguments
                            : visitedInitializersArguments[i],
                        initializerArgumentStates[i],
                        ref children,
                        static i => e =>
                            Call(
                                Property(e, nameof(MethodCallExpression.Arguments)),
                                ReadOnlyCollectionIndexerGetter,
                                arguments: [Constant(i)]));

                    if (visitedArguments is not null && visitedInitializersArguments is null)
                    {
                        visitedInitializersArguments = new IReadOnlyList<Expression>[initializers.Count];
                        for (var j = 0; j < i; j++)
                        {
                            visitedInitializersArguments[j] = initializers[j].Arguments;
                        }
                    }

                    if (visitedInitializersArguments is not null)
                    {
                        visitedInitializersArguments[i] = (IReadOnlyList<Expression>?)visitedArguments ?? initializer.Arguments;
                    }
                }

                _state = children is null
                    ? State.NoEvaluatability
                    : State.CreateContainsEvaluatable(typeof(ListInitExpression), children);
                break;

            default:
                throw new UnreachableException();
        }

        foreach (var argumentState in initializerArgumentStates)
        {
            StateArrayPool.Return(argumentState);
        }

        if (visitedInitializersArguments is null)
        {
            return listInit.Update(@new, listInit.Initializers);
        }

        var visitedInitializers = new ElementInit[initializers.Count];
        for (var i = 0; i < visitedInitializersArguments.Length; i++)
        {
            visitedInitializers[i] = initializers[i].Update(visitedInitializersArguments[i]);
        }

        return listInit.Update(@new, visitedInitializers);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unary)
    {
        var operand = Visit(unary.Operand, out var operandState);

        switch (operandState.StateType)
        {
            case StateType.NoEvaluatability:
                _state = State.NoEvaluatability;
                break;

            case StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable or StateType.Unknown:
            {
                if (IsGenerallyEvaluatable(unary))
                {
                    // Avoid allocating for the notEvaluatableAsRootHandler closure below unless we actually end up in the evaluatable case
                    var (unary2, operand2, operandState2) = (unary, operand, operandState);
                    _state = State.CreateEvaluatable(
                        typeof(UnaryExpression),
                        _state.ContainsCapturedVariable,
                        // See note below on EvaluateChildren
                        notEvaluatableAsRootHandler: () => EvaluateOperand(unary2, operand2, operandState2));
                    break;
                }

                goto case StateType.ContainsEvaluatable;
            }

            case StateType.ContainsEvaluatable:
                return EvaluateOperand(unary, operand, operandState);

            default:
                throw new UnreachableException();
        }

        return unary.Update(operand);

        // There are some cases of Convert nodes which we shouldn't evaluate when they're at the top of an evaluatable root (but can
        // evaluate when they're part of a larger fragment).
        // To support that, when the UnaryExpression is evaluatable, we include a nonEvaluatableAsRootHandler lambda in the returned state,
        // which gets invoked up the stack, calling this method. This evaluates the UnaryExpression's operand, but not the UnaryExpression.
        UnaryExpression EvaluateOperand(UnaryExpression unary, Expression operand, State operandState)
        {
            if (operandState.IsEvaluatable)
            {
                operand = ProcessEvaluatableRoot(operand, ref operandState);
            }

            if (_state.ContainsEvaluatable)
            {
                _state = _calculatingPath
                    ? State.CreateContainsEvaluatable(
                        typeof(UnaryExpression),
                        [_state.Path! with { PathFromParent = static e => Property(e, nameof(UnaryExpression.Operand)) }])
                    : State.NoEvaluatability;
            }

            return unary.Update(operand);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ElementInit VisitElementInit(ElementInit node)
        => throw new UnreachableException(); // Handled in VisitListInit

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        => throw new InvalidOperationException(CoreStrings.MemberListBindingNotSupported);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        => throw new InvalidOperationException(CoreStrings.MemberMemberBindingNotSupported);

    #endregion Visitation implementations

    #region Unsupported node types

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBlock(BlockExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override CatchBlock VisitCatchBlock(CatchBlock node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitDebugInfo(DebugInfoExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitDynamic(DynamicExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitGoto(GotoExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override LabelTarget VisitLabelTarget(LabelTarget? node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLabel(LabelExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLoop(LoopExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSwitch(SwitchExpression node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override SwitchCase VisitSwitchCase(SwitchCase node)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitTry(TryExpression node)
        => throw new NotSupportedException();

    #endregion Unsupported node types

    private static StateType CombineStateTypes(StateType stateType1, StateType stateType2)
        => (stateType1, stateType2) switch
        {
            (StateType.Unknown, var s) => s,
            (var s, StateType.Unknown) => s,

            (StateType.NoEvaluatability, StateType.NoEvaluatability) => StateType.NoEvaluatability,

            (StateType.EvaluatableWithoutCapturedVariable, StateType.EvaluatableWithoutCapturedVariable)
                => StateType.EvaluatableWithoutCapturedVariable,

            (StateType.EvaluatableWithCapturedVariable,
                StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable)
                or
                (StateType.EvaluatableWithCapturedVariable or StateType.EvaluatableWithoutCapturedVariable,
                StateType.EvaluatableWithCapturedVariable)
                => StateType.EvaluatableWithCapturedVariable,

            _ => StateType.ContainsEvaluatable
        };

    private Expression[]? Visit(
        ReadOnlyCollection<Expression> expressions,
        ref StateType aggregateStateType,
        out State[] expressionStates,
        bool poolExpressionStates = true)
        => Visit(expressions, Visit, ref aggregateStateType, out expressionStates, poolExpressionStates);

    // This follows the ExpressionVisitor.Visit(ReadOnlyCollection<T>) pattern.
    private T[]? Visit<T>(
        ReadOnlyCollection<T> expressions,
        Func<T, T> elementVisitor,
        ref StateType aggregateStateType,
        out State[] expressionStates,
        bool poolExpressionStates = true)
    {
        if (expressions.Count == 0)
        {
            aggregateStateType = CombineStateTypes(aggregateStateType, StateType.EvaluatableWithoutCapturedVariable);
            expressionStates = [];
            return null;
        }

        // In the normal case, the array for containing the expression states is pooled - we allocate it here and return it in the calling
        // function at the end of processing.
        // However, we have cases where a node is evaluatable, but not as an evaluatable root (e.g. NewExpression, NewArrayExpression - see
        // e.g. VisitNewExpression for more details). In these cases we return Evaluatable state, but with a "NotEvaluatableAsRootHandler"
        // that allows evaluating the node's children up the stack in case it's the root. The state array must continue living for that case
        // even once VisitNew returns, as the callback may be called later and needs to access the states. But the callback may also never
        // be called (if the NewExpression isn't a root, but rather part of a larger evaluatable fragment).
        // So we lack an easy place to return the array to the pool, and refrain from pooling it for that case (at least for now).
        expressionStates = poolExpressionStates ? StateArrayPool.Rent(expressions.Count) : new State[expressions.Count];

        T[]? newExpressions = null;
        for (var i = 0; i < expressions.Count; i++)
        {
            var oldExpression = expressions[i];
            var newExpression = elementVisitor(oldExpression);
            var expressionState = _state;

            if (!ReferenceEquals(newExpression, oldExpression) && newExpressions is null)
            {
                newExpressions = new T[expressions.Count];
                for (var j = 0; j < i; j++)
                {
                    newExpressions[j] = expressions[j];
                }
            }

            if (newExpressions is not null)
            {
                newExpressions[i] = newExpression;
            }

            expressionStates[i] = expressionState;

            aggregateStateType = CombineStateTypes(aggregateStateType, expressionState.StateType);
        }

        return newExpressions;
    }

    private Expression[]? EvaluateList(
        IReadOnlyList<Expression> expressions,
        State[] expressionStates,
        ref List<PathNode>? children,
        Func<int, Func<Expression, Expression>> pathFromParentGenerator)
    {
        // This allows us to make in-place changes in the expression array when the previous visitation pass made modifications (and so
        // returned a mutable array). This removes an additional copy that would be needed.
        var visitedExpressions = expressions as Expression[];

        for (var i = 0; i < expressions.Count; i++)
        {
            var argumentState = expressionStates[i];
            if (argumentState.IsEvaluatable)
            {
                if (visitedExpressions is null)
                {
                    visitedExpressions = new Expression[expressions.Count];
                    for (var j = 0; j < i; j++)
                    {
                        visitedExpressions[j] = expressions[j];
                    }
                }

                visitedExpressions[i] = ProcessEvaluatableRoot(expressions[i], ref argumentState);
                expressionStates[i] = argumentState;
            }
            else if (visitedExpressions is not null)
            {
                visitedExpressions[i] = expressions[i];
            }

            if (argumentState.ContainsEvaluatable && _calculatingPath)
            {
                children ??= [];
                children.Add(argumentState.Path! with { PathFromParent = pathFromParentGenerator(i) });
            }
        }

        return visitedExpressions;
    }

    [return: NotNullIfNotNull(nameof(evaluatableRoot))]
    private Expression? ProcessEvaluatableRoot(Expression? evaluatableRoot, ref State state)
    {
        if (evaluatableRoot is null)
        {
            return null;
        }

        var evaluateAsParameter =
            // In some cases, constantization is forced by the context ([NotParameterized], EF.Constant)
            !state.ForceConstantization
            && _parameterize
            && (
                // If the nodes contains a captured variable somewhere within it, we evaluate as a parameter.
                state.ContainsCapturedVariable
                // We don't evaluate as constant if we're not inside a lambda, i.e. in a top-level operator. This is to make sure that
                // non-lambda arguments to e.g. Skip/Take are parameterized rather than evaluated as constant, since that would produce
                // different SQLs for each value.
                || !_inLambda);

        // We have some cases where a node is evaluatable, but only as part of a larger subtree, and should not be evaluated as a tree root.
        // For these cases, the node's state has a notEvaluatableAsRootHandler lambda, which we can invoke to make evaluate the node's
        // children (as needed), but not itself.
        if (TryHandleNonEvaluatableAsRoot(evaluatableRoot, state, evaluateAsParameter, out var result))
        {
            return result;
        }

        var value = Evaluate(evaluatableRoot, out var parameterName, out var isContextAccessor);

        switch (value)
        {
            // If the query contains a nested IQueryable, e.g. Where(b => context.Blogs.Count()...), the context.Blogs parts gets
            // evaluated as a parameter; visit its expression tree instead.
            case IQueryable { Expression: var innerExpression }:
                return Visit(innerExpression);

            case Expression innerExpression when !isContextAccessor:
                return Visit(innerExpression);
        }

        if (isContextAccessor)
        {
            // Context accessors (query filters accessing the context) never get constantized
            evaluateAsParameter = true;
        }

        if (evaluateAsParameter)
        {
            if (_parameterizedValues.TryGetValue(evaluatableRoot, out var cachedParameter))
            {
                // We're here when the same captured variable (or other fragment) is referenced more than once in the query; we want to
                // use the same query parameter rather than sending it twice.
                // Note that in path calculation (precompiled query), we don't have to do anything, as the path only needs to be returned
                // once.
                state = State.NoEvaluatability;
                return cachedParameter;
            }

            if (_calculatingPath)
            {
                state = new()
                {
                    StateType = StateType.ContainsEvaluatable,
                    Path = new()
                    {
                        ExpressionType = state.ExpressionType!,
                        ParameterName = parameterName,
                        Children = Array.Empty<PathNode>()
                    }
                };

                // We still maintain _parameterValues since later parameter names are generated based on already-populated names.
                _parameterValues.AddParameter(parameterName, null);

                return evaluatableRoot;
            }

            // Regular parameter extraction mode; client-evaluate the subtree and replace it with a query parameter.
            state = State.NoEvaluatability;

            _parameterValues.AddParameter(parameterName, value);

            return _parameterizedValues[evaluatableRoot] = Parameter(evaluatableRoot.Type, parameterName);
        }

        // Evaluate as constant
        state = State.NoEvaluatability;

        // In precompilation mode, we don't care about constant evaluation since the expression tree itself isn't going to get used.
        // We only care about generating code for extracting captured variables, so ignore.
        if (_calculatingPath)
        {
            // TODO: EF.Constant is probably incompatible with precompilation, may need to throw (but not here, only from EF.Constant)
            return evaluatableRoot;
        }

        var returnType = evaluatableRoot.Type;
        var constantExpression = Constant(value, value?.GetType() ?? returnType);

        return constantExpression.Type != returnType
            ? Convert(constantExpression, returnType)
            : constantExpression;

        bool TryHandleNonEvaluatableAsRoot(Expression root, State state, bool asParameter, [NotNullWhen(true)] out Expression? result)
        {
            switch (root)
            {
                // We don't parameterize NewArrayExpression when its an evaluatable root, since we want to allow translating new[] { x, y }
                // to e.g. IN (x, y) rather than parameterizing the whole thing. But bubble up the evaluatable state so it may get evaluated
                // at a higher level.
                case NewArrayExpression when asParameter:
                // We don't constantize NewExpression/MemberInitExpression since that would embed arbitrary user type instances in our
                // shaper.
                case NewExpression or MemberInitExpression when !asParameter:
                // There are some cases of Convert nodes which we shouldn't evaluate when they're at the top of an evaluatable root (but can
                // evaluate when they're part of a larger fragment).
                case UnaryExpression unary when PreserveConvertNode(unary):
                    result = state.NotEvaluatableAsRootHandler!();
                    return true;

                default:
                    result = null;
                    return false;
            }

            bool PreserveConvertNode(Expression expression)
            {
                if (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression)
                {
                    if (unaryExpression.Type == typeof(object)
                        || unaryExpression.Type == typeof(Enum)
                        || unaryExpression.Operand.Type.UnwrapNullableType().IsEnum)
                    {
                        return true;
                    }

                    var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                    if (unaryExpression.Type.UnwrapNullableType() == typeof(int)
                        && (innerType == typeof(byte)
                            || innerType == typeof(sbyte)
                            || innerType == typeof(char)
                            || innerType == typeof(short)
                            || innerType == typeof(ushort)))
                    {
                        return true;
                    }

                    return PreserveConvertNode(unaryExpression.Operand);
                }

                return false;
            }
        }
    }

    private object? Evaluate(Expression? expression)
        => Evaluate(expression, out _, out _);

    private object? Evaluate(Expression? expression, out string parameterName, out bool isContextAccessor)
    {
        var value = EvaluateCore(expression, out var tempParameterName, out isContextAccessor);
        parameterName = tempParameterName ?? "p";

        var compilerPrefixIndex = parameterName.LastIndexOf('>');
        if (compilerPrefixIndex != -1)
        {
            parameterName = parameterName[(compilerPrefixIndex + 1)..];
        }

        // The VB compiler prefixes closure member names with $VB$Local_, remove that (#33150)
        if (parameterName.StartsWith("$VB$Local_", StringComparison.Ordinal))
        {
            parameterName = parameterName.Substring("$VB$Local_".Length);
        }

        parameterName = $"{QueryCompilationContext.QueryParameterPrefix}{parameterName}_{_parameterValues.ParameterValues.Count}";

        return value;

        object? EvaluateCore(Expression? expression, out string? parameterName, out bool isContextAccessor)
        {
            parameterName = null;
            isContextAccessor = false;

            if (expression == null)
            {
                return null;
            }

            if (_generateContextAccessors)
            {
                var visited = _contextParameterReplacer.Visit(expression);

                if (visited != expression)
                {
                    parameterName = QueryFilterPrefix
                        + (RemoveConvert(expression) is MemberExpression { Member.Name: var memberName } ? ("__" + memberName) : "__p");
                    isContextAccessor = true;

                    return Lambda(visited, _contextParameterReplacer.ContextParameterExpression);
                }

                static Expression RemoveConvert(Expression expression)
                    => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
                        ? RemoveConvert(unaryExpression.Operand)
                        : expression;
            }

            switch (expression)
            {
                case MemberExpression memberExpression:
                    var instanceValue = EvaluateCore(memberExpression.Expression, out parameterName, out isContextAccessor);
                    try
                    {
                        switch (memberExpression.Member)
                        {
                            case FieldInfo fieldInfo:
                                parameterName = parameterName is null ? fieldInfo.Name : $"{parameterName}_{fieldInfo.Name}";
                                return fieldInfo.GetValue(instanceValue);

                            case PropertyInfo propertyInfo:
                                parameterName = parameterName is null ? propertyInfo.Name : $"{parameterName}_{propertyInfo.Name}";
                                return propertyInfo.GetValue(instanceValue);
                        }
                    }
                    catch
                    {
                        // Try again when we compile the delegate
                    }

                    break;

                case ConstantExpression constantExpression:
                    return constantExpression.Value;

                case MethodCallExpression methodCallExpression:
                    parameterName = methodCallExpression.Method.Name;
                    break;

                case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
                    when (unaryExpression.Type.UnwrapNullableType() == unaryExpression.Operand.Type):
                    return EvaluateCore(unaryExpression.Operand, out parameterName, out isContextAccessor);
            }

            try
            {
                return Lambda<Func<object>>(
                        Convert(expression, typeof(object)))
                    .Compile(preferInterpretation: true)
                    .Invoke();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    _logger.ShouldLogSensitiveData()
                        ? CoreStrings.ExpressionParameterizationExceptionSensitive(expression)
                        : CoreStrings.ExpressionParameterizationException,
                    exception);
            }
        }
    }

    private bool IsGenerallyEvaluatable(Expression expression)
        => _evaluatableExpressionFilter.IsEvaluatableExpression(expression, _model)
            && (_parameterize
                // Don't evaluate QueryableMethods if in compiled query
                || !(expression is MethodCallExpression { Method: var method } && method.DeclaringType == typeof(Queryable)));

    private enum StateType
    {
        /// <summary>
        /// A temporary initial state, before any children have been examined.
        /// </summary>
        Unknown,

        /// <summary>
        /// Means that the current node is neither evaluatable, nor does it contains an evaluatable node.
        /// </summary>
        NoEvaluatability,

        /// <summary>
        ///     Whether the current node is evaluatable, i.e. contains no references to server-side resources, and does not contain any
        ///     captured variables. Such nodes can be evaluated and the result integrated as constants in the tree.
        /// </summary>
        EvaluatableWithoutCapturedVariable,

        /// <summary>
        ///     Whether the current node is evaluatable, i.e. contains no references to server-side resources, but contains captured
        ///     variables. Such nodes can be parameterized.
        /// </summary>
        EvaluatableWithCapturedVariable,

        /// <summary>
        /// Whether the current node contains (parameterizable) evaluatable nodes anywhere within its children.
        /// </summary>
        ContainsEvaluatable
    }

    private readonly record struct State
    {
        public static State CreateEvaluatable(
            Type expressionType,
            bool containsCapturedVariable,
            Func<Expression>? notEvaluatableAsRootHandler = null)
            => new()
            {
                StateType = containsCapturedVariable
                    ? StateType.EvaluatableWithCapturedVariable
                    : StateType.EvaluatableWithoutCapturedVariable,
                ExpressionType = expressionType,
                NotEvaluatableAsRootHandler = notEvaluatableAsRootHandler
            };

        public static State CreateContainsEvaluatable(Type expressionType, IReadOnlyList<PathNode> children)
            => new()
            {
                StateType = StateType.ContainsEvaluatable,
                Path = new() { ExpressionType = expressionType, Children = children }
            };

        /// <summary>
        /// Means that we're neither within an evaluatable subtree, nor on a node which contains one (and therefore needs to track the
        /// path to it).
        /// </summary>
        public static readonly State NoEvaluatability = new() { StateType = StateType.NoEvaluatability };

        public StateType StateType { get; init; }

        public Type? ExpressionType { get; init; }

        /// <summary>
        /// A tree containing information on reaching all evaluatable nodes contained within this node.
        /// </summary>
        public PathNode? Path { get; init; }

        public bool ForceConstantization { get; init; }

        public Func<Expression>? NotEvaluatableAsRootHandler { get; init; }

        public bool IsEvaluatable
            => StateType is StateType.EvaluatableWithoutCapturedVariable or StateType.EvaluatableWithCapturedVariable or StateType.Unknown;

        public bool ContainsCapturedVariable
            => StateType is StateType.EvaluatableWithCapturedVariable;

        public bool ContainsEvaluatable
            => StateType is StateType.ContainsEvaluatable;

        public override string ToString()
            => StateType switch
            {
                StateType.NoEvaluatability => "No evaluatability",
                StateType.EvaluatableWithoutCapturedVariable => "Evaluatable, no captured vars",
                StateType.EvaluatableWithCapturedVariable => "Evaluatable, captured vars",
                StateType.ContainsEvaluatable => "Contains evaluatable",

                _ => throw new UnreachableException()
            };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public sealed record PathNode
    {
        /// <summary>
        ///     The type of the expression represented by this <see cref="PathNode" />.
        /// </summary>
        public required Type ExpressionType { get; init; }

        /// <summary>
        ///     Children of this node which contain parameterizable fragments.
        /// </summary>
        public required IReadOnlyList<PathNode>? Children { get; init; }

        /// <summary>
        ///     A function that accepts the parent node, and returns an expression representing the path to this node from that parent
        ///     node. The returned expression can then be used to generate C# code that traverses the expression tree.
        /// </summary>
        public Func<Expression, Expression>? PathFromParent { get; init; }

        /// <summary>
        ///     For nodes representing parameterizable roots, contains the preferred parameter name, generated based on the expression
        ///     node type/contents.
        /// </summary>
        public string? ParameterName { get; init; }
    }

    private sealed class ContextParameterReplacer(Type contextType) : ExpressionVisitor
    {
        public ParameterExpression ContextParameterExpression { get; } = Parameter(contextType, "context");

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
            => expression?.Type != typeof(object)
                && expression?.Type.IsAssignableFrom(contextType) == true
                    ? ContextParameterExpression
                    : base.Visit(expression);
    }

    private sealed class DummyParameterValues : IParameterValues
    {
        private readonly Dictionary<string, object?> _parameterValues = new();

        public IReadOnlyDictionary<string, object?> ParameterValues
            => _parameterValues;

        public void AddParameter(string name, object? value)
            => _parameterValues.Add(name, value);
    }
}
