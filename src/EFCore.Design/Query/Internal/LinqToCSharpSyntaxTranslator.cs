// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using E = System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class LinqToCSharpSyntaxTranslator : ExpressionVisitor, ILinqToCSharpSyntaxTranslator
{
    private sealed record StackFrame(
        Dictionary<ParameterExpression, string> Variables,
        HashSet<string> VariableNames,
        Dictionary<LabelTarget, string> Labels,
        HashSet<string> UnnamedLabelNames);

    private readonly Stack<StackFrame> _stack
        = new(new[] { new StackFrame(new(), new(), new(), new()) });

    private int _unnamedParameterCounter;

    private sealed record LiftedState(
        List<StatementSyntax> Statements,
        Dictionary<ParameterExpression, string> Variables,
        HashSet<string> VariableNames,
        List<LocalDeclarationStatementSyntax> UnassignedVariableDeclarations);

    private LiftedState _liftedState = new(new(), new(), new(), new());

    private ExpressionContext _context;
    private bool _onLastLambdaLine;

    private readonly HashSet<ParameterExpression> _capturedVariables = new();
    private ISet<string> _collectedNamespaces = null!;

    private static MethodInfo? _activatorCreateInstanceMethod;
    private static MethodInfo? _typeGetFieldMethod;
    private static MethodInfo? _fieldGetValueMethod;
    private static MethodInfo? _mathPowMethod;

    private readonly SideEffectDetectionSyntaxWalker _sideEffectDetector = new();
    private readonly ConstantDetectionSyntaxWalker _constantDetector = new();
    private readonly SyntaxGenerator _g;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public LinqToCSharpSyntaxTranslator(SyntaxGenerator syntaxGenerator)
        => _g = syntaxGenerator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlySet<ParameterExpression> CapturedVariables
        => _capturedVariables.ToHashSet();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SyntaxNode? Result { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SyntaxNode TranslateStatement(Expression node, ISet<string> collectedNamespaces)
        => TranslateCore(node, collectedNamespaces, statementContext: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SyntaxNode TranslateExpression(Expression node, ISet<string> collectedNamespaces)
        => TranslateCore(node, collectedNamespaces, statementContext: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SyntaxNode TranslateCore(Expression node, ISet<string> collectedNamespaces, bool statementContext = false)
    {
        _capturedVariables.Clear();
        _collectedNamespaces = collectedNamespaces;
        _unnamedParameterCounter = 0;
        _context = statementContext ? ExpressionContext.Statement : ExpressionContext.Expression;
        _onLastLambdaLine = true;

        Visit(node);

        if (_liftedState.Statements.Count > 0)
        {
            if (_context == ExpressionContext.Expression)
            {
                throw new NotSupportedException("Lifted expressions remaining at top-level in expression context");
            }
        }

        Check.DebugAssert(_stack.Count == 1, "_parameterStack.Count == 1");
        Check.DebugAssert(_stack.Peek().Variables.Count == 0, "_stack.Peek().Parameters.Count == 0");
        Check.DebugAssert(_stack.Peek().VariableNames.Count == 0, "_stack.Peek().ParameterNames.Count == 0");
        Check.DebugAssert(_stack.Peek().Labels.Count == 0, "_stack.Peek().Labels.Count == 0");
        Check.DebugAssert(_stack.Peek().UnnamedLabelNames.Count == 0, "_stack.Peek().UnnamedLabelNames.Count == 0");

        return Result!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(node))]
    protected virtual SyntaxNode? Translate(Expression? node)
    {
        Visit(node);

        return Result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual T Translate<T>(Expression? node) where T : CSharpSyntaxNode
    {
        Visit(node);

        return Result as T
               ?? throw new InvalidOperationException(
                   $"Got translated node of type '{Result?.GetType().Name ?? "<null>"}' instead of the expected {typeof(T)}");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ExpressionSyntax Translate(Expression expression, IdentifierNameSyntax? lowerableAssignmentVariable)
    {
        Check.DebugAssert(
            _context is ExpressionContext.Expression or ExpressionContext.ExpressionLambda,
            "Cannot lower in statement context");

        return expression switch
        {
            SwitchExpression switchExpression
                => (ExpressionSyntax)TranslateSwitch(switchExpression, lowerableAssignmentVariable),

            ConditionalExpression conditionalExpression
                => (ExpressionSyntax)TranslateConditional(conditionalExpression, lowerableAssignmentVariable),

            _ => Translate<ExpressionSyntax>(expression)
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("node")]
    public override Expression? Visit(Expression? node)
    {
        if (node is null)
        {
            Result = null;
            return null;
        }

        return base.Visit(node);
    }

    /// <inheritdoc />
    protected override Expression VisitBinary(BinaryExpression binary)
    {
        using var _ = ChangeContext(ExpressionContext.Expression);

        // Handle some special cases
        switch (binary.NodeType)
        {
            case ExpressionType.Assign:
                return VisitAssignment(binary);

            case ExpressionType.Power when binary.Left.Type == typeof(double) && binary.Right.Type == typeof(double):
                return Visit(
                    E.Call(
                        _mathPowMethod ??= typeof(Math).GetMethod(
                            nameof(Math.Pow), BindingFlags.Static | BindingFlags.Public, new[] { typeof(double), typeof(double) })!,
                        binary.Left,
                        binary.Right));

            case ExpressionType.Power:
                throw new NotImplementedException("Power over non-double operands");

            case ExpressionType.PowerAssign:
                return Visit(
                    E.Assign(
                        binary.Left,
                        E.Power(
                            binary.Left,
                            binary.Right)));
        }

        var liftedStatementOrigPosition = _liftedState.Statements.Count;
        var left = Translate<ExpressionSyntax>(binary.Left);
        var liftedStatementLeftPosition = _liftedState.Statements.Count;
        var right = Translate<ExpressionSyntax>(binary.Right);

        // If both sides were lifted, we don't need to do anything special. Same if the left side was lifted.
        // But if the right side was lifted and the left wasn't, then in order to preserve evaluation order we need to lift the left side
        // out as well, otherwise the right side gets evaluated before the left.
        // We refrain from doing this only if the two expressions can't possibly have side effects over each other, for nicer code.
        if (_liftedState.Statements.Count > liftedStatementLeftPosition
            && liftedStatementLeftPosition == liftedStatementOrigPosition
            && !_sideEffectDetector.CanBeReordered(left, right))
        {
            var name = UniquifyVariableName("lifted");
            _liftedState.Statements.Insert(
                liftedStatementLeftPosition,
                GenerateVarDeclaration(name, left));
            _liftedState.VariableNames.Add(name);
            left = IdentifierName(name);
        }

        if (binary.NodeType == ExpressionType.ArrayIndex)
        {
            Result = ElementAccessExpression(left, BracketedArgumentList(SingletonSeparatedList(Argument(right))));
            return binary;
        }

        // TODO: Confirm what to do with the checked expression types

        var syntaxKind = binary.NodeType switch
        {
            ExpressionType.Equal => SyntaxKind.EqualsExpression,
            ExpressionType.NotEqual => SyntaxKind.NotEqualsExpression,

            ExpressionType.Add => SyntaxKind.AddExpression,
            ExpressionType.AddChecked => SyntaxKind.AddExpression,
            ExpressionType.Subtract => SyntaxKind.SubtractExpression,
            ExpressionType.SubtractChecked => SyntaxKind.SubtractExpression,
            ExpressionType.Multiply => SyntaxKind.MultiplyExpression,
            ExpressionType.MultiplyChecked => SyntaxKind.MultiplyExpression,
            ExpressionType.Divide => SyntaxKind.DivideExpression,
            ExpressionType.Modulo => SyntaxKind.ModuloExpression,
            ExpressionType.AddAssign => SyntaxKind.AddAssignmentExpression,
            ExpressionType.AddAssignChecked => SyntaxKind.AddAssignmentExpression,
            ExpressionType.SubtractAssign => SyntaxKind.SubtractAssignmentExpression,
            ExpressionType.SubtractAssignChecked => SyntaxKind.SubtractAssignmentExpression,
            ExpressionType.MultiplyAssign => SyntaxKind.MultiplyAssignmentExpression,
            ExpressionType.MultiplyAssignChecked => SyntaxKind.MultiplyAssignmentExpression,
            ExpressionType.DivideAssign => SyntaxKind.DivideAssignmentExpression,
            ExpressionType.ModuloAssign => SyntaxKind.ModuloAssignmentExpression,

            ExpressionType.GreaterThan => SyntaxKind.GreaterThanExpression,
            ExpressionType.GreaterThanOrEqual => SyntaxKind.GreaterThanOrEqualExpression,
            ExpressionType.LessThan => SyntaxKind.LessThanExpression,
            ExpressionType.LessThanOrEqual => SyntaxKind.LessThanOrEqualExpression,

            ExpressionType.AndAlso => SyntaxKind.LogicalAndExpression,
            ExpressionType.OrElse => SyntaxKind.LogicalOrExpression,
            ExpressionType.AndAssign => SyntaxKind.AndAssignmentExpression,
            ExpressionType.OrAssign => SyntaxKind.OrAssignmentExpression,

            ExpressionType.And => SyntaxKind.BitwiseAndExpression,
            ExpressionType.Or => SyntaxKind.BitwiseOrExpression,
            ExpressionType.ExclusiveOr => SyntaxKind.ExclusiveOrExpression,
            ExpressionType.LeftShift => SyntaxKind.LeftShiftExpression,
            ExpressionType.RightShift => SyntaxKind.RightShiftExpression,
            // TODO UnsignedRightShiftExpression
            ExpressionType.ExclusiveOrAssign => SyntaxKind.ExclusiveOrAssignmentExpression,
            ExpressionType.LeftShiftAssign => SyntaxKind.LeftShiftAssignmentExpression,
            ExpressionType.RightShiftAssign => SyntaxKind.RightShiftAssignmentExpression,

            ExpressionType.TypeIs => SyntaxKind.IsExpression,
            ExpressionType.TypeAs => SyntaxKind.AsExpression,
            ExpressionType.Coalesce => SyntaxKind.CoalesceExpression,

            _ => throw new ArgumentOutOfRangeException("BinaryExpression with " + binary.NodeType)
        };

        Result = BinaryExpression(syntaxKind, left, right);

        return binary;

        Expression VisitAssignment(BinaryExpression assignment)
        {
            var translatedLeft = Translate<ExpressionSyntax>(assignment.Left);

            ExpressionSyntax translatedRight;

            // LINQ expression trees can directly access private members, but C# code cannot.
            // If a private member is being set, VisitMember generated a reflection GetValue invocation for it; detect
            // that here and replace it with SetValue instead.
            // TODO: Replace this with a more efficient API for .NET 8.0.
            // TODO: Private property
            if (translatedLeft is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name.Identifier.Text: nameof(FieldInfo.GetValue),
                        Expression: var fieldInfoExpression
                    },
                    ArgumentList.Arguments: [var lValue]
                })
            {
                translatedRight = Translate<ExpressionSyntax>(assignment.Right);

                Result = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        fieldInfoExpression,
                        IdentifierName(nameof(FieldInfo.SetValue))),
                    ArgumentList(
                        SeparatedList(new[] { lValue, Argument(translatedRight) })));
            }
            else
            {
                // Identify assignment where the RHS supports assignment lowering (switch, conditional). If the e.g. switch expression is
                // lifted out (because some arm contains a block), this will lower the variable to be assigned inside the resulting switch
                // statement, rather then adding another useless temporary variable.
                translatedRight = Translate(
                    assignment.Right,
                    lowerableAssignmentVariable: translatedLeft as IdentifierNameSyntax);

                // If the RHS was lifted out and the assignment lowering succeeded, Translate above returns the lowered assignment variable;
                // this would mean that we return a useless identity assignment (i = i). Instead, just return it.
                if (translatedRight == translatedLeft)
                {
                    Result = translatedRight;
                }
                else
                {
                    Result = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, translatedLeft, translatedRight);
                }
            }

            return assignment;
        }
    }

    /// <inheritdoc />
    protected override Expression VisitBlock(BlockExpression block)
    {
        var blockContext = _context;

        var parentOnLastLambdaLine = _onLastLambdaLine;
        var parentLiftedState = _liftedState;

        // Expression blocks have no stack of their own, since they're lifted directly to their parent non-expression block.
        StackFrame? ownStackFrame = null;
        if (blockContext != ExpressionContext.Expression)
        {
            ownStackFrame = PushNewStackFrame();
            _liftedState = new(new(), new(), new(), new());
        }

        var stackFrame = _stack.Peek();

        // Do a 1st pass to identify and register any labels, since goto can appear before its label.
        PreprocessLabels();

        try
        {
            // Go over the block's variables, assign names to any unnamed ones and uniquify. Then add them to our stack frame, unless
            // this is an expression block that will get lifted.
            foreach (var parameter in block.Variables)
            {
                var (variables, variableNames) = (stackFrame.Variables, stackFrame.VariableNames);

                var uniquifiedName = UniquifyVariableName(parameter.Name ?? "unnamed");

                if (blockContext == ExpressionContext.Expression)
                {
                    _liftedState.Variables.Add(parameter, uniquifiedName);
                    _liftedState.VariableNames.Add(uniquifiedName);
                }
                else
                {
                    variables.Add(parameter, uniquifiedName);
                    variableNames.Add(uniquifiedName);
                }
            }

            var unassignedVariables = block.Variables.ToList();

            var statements = new List<StatementSyntax>();
            LabeledStatementSyntax? pendingLabeledStatement = null;

            // Now visit the block's expressions
            for (var i = 0; i < block.Expressions.Count; i++)
            {
                var expression = block.Expressions[i];
                var onLastBlockLine = i == block.Expressions.Count - 1;
                _onLastLambdaLine = parentOnLastLambdaLine && onLastBlockLine;

                // Any lines before the last are evaluated in statement context (they aren't returned); the last line is evaluated in the
                // context of the block as a whole. _context now refers to the statement's context, blockContext to the block's.
                var statementContext = onLastBlockLine ? _context : ExpressionContext.Statement;

                SyntaxNode translated;
                using (ChangeContext(statementContext))
                {
                    translated = Translate(expression);
                }

                // If we have a labeled statement, unwrap it and keep the label as pending. VisitLabel returns a dummy statement (since
                // LINQ labels don't have a statement, unlike C#), so we'll skip that statement and add the label to the next real one.
                if (translated is LabeledStatementSyntax labeledStatement)
                {
                    if (pendingLabeledStatement is not null)
                    {
                        throw new NotImplementedException("Multiple labels on the same statement");
                    }

                    pendingLabeledStatement = labeledStatement;
                    translated = labeledStatement.Statement;
                }

                // Syntax optimization. This is an assignment of a block variable to some value. Render this as:
                // var x = <expression>;
                // ... instead of:
                // int x;
                // x = <expression>;
                // ... except for expression context (i.e. on the last line), where we just return the value if needed.
                if (expression is BinaryExpression { NodeType: ExpressionType.Assign, Left: ParameterExpression lValue }
                    && translated is AssignmentExpressionSyntax { Right: var valueSyntax }
                    && statementContext == ExpressionContext.Statement
                    && unassignedVariables.Remove(lValue))
                {
                    var useExplicitVariableType = valueSyntax.Kind() == SyntaxKind.NullLiteralExpression;

                    translated = useExplicitVariableType
                        ? _g.LocalDeclarationStatement(Translate(lValue.Type), LookupVariableName(lValue), valueSyntax)
                        : _g.LocalDeclarationStatement(LookupVariableName(lValue), valueSyntax);
                }

                if (statementContext == ExpressionContext.Expression)
                {
                    // We're on the last line of a block in expression context - the block is being lifted out.
                    // All statements before the last line (this one) have already been added to _liftedStatements, just return the last
                    // expression.
                    Check.DebugAssert(onLastBlockLine, "onLastBlockLine");
                    Result = translated;
                    break;
                }

                if (blockContext != ExpressionContext.Expression)
                {
                    if (_liftedState.Statements.Count > 0)
                    {
                        // If any expressions were lifted out of the current expression, flatten them into our own block, just before the
                        // expression from which they were lifted. Note that we don't do this in Expression context, since our own block is
                        // lifted out.
                        statements.AddRange(_liftedState.Statements);
                        _liftedState.Statements.Clear();
                    }

                    // Same for any variables being lifted out of the block; we add them to our own stack frame so that we can do proper
                    // variable name uniquification etc.
                    if (_liftedState.Variables.Count > 0)
                    {
                        foreach (var (parameter, name) in _liftedState.Variables)
                        {
                            stackFrame.Variables[parameter] = name;
                            stackFrame.VariableNames.Add(name);
                        }

                        _liftedState.Variables.Clear();
                    }
                }

                // Skip useless expressions with no side effects in statement context (these can be the result of switch/conditional lifting
                // with assignment lowering)
                if (statementContext == ExpressionContext.Statement && !_sideEffectDetector.MayHaveSideEffects(translated))
                {
                    continue;
                }

                var statement = translated switch
                {
                    StatementSyntax s => s,

                    // If this is the last line in an expression lambda, wrap it in a return statement.
                    ExpressionSyntax e when _onLastLambdaLine && statementContext == ExpressionContext.ExpressionLambda
                        => ReturnStatement(e),

                    // If we're in statement context and we have an expression that can't stand alone (e.g. literal), assign it to discard
                    ExpressionSyntax e when statementContext == ExpressionContext.Statement && !IsExpressionValidAsStatement(e)
                        => ExpressionStatement((ExpressionSyntax)_g.AssignmentStatement(_g.IdentifierName("_"), e)),

                    ExpressionSyntax e => ExpressionStatement(e),

                    _ => throw new ArgumentOutOfRangeException()
                };

                if (blockContext == ExpressionContext.Expression)
                {
                    // This block is in expression context, and so will be lifted (we won't be returning a block).
                    _liftedState.Statements.Add(statement);
                }
                else
                {
                    if (pendingLabeledStatement is not null)
                    {
                        statement = pendingLabeledStatement.WithStatement(statement);
                        pendingLabeledStatement = null;
                    }

                    statements.Add(statement);
                }
            }

            // If a label existed on the last line of the block, add an empty statement (since C# requires it); for expression blocks we'd
            // have to lift that, not supported for now.
            if (pendingLabeledStatement is not null)
            {
                if (blockContext == ExpressionContext.Expression)
                {
                    throw new NotImplementedException("Label on last expression of an expression block");
                }
                else
                {
                    statements.Add(pendingLabeledStatement.WithStatement(EmptyStatement()));
                }
            }

            // Above we transform top-level assignments (i = 8) to var-declarations with initializers (var i = 8); those variables have
            // already been taken care of and removed from the list.
            // But there may still be variables that get assigned inside nested blocks or other situations; prepare declarations for those
            // and either add them to the block, or lift them if we're an expression block.
            var unassignedVariableDeclarations =
                unassignedVariables.Select(
                    v => (LocalDeclarationStatementSyntax)_g.LocalDeclarationStatement(Translate(v.Type), LookupVariableName(v)));

            if (blockContext == ExpressionContext.Expression)
            {
                _liftedState.UnassignedVariableDeclarations.AddRange(unassignedVariableDeclarations);
            }
            else
            {
                statements.InsertRange(0, unassignedVariableDeclarations.Concat(_liftedState.UnassignedVariableDeclarations));
                _liftedState.UnassignedVariableDeclarations.Clear();

                // We're done. If the block is in an expression context, it needs to be lifted out; but not if it's in a lambda (in that
                // case we just added return above).
                Result = Block(statements);
            }

            return block;
        }
        finally
        {
            _onLastLambdaLine = parentOnLastLambdaLine;
            _liftedState = parentLiftedState;

            if (ownStackFrame is not null)
            {
                var popped = _stack.Pop();
                Check.DebugAssert(popped.Equals(ownStackFrame), "popped.Equals(ownStackFrame)");
            }
        }

        // Returns true for expressions which have side-effects, and can therefore appear alone as a statement
        static bool IsExpressionValidAsStatement(ExpressionSyntax expression)
            => expression.Kind() switch
            {
                SyntaxKind.InvocationExpression => true,

                SyntaxKind.AddAssignmentExpression => true,
                SyntaxKind.AndAssignmentExpression => true,
                SyntaxKind.CoalesceAssignmentExpression => true,
                SyntaxKind.DivideAssignmentExpression => true,
                SyntaxKind.ModuloAssignmentExpression => true,
                SyntaxKind.MultiplyAssignmentExpression => true,
                SyntaxKind.OrAssignmentExpression => true,
                SyntaxKind.SimpleAssignmentExpression => true,
                SyntaxKind.SubtractAssignmentExpression => true,
                SyntaxKind.ExclusiveOrAssignmentExpression => true,
                SyntaxKind.LeftShiftAssignmentExpression => true,
                SyntaxKind.RightShiftAssignmentExpression => true,

                SyntaxKind.PostIncrementExpression => true,
                SyntaxKind.PostDecrementExpression => true,
                SyntaxKind.PreIncrementExpression => true,
                SyntaxKind.PreDecrementExpression => true,

                _ => false
            };

        void PreprocessLabels()
        {
            // LINQ label targets can be unnamed, so we need to generate names for unnamed ones and maintain a target->name mapping.
            // We need to maintain this as a stack for every block which has labels.
            // Normal blocks get their own labels stack frame, which gets popped when we leave the block. Expression labels add their
            // labels to their parent's stack frame (since they get lifted).
            var stackFrame = _stack.Peek();

            foreach (var label in block.Expressions.OfType<LabelExpression>())
            {
                if (stackFrame.Labels.TryGetValue(label.Target, out var identifier))
                {
                    continue;
                }

                var (_, _, labels, unnamedLabelNames) = stackFrame;

                // Generate names for unnamed label targets and uniquify
                identifier = label.Target.Name ?? "unnamedLabel";
                var identifierBase = identifier;
                for (var i = 0; unnamedLabelNames.Contains(identifier); i++)
                {
                    identifier = identifierBase + i;
                }

                if (label.Target.Name is null)
                {
                    unnamedLabelNames.Add(identifier);
                }
                labels.Add(label.Target, identifier);
            }
        }
    }

    /// <inheritdoc />
    protected override CatchBlock VisitCatchBlock(CatchBlock catchBlock)
    {
        Result = TranslateCatchBlock(catchBlock);

        return catchBlock;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SyntaxNode TranslateCatchBlock(CatchBlock catchBlock, bool noType = false)
    {
        var translatedBody = Translate(catchBlock.Body) switch
        {
            BlockSyntax b => b,
            StatementSyntax s => Block(s),
            ExpressionSyntax e => Block(ExpressionStatement(e)),
            _ => throw new ArgumentOutOfRangeException()
        };

        var catchDeclaration = noType
            ? null
            : CatchDeclaration(Translate(catchBlock.Test));

        if (catchBlock.Variable is not null)
        {
            Check.DebugAssert(catchDeclaration is not null, "catchDeclaration is not null");

            if (catchBlock.Variable.Name is null)
            {
                throw new NotSupportedException("TranslateCatchBlock: unnamed parameter as catch variable");
            }

            catchDeclaration = catchDeclaration.WithIdentifier(Identifier(catchBlock.Variable.Name));
        }

        return CatchClause(
            catchDeclaration,
            catchBlock.Filter is null ? null : CatchFilterClause(Translate<ExpressionSyntax>(catchBlock.Filter)),
            translatedBody);
    }

    /// <inheritdoc />
    protected override Expression VisitConditional(ConditionalExpression conditional)
    {
        Result = TranslateConditional(conditional, lowerableAssignmentVariable: null);

        return conditional;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual CSharpSyntaxNode TranslateConditional(
        ConditionalExpression conditional,
        IdentifierNameSyntax? lowerableAssignmentVariable)
    {
        // ConditionalExpression can be an expression or an if/else statement.
        var test = Translate<ExpressionSyntax>(conditional.Test);

        var isFalseAbsent = conditional.IfFalse is DefaultExpression defaultIfFalse && defaultIfFalse.Type == typeof(void);

        switch (_context)
        {
            case ExpressionContext.Statement:
                return TranslateConditionalStatement(conditional);

            case ExpressionContext.Expression:
            case ExpressionContext.ExpressionLambda:
            {
                if (isFalseAbsent)
                {
                    throw new NotSupportedException(
                        $"Missing {nameof(System.Linq.Expressions.ConditionalExpression.IfFalse)} in {nameof(ConditionalExpression)} in expression context");
                }

                var parentLiftedState = _liftedState;
                _liftedState = new(new(), new(), new(), new());

                // If we're in a lambda body, we try to translate as an expression if possible (i.e. no blocks in the true/false arms).
                using (ChangeContext(ExpressionContext.Expression))
                {
                    var ifTrue = Translate(conditional.IfTrue);
                    var ifFalse = Translate(conditional.IfFalse);

                    if (ifTrue is not ExpressionSyntax ifTrueExpression
                        || ifFalse is not ExpressionSyntax ifFalseExpression)
                    {
                        throw new InvalidOperationException("Trying to evaluate a non-expression condition in expression context");
                    }

                    // There were no lifted expressions inside either arm - we can translate directly to a C# conditional expression
                    if (_liftedState.Statements.Count == 0)
                    {
                        _liftedState = parentLiftedState;
                        return ConditionalExpression(test, ifTrueExpression, ifFalseExpression);
                    }
                }

                // If we're in a lambda body and couldn't translate as a conditional expression, translate as an if/else statement with
                // return. Wrap the true/false sides in blocks to have "return" added.
                if (_context == ExpressionContext.ExpressionLambda)
                {
                    _liftedState = parentLiftedState;

                    return Block(TranslateConditionalStatement(conditional.Update(
                        conditional.Test,
                        conditional.IfTrue is BlockExpression ? conditional.IfTrue : E.Block(conditional.IfTrue),
                        conditional.IfFalse is BlockExpression ? conditional.IfFalse : E.Block(conditional.IfFalse))));
                }

                // We're in regular expression context, and there are lifted expressions inside one of the arms; we translate to an if/else
                // statement but lowering an assignment into both sides of the condition
                _liftedState = new(new(), new(), new(), new());

                IdentifierNameSyntax assignmentVariable;
                TypeSyntax? loweredAssignmentVariableType = null;

                if (lowerableAssignmentVariable is null)
                {
                    var name = UniquifyVariableName("liftedConditional");
                    var parameter = E.Parameter(conditional.Type, name);
                    assignmentVariable = IdentifierName(name);
                    loweredAssignmentVariableType = Translate(parameter.Type);
                }
                else
                {
                    assignmentVariable = lowerableAssignmentVariable;
                }

                var ifTrueStatement = ProcessArmBody(conditional.IfTrue);
                var ifFalseStatement = ProcessArmBody(conditional.IfFalse);

                _liftedState = parentLiftedState;

                if (lowerableAssignmentVariable is null)
                {
                    _liftedState.Statements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(loweredAssignmentVariableType!)
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(assignmentVariable.Identifier.Text)))));
                }
                _liftedState.Statements.Add(IfStatement(test, ifTrueStatement, ElseClause(ifFalseStatement)));
                return assignmentVariable;

                StatementSyntax ProcessArmBody(Expression body)
                {
                    Check.DebugAssert(_liftedState.Statements.Count == 0, "_liftedExpressions.Count == 0");

                    var translatedBody = Translate(body, assignmentVariable);

                    // Usually we add an assignment for the variable.
                    // The exception is if the body was itself lifted out and the assignment lowering succeeded (nested conditionals) -
                    // in this case we get back the lowered assignment variable, and don't need the assignment (i = i)
                    if (translatedBody != assignmentVariable)
                    {
                        _liftedState.Statements.Add(ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                assignmentVariable,
                                translatedBody)));
                    }

                    var block = Block(_liftedState.Statements);

                    _liftedState.Statements.Clear();
                    return block;
                }
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        IfStatementSyntax TranslateConditionalStatement(ConditionalExpression conditional)
        {
            var ifTrue = Translate(conditional.IfTrue);
            var ifFalse = Translate(conditional.IfFalse);

            var ifTrueStatement = ProcessArmBody(ifTrue, isTrueArm: true);

            if (isFalseAbsent)
            {
                return IfStatement(test, ifTrueStatement);
            }

            var ifFalseStatement = ProcessArmBody(ifFalse, isTrueArm: false);

            return IfStatement(test, ifTrueStatement, ElseClause(ifFalseStatement));

            StatementSyntax ProcessArmBody(SyntaxNode body, bool isTrueArm)
                => body switch
                {
                    BlockSyntax b => b,

                    // We want to specifically exempt IfStatementSyntax under the Else from being wrapped by a block, so as to get nice
                    // else if syntax
                    IfStatementSyntax i => isTrueArm ? Block(i) : i,

                    ExpressionSyntax e => Block(ExpressionStatement(e)),
                    StatementSyntax s => Block(s),

                    _ => throw new ArgumentOutOfRangeException()
                };
        }
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression constant)
    {
        Result = GenerateValue(constant.Value);

        return constant;

        ExpressionSyntax GenerateValue(object? value)
            => value switch
            {
                int or long or uint or ulong or short or sbyte or ushort or byte or double or float or decimal or char
                    => (ExpressionSyntax)_g.LiteralExpression(constant.Value),

                string or bool or null => (ExpressionSyntax)_g.LiteralExpression(constant.Value),

                Type t => TypeOfExpression(Translate(t)),
                Enum e => HandleEnum(e),

                ITuple tuple
                    when tuple.GetType() is { IsGenericType: true } tupleType
                         && tupleType.Name.StartsWith("ValueTuple`", StringComparison.Ordinal)
                         && tupleType.Namespace == "System"
                    => HandleValueTuple(tuple),

                IEqualityComparer c
                    when c == StructuralComparisons.StructuralEqualityComparer
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Translate(typeof(StructuralComparisons)),
                        IdentifierName(nameof(StructuralComparisons.StructuralEqualityComparer))),

                CultureInfo cultureInfo when cultureInfo == CultureInfo.InvariantCulture
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Translate(typeof(CultureInfo)),
                        IdentifierName(nameof(CultureInfo.InvariantCulture))),

                CultureInfo cultureInfo when cultureInfo == CultureInfo.InstalledUICulture
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Translate(typeof(CultureInfo)),
                        IdentifierName(nameof(CultureInfo.InstalledUICulture))),

                CultureInfo cultureInfo when cultureInfo == CultureInfo.CurrentCulture
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Translate(typeof(CultureInfo)),
                        IdentifierName(nameof(CultureInfo.CurrentCulture))),

                CultureInfo cultureInfo when cultureInfo == CultureInfo.CurrentUICulture
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Translate(typeof(CultureInfo)),
                        IdentifierName(nameof(CultureInfo.CurrentUICulture))),

                CultureInfo cultureInfo when cultureInfo == CultureInfo.DefaultThreadCurrentCulture
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Translate(typeof(CultureInfo)),
                        IdentifierName(nameof(CultureInfo.DefaultThreadCurrentCulture))),

                CultureInfo cultureInfo when cultureInfo == CultureInfo.DefaultThreadCurrentUICulture
                    => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Translate(typeof(CultureInfo)),
                        IdentifierName(nameof(CultureInfo.DefaultThreadCurrentUICulture))),

                _ => throw new NotSupportedException(
                    $"Encountered a constant of unsupported type '{value.GetType().Name}'. Only primitive constant nodes are supported.")
            };

        ExpressionSyntax HandleEnum(Enum e)
        {
            var enumType = e.GetType();

            var formatted = Enum.Format(enumType, e, "G");
            if (char.IsDigit(formatted[0]))
            {
                // Unknown value, render as a cast of the underlying integral value
                if (!Enum.IsDefined(e.GetType(), e))
                {
                    var underlyingType = enumType.GetEnumUnderlyingType();

                    return CastExpression(
                        Translate(enumType),
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            underlyingType == typeof(sbyte)
                            || underlyingType == typeof(short)
                            || underlyingType == typeof(int)
                            || underlyingType == typeof(long)
                                ? Literal(long.Parse(formatted))
                                : Literal(ulong.Parse(formatted))));
                }
            }

            var components = formatted.Split(", ");
            Check.DebugAssert(components.Length > 0, "components.Length > 0");

            return components.Aggregate(
                (ExpressionSyntax?)null,
                (last, next) => last is null
                    ? MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(enumType.Name),
                        IdentifierName(next))
                    : BinaryExpression(
                        SyntaxKind.BitwiseOrExpression, last,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(enumType.Name),
                            IdentifierName(next))))!;
        }

        ExpressionSyntax HandleValueTuple(ITuple tuple)
        {
            var arguments = new ArgumentSyntax[tuple.Length];
            for (var i = 0; i < tuple.Length; i++)
            {
                arguments[i] = Argument(GenerateValue(tuple[i]!));
            }

            return TupleExpression(SeparatedList(arguments));
        }
    }

    /// <inheritdoc />
    protected override Expression VisitDebugInfo(DebugInfoExpression node)
        => throw new NotSupportedException("DebugInfo nodes are not supporting when translating expression trees to C#");

    /// <inheritdoc />
    protected override Expression VisitDefault(DefaultExpression node)
    {
        Result = DefaultExpression(Translate(node.Type));

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitGoto(GotoExpression gotoNode)
    {
        Result = GotoStatement(SyntaxKind.GotoStatement, TranslateLabelTarget(gotoNode.Target));
        return gotoNode;
    }

    /// <inheritdoc />
    protected override Expression VisitInvocation(InvocationExpression invocation)
    {
        var lambda = (LambdaExpression)invocation.Expression;

        // We need to inline the lambda invocation into the tree, by replacing parameters in the lambda body with the invocation arguments.
        // However, if an argument to the invocation can have side effects (e.g. a method call), and it's referenced multiple times from
        // the body, then that would cause multiple evaluation, which is wrong (same if the arguments are evaluated only once but in reverse
        // order).
        // So we have to lift such arguments.
        var arguments = new Expression[invocation.Arguments.Count];

        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = invocation.Arguments[i];

            if (argument is ConstantExpression)
            {
                // No need to evaluate into a separate variable, just pass directly
                arguments[i] = argument;
                continue;
            }

            // Need to lift
            var name = UniquifyVariableName(lambda.Parameters[i].Name ?? "lifted");
            var parameter = E.Parameter(argument.Type, name);
            _liftedState.Statements.Add(GenerateVarDeclaration(name, Translate<ExpressionSyntax>(argument)));
            arguments[i] = parameter;
        }

        var replacedBody = new ReplacingExpressionVisitor(lambda.Parameters, arguments).Visit(lambda.Body);
        Result = Translate(replacedBody);

        return invocation;
    }

    /// <inheritdoc />
    protected override Expression VisitLabel(LabelExpression label)
    {
        // C# labels apply on a statement, but in LINQ they can appear anywhere (i.e. last thing in a block).
        // So we apply the label to a dummy null literal statement, which we'll filter out of the block in statement context anyway.
        Result = LabeledStatement(
            TranslateLabelTarget(label.Target).Identifier.Text,
            ExpressionStatement(LiteralExpression(SyntaxKind.NullLiteralExpression)));
        return label;
    }

    /// <inheritdoc />
    protected override LabelTarget VisitLabelTarget(LabelTarget? labelTarget)
    {
        if (labelTarget is null)
        {
            throw new NotImplementedException("Null argument in VisitLabelTarget");
        }

        Result = TranslateLabelTarget(labelTarget);
        return labelTarget;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IdentifierNameSyntax TranslateLabelTarget(LabelTarget labelTarget)
    {
        // In LINQ expression trees, label targets can have a return type (they're expressions), which means they return the last evaluated
        // thing if e.g. they're the last expression in a block. This would require lifting out the last evaluation before the goto/break,
        // assigning it to a temporary variable, and adding a variable evaluation after the label.
        if (labelTarget.Type != typeof(void))
        {
            throw new NotImplementedException("Non-void label target");
        }

        // We did a processing pass on the block's labels, so any labels should already be found in our label stack frame
        return IdentifierName(_stack.Peek().Labels[labelTarget]);
    }

    private TypeSyntax Translate(Type type)
    {
        if (type.IsGenericType)
        {
            return GenericName(
                Identifier(type.Name.Substring(0, type.Name.IndexOf('`'))),
                TypeArgumentList(SeparatedList(type.GenericTypeArguments.Select(Translate))));
        }

        if (type == typeof(string))
        {
            return PredefinedType(Token(SyntaxKind.StringKeyword));
        }

        if (type == typeof(bool))
        {
            return PredefinedType(Token(SyntaxKind.BoolKeyword));
        }

        if (type == typeof(byte))
        {
            return PredefinedType(Token(SyntaxKind.ByteKeyword));
        }

        if (type == typeof(sbyte))
        {
            return PredefinedType(Token(SyntaxKind.SByteKeyword));
        }

        if (type == typeof(int))
        {
            return PredefinedType(Token(SyntaxKind.IntKeyword));
        }

        if (type == typeof(uint))
        {
            return PredefinedType(Token(SyntaxKind.UIntKeyword));
        }

        if (type == typeof(short))
        {
            return PredefinedType(Token(SyntaxKind.ShortKeyword));
        }

        if (type == typeof(ushort))
        {
            return PredefinedType(Token(SyntaxKind.UShortKeyword));
        }

        if (type == typeof(long))
        {
            return PredefinedType(Token(SyntaxKind.LongKeyword));
        }

        if (type == typeof(ulong))
        {
            return PredefinedType(Token(SyntaxKind.ULongKeyword));
        }

        if (type == typeof(float))
        {
            return PredefinedType(Token(SyntaxKind.FloatKeyword));
        }

        if (type == typeof(double))
        {
            return PredefinedType(Token(SyntaxKind.DoubleKeyword));
        }

        if (type == typeof(decimal))
        {
            return PredefinedType(Token(SyntaxKind.DecimalKeyword));
        }

        if (type == typeof(char))
        {
            return PredefinedType(Token(SyntaxKind.CharKeyword));
        }

        if (type == typeof(object))
        {
            return PredefinedType(Token(SyntaxKind.ObjectKeyword));
        }

        if (type == typeof(void))
        {
            return PredefinedType(Token(SyntaxKind.VoidKeyword));
        }

        if (type.IsNested)
        {
            return QualifiedName(
                (NameSyntax)Translate(type.DeclaringType!),
                IdentifierName(type.Name));
        }

        if (type.Namespace != null)
        {
            _collectedNamespaces.Add(type.Namespace);
        }

        return IdentifierName(type.Name);
    }

    /// <inheritdoc />
    protected override Expression VisitLambda<T>(Expression<T> lambda)
    {
        using var _ = ChangeContext(
            lambda.ReturnType == typeof(void) ? ExpressionContext.Statement : ExpressionContext.ExpressionLambda);
        var parentOnLastLambdaLine = _onLastLambdaLine;
        _onLastLambdaLine = true;

        var stackFrame = PushNewStackFrame();

        var localUnnamedParameterCounter = 0;
        foreach (var parameter in lambda.Parameters)
        {
            var name = parameter.Name ?? "unnamed" + (++localUnnamedParameterCounter);
            stackFrame.Variables[parameter] = name;
            stackFrame.VariableNames.Add(name);
        }

        var body = (CSharpSyntaxNode)Translate(lambda.Body);

        // If the lambda body was an expression that had lifted statements (e.g. some block in expression context), we need to create
        // a block to contain these statements
        if (_liftedState.Statements.Count > 0)
        {
            Check.DebugAssert(lambda.ReturnType != typeof(void), "lambda.ReturnType != typeof(void)");

            body = Block(_liftedState.Statements.Append(ReturnStatement((ExpressionSyntax)body)));
            _liftedState.Statements.Clear();
        }

        // Note that we always explicitly include the parameters' types.
        // This is because in some cases, the parameter isn't actually used in the lambda body, and the compiler can't infer its type.
        // However, we can't do that when the type is anonymous.
        Result = ParenthesizedLambdaExpression(
            ParameterList(SeparatedList(lambda.Parameters.Select(p =>
                Parameter(Identifier(LookupVariableName(p)))
                    .WithType(p.Type.IsAnonymousType() ? null : Translate(p.Type))))),
            body);

        var popped = _stack.Pop();
        Check.DebugAssert(popped.Equals(stackFrame), "popped.Equals(stackFrame)");

        _onLastLambdaLine = parentOnLastLambdaLine;

        return lambda;
    }

    /// <inheritdoc />
    protected override Expression VisitLoop(LoopExpression loop)
    {
        if (_context == ExpressionContext.Expression)
        {
            throw new NotImplementedException();
        }

        var rewrittenLoop1 = loop;

        if (loop.ContinueLabel is not null)
        {
            var blockBody = loop.Body is BlockExpression b ? b : E.Block(loop.Body);
            blockBody = blockBody.Update(
                blockBody.Variables,
                new[] { E.Label(loop.ContinueLabel) }.Concat(blockBody.Expressions));

            rewrittenLoop1 = loop.Update(
                loop.BreakLabel,
                continueLabel: null,
                blockBody);
        }

        Expression rewrittenLoop2 = rewrittenLoop1;

        if (loop.BreakLabel is not null)
        {
            rewrittenLoop2 =
                E.Block(
                    rewrittenLoop1.Update(breakLabel: null, rewrittenLoop1.ContinueLabel, rewrittenLoop1.Body),
                    E.Label(loop.BreakLabel));
        }

        if (rewrittenLoop2 != loop)
        {
            return Visit(rewrittenLoop2);
        }

        var translatedBody = Translate(loop.Body) switch
        {
            BlockSyntax b => b,
            StatementSyntax s => Block(s),
            ExpressionSyntax e => Block(ExpressionStatement(e)),
            _ => throw new ArgumentOutOfRangeException()
        };

        StatementSyntax translated = WhileStatement(
            LiteralExpression(SyntaxKind.TrueLiteralExpression),
            translatedBody);

        Result = translated;

        return loop;
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression member)
    {
        using var _ = ChangeContext(ExpressionContext.Expression);

        // LINQ expression trees can directly access private members, but C# code cannot; render (slow) reflection code that does the same
        // thing. Note that assignment to private members is handled in VisitBinary.
        // TODO: Replace this with a more efficient API for .NET 8.0.
        switch (member.Member)
        {
            case FieldInfo { IsPrivate: true } fieldInfo:
                if (member.Expression is null)
                {
                    throw new NotImplementedException("Private static field access");
                }

                if (member.Member.DeclaringType is null)
                {
                    throw new NotSupportedException("Private field without a declaring type: " + member.Member.Name);
                }

                Result = Translate(
                    E.Call(
                        E.Call(
                            E.Constant(member.Member.DeclaringType),
                            _typeGetFieldMethod ??= typeof(Type).GetMethod(
                                nameof(Type.GetField), new[] { typeof(string), typeof(BindingFlags) })!,
                            E.Constant(fieldInfo.Name),
                            E.Constant(BindingFlags.NonPublic | BindingFlags.Instance)),
                        _fieldGetValueMethod ??= typeof(FieldInfo).GetMethod(nameof(FieldInfo.GetValue), new[] { typeof(object) })!,
                        member.Expression));

                break;

            // TODO: private property
            // TODO: private event

            default:
                Result = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    member.Expression is null
                        ? Translate(member.Member.DeclaringType!) // static
                        : Translate<ExpressionSyntax>(member.Expression),
                    IdentifierName(member.Member.Name));
                break;
        }

        return member;
    }

    /// <inheritdoc />
    protected override Expression VisitIndex(IndexExpression index)
    {
        using var _ = ChangeContext(ExpressionContext.Expression);

        if (index.Arguments.Count > 1)
        {
            throw new NotImplementedException("IndexExpression with multiple arguments");
        }

        Result =
            ElementAccessExpression(Translate<ExpressionSyntax>(index.Object!))
                .WithArgumentList(
                    BracketedArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                Translate<ExpressionSyntax>(index.Arguments.Single())))));

        return index;
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression call)
    {
        if (call.Method.DeclaringType is null)
        {
            throw new NotSupportedException($"Can't translate method '{call.Method.Name}' which has no declaring type");
        }

        using var _ = ChangeContext(ExpressionContext.Expression);

        var arguments = TranslateMethodArguments(call.Method.GetParameters(), call.Arguments);

        // For generic methods, we check whether the generic type arguments are inferrable (e.g. they all appear in the parameters), and
        // only explicitly specify the arguments if not. Note that this isn't just for prettier code: anonymous types cannot be explicitly
        // named in code.
        SimpleNameSyntax methodIdentifier;
        if (!call.Method.IsGenericMethod || GenericTypeParameterAreInferrable())
        {
            methodIdentifier = IdentifierName(call.Method.Name);
        }
        else
        {
            Check.DebugAssert(
                call.Method.GetGenericArguments().All(ga => !ga.IsAnonymousType()),
                "Anonymous type as generic type argument for method whose type arguments aren't inferrable");

            methodIdentifier = GenericName(
                Identifier(call.Method.Name),
                TypeArgumentList(
                    SeparatedList(
                        call.Method.GetGenericArguments().Select(Translate))));
        }

        // Extension syntax
        if (call.Method.IsDefined(typeof(ExtensionAttribute), inherit: false)
            && !(arguments[0].Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression)))
        {
            Result = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    arguments[0].Expression,
                    methodIdentifier),
                ArgumentList(SeparatedList(arguments[1..])));
        }
        else if (call.Method is { Name: "op_Equality", IsHideBySig: true, IsSpecialName: true })
        {
            Result = BinaryExpression(
                SyntaxKind.EqualsExpression,
                Translate<ExpressionSyntax>(call.Arguments[0]),
                Translate<ExpressionSyntax>(call.Arguments[1]));
        }
        else
        {
            ExpressionSyntax expression;
            if (call.Object is null)
            {
                // Static method call. Recursively add MemberAccessExpressions for all declaring types (for methods on nested types)
                expression = GetMemberAccessesForAllDeclaringTypes(call.Method.DeclaringType);

                ExpressionSyntax GetMemberAccessesForAllDeclaringTypes(Type type)
                {
                    return type.DeclaringType is null
                        ? Translate(type)
                        : MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            GetMemberAccessesForAllDeclaringTypes(type.DeclaringType),
                            IdentifierName(type.Name));
                }
            }
            else
            {
                expression = Translate<ExpressionSyntax>(call.Object);
            }

            if (call.Method.Name.StartsWith("get_", StringComparison.Ordinal)
                     && call.Method.GetParameters().Length == 1
                     && call.Method is { IsHideBySig: true, IsSpecialName: true })
            {
                Result = ElementAccessExpression(
                    expression,
                    BracketedArgumentList(SeparatedList(arguments)));
            }
            else
            {
                Result = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression,
                        methodIdentifier),
                    ArgumentList(SeparatedList(arguments)));
            }
        }

        if (call.Method.DeclaringType.Namespace is { } ns)
        {
            _collectedNamespaces.Add(ns);
        }

        return call;

        bool GenericTypeParameterAreInferrable()
        {
            var originalDefinition = call.Method.GetGenericMethodDefinition();
            var unseenTypeParameters = originalDefinition.GetGenericArguments().ToList();

            foreach (var parameter in originalDefinition.GetParameters())
            {
                ProcessType(parameter.ParameterType);
            }

            return unseenTypeParameters.Count == 0;

            void ProcessType(Type type)
            {
                if (type.IsGenericParameter)
                {
                    unseenTypeParameters.Remove(type);
                }
                else if (type.IsGenericType)
                {
                    foreach (var genericArgument in type.GetGenericArguments())
                    {
                        ProcessType(genericArgument);
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression newArray)
    {
        using var _ = ChangeContext(ExpressionContext.Expression);

        var elementType = Translate(newArray.Type.GetElementType()!);
        var expressions = TranslateList(newArray.Expressions);

        if (newArray.NodeType == ExpressionType.NewArrayBounds)
        {
            Result =
                ArrayCreationExpression(
                    ArrayType(
                        elementType,
                        SingletonList(ArrayRankSpecifier(SeparatedList(expressions)))));

            return newArray;
        }

        Check.DebugAssert(newArray.NodeType == ExpressionType.NewArrayInit, "newArray.NodeType == ExpressionType.NewArrayInit");

        Result = _g.ArrayCreationExpression(elementType, expressions);

        return newArray;
    }

    /// <inheritdoc />
    protected override Expression VisitNew(NewExpression node)
    {
        using var _ = ChangeContext(ExpressionContext.Expression);

        var arguments = node.Constructor is null
            ? Array.Empty<ArgumentSyntax>()
            : TranslateMethodArguments(node.Constructor.GetParameters(), node.Arguments);

        if (node.Type.IsAnonymousType())
        {
            if (node.Members is null)
            {
                throw new NotSupportedException("Anonymous type creation without members");
            }

            Result = AnonymousObjectCreationExpression(
                SeparatedList(
                    arguments.Select(
                            (arg, i) =>
                                AnonymousObjectMemberDeclarator(NameEquals(node.Members[i].Name), arg.Expression))
                        .ToArray()));

            return node;
        }

        // If the type has any required properties and the constructor doesn't have [SetsRequiredMembers], we can't just generate an
        // instantiation expression.
        // TODO: Currently matching attributes by name since we target .NET 6.0. If/when we target .NET 7.0 and above, match the type.
        if (node.Type.GetCustomAttributes(inherit: true)
                .Any(a => a.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute")
            && node.Constructor is not null
            && node.Constructor.GetCustomAttributes()
                .Any(a => a.GetType().FullName == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute") != true)
        {
            // If the constructor is parameterless, we generate Activator.Create<T>() which is almost as fast (<10ns difference).
            // For constructors with parameters, we currently throw as not supported (we can pass parameters, but boxing, probably
            // speed degradation etc.).
            if (node.Constructor.GetParameters().Length == 0)
            {
                Result =
                    Translate(
                        E.Call(
                            (_activatorCreateInstanceMethod ??= typeof(Activator).GetMethod(
                                nameof(Activator.CreateInstance), Array.Empty<Type>())!)
                            .MakeGenericMethod(node.Type)));
            }
            else
            {
                throw new NotImplementedException("Instantiation of type with required properties via constructor that has parameters");
            }
        }
        else
        {
            // Normal case with plain old instantiation
            Result = ObjectCreationExpression(
                Translate(node.Type),
                ArgumentList(SeparatedList(arguments)),
                initializer: null);
        }

        if (node.Constructor?.DeclaringType?.Namespace is not null)
        {
            _collectedNamespaces.Add(node.Constructor.DeclaringType.Namespace);
        }

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression parameter)
    {
        // Note that the parameter in the lambda declaration is handled separately in VisitLambda
        if (_stack.Peek().Variables.TryGetValue(parameter, out var name)
            || _liftedState.Variables.TryGetValue(parameter, out name))
        {
            Result = IdentifierName(name);

            return parameter;
        }

        // This parameter is unknown to us - it's captured from outside the entire expression tree.
        // Simply return its name without worrying about uniquification, since the variable needs to correspond to the outside in any
        // case (it's the callers responsibility).
        _capturedVariables.Add(parameter);

        if (parameter.Name is null)
        {
            throw new NotSupportedException("Unnamed captured variable");
        }

        Result = IdentifierName(parameter.Name);
        return parameter;
    }

    /// <inheritdoc />
    protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        => throw new NotSupportedException();

    /// <inheritdoc />
    protected override SwitchCase VisitSwitchCase(SwitchCase node)
        => throw new NotSupportedException("Translation happens as part of VisitSwitch");

    /// <inheritdoc />
    protected override Expression VisitSwitch(SwitchExpression switchNode)
    {
        Result = TranslateSwitch(switchNode, lowerableAssignmentVariable: null);

        return switchNode;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual CSharpSyntaxNode TranslateSwitch(SwitchExpression switchNode, IdentifierNameSyntax? lowerableAssignmentVariable)
    {
        if (switchNode.Comparison is not null)
        {
            throw new NotImplementedException("Switch with non-null comparison method");
        }

        var switchValue = Translate<ExpressionSyntax>(switchNode.SwitchValue);

        switch (_context)
        {
            case ExpressionContext.Statement:
            {
                var parentLiftedState = _liftedState;
                _liftedState = new(new(), new(), new(), new());

                var cases = List(
                    switchNode.Cases.Select(
                        c => SwitchSection(
                            labels: List<SwitchLabelSyntax>(
                                c.TestValues.Select(tv => CaseSwitchLabel(Translate<ExpressionSyntax>(tv)))),
                            statements: ProcessArmBody(c.Body))));

                // LINQ SwitchExpression supports non-literal labels, which C# does not support. This rewrites the switch as a series of
                // nested ConditionalExpressions.
                if (cases.Any(c => c.Labels.Any(l => l is CaseSwitchLabelSyntax l2 && !_constantDetector.IsConstant(l2.Value))))
                {
                    _liftedState = parentLiftedState;
                    return TranslateConditional(RewriteSwitchToConditionals(switchNode), lowerableAssignmentVariable);
                }

                if (switchNode.DefaultBody is not null)
                {
                    cases = cases.Add(
                        SwitchSection(
                            SingletonList<SwitchLabelSyntax>(DefaultSwitchLabel()),
                            ProcessArmBody(switchNode.DefaultBody)));
                }

                return SwitchStatement(switchValue, cases);

                SyntaxList<StatementSyntax> ProcessArmBody(Expression body)
                {
                    var translatedBody = Translate(body);

                    var result = translatedBody switch
                    {
                        BlockSyntax block => SingletonList<StatementSyntax>(block.WithStatements(block.Statements.Add(BreakStatement()))),
                        StatementSyntax s => List(new[] { s, BreakStatement() }),
                        ExpressionSyntax e => List(new StatementSyntax[] { ExpressionStatement(e), BreakStatement() }),

                        _ => throw new ArgumentOutOfRangeException()
                    };

                    return result;
                }
            }

            case ExpressionContext.Expression:
            case ExpressionContext.ExpressionLambda:
            {
                if (switchNode.DefaultBody is null)
                {
                    throw new NotSupportedException("Missing default arm for switch expression");
                }

                var parentLiftedState = _liftedState;
                _liftedState = new(new(), new(), new(), new());

                // Translate all arms
                var arms = SeparatedList(
                    switchNode.Cases.SelectMany(
                            c => c.TestValues, (c, tv) => SwitchExpressionArm(
                                ConstantPattern(Translate<ExpressionSyntax>(tv)),
                                Translate<ExpressionSyntax>(c.Body)))
                        .Append(SwitchExpressionArm(DiscardPattern(), Translate<ExpressionSyntax>(switchNode.DefaultBody))));

                // LINQ SwitchExpression supports non-literal labels, which C# does not support. This rewrites the switch as a series of
                // nested ConditionalExpressions.
                if (arms.Any(a => a.Pattern is ConstantPatternSyntax cp && !_constantDetector.IsConstant(cp.Expression)))
                {
                    _liftedState = parentLiftedState;
                    return TranslateConditional(RewriteSwitchToConditionals(switchNode), lowerableAssignmentVariable);
                }

                // If there were no lifted expressions inside any arm, we can translate directly to a C# switch expression
                if (_liftedState.Statements.Count == 0)
                {
                    _liftedState = parentLiftedState;
                    return SwitchExpression(switchValue, arms);
                }

                // There are lifted expressions inside some of the arms, we must lift the entire switch expression, rewriting it to
                // a switch statement.
                _liftedState = new(new(), new(), new(), new());

                IdentifierNameSyntax assignmentVariable;
                TypeSyntax? loweredAssignmentVariableType = null;

                if (lowerableAssignmentVariable is null)
                {
                    var name = UniquifyVariableName("liftedSwitch");
                    var parameter = E.Parameter(switchNode.Type, name);
                    assignmentVariable = IdentifierName(name);
                    loweredAssignmentVariableType = Translate(parameter.Type);
                }
                else
                {
                    assignmentVariable = lowerableAssignmentVariable;
                }

                var cases = List(
                    switchNode.Cases.Select(
                        c => SwitchSection(
                            labels: List<SwitchLabelSyntax>(
                                c.TestValues.Select(tv => CaseSwitchLabel(Translate<LiteralExpressionSyntax>(tv)))),
                            statements: ProcessArmBody(c.Body)))
                        .Append(SwitchSection(
                            SingletonList<SwitchLabelSyntax>(DefaultSwitchLabel()),
                            ProcessArmBody(switchNode.DefaultBody))));

                _liftedState = parentLiftedState;

                if (lowerableAssignmentVariable is null)
                {
                    _liftedState.Statements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(loweredAssignmentVariableType!)
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(assignmentVariable.Identifier.Text)))));
                }

                _liftedState.Statements.Add(SwitchStatement(switchValue, cases));
                return assignmentVariable;

                SyntaxList<StatementSyntax> ProcessArmBody(Expression body)
                {
                    Check.DebugAssert(_liftedState.Statements.Count == 0, "_liftedExpressions.Count == 0");

                    var translatedBody = Translate(body, assignmentVariable);

                    var assignmentStatement = ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            assignmentVariable,
                            translatedBody));

                    if (_liftedState.Statements.Count == 0)
                    {
                        // Simple expression, can embed directly in the switch case
                        return List(new StatementSyntax[] { assignmentStatement, BreakStatement() });
                    }

                    // Usually we add an assignment for the variable.
                    // The exception is if the body was itself lifted out and the assignment lowering succeeded (nested conditionals) -
                    // in this case we get back the lowered assignment variable, and don't need the assignment (i = i)
                    if (translatedBody != assignmentVariable)
                    {
                        _liftedState.Statements.Add(ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                assignmentVariable,
                                translatedBody)));
                    }

                    _liftedState.Statements.Add(BreakStatement());
                    var block = SingletonList<StatementSyntax>(Block(_liftedState.Statements));

                    _liftedState.Statements.Clear();
                    return block;
                }
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        static ConditionalExpression RewriteSwitchToConditionals(SwitchExpression node)
        {
            if (node.Type == typeof(void))
            {
                return (ConditionalExpression)(node.Cases
                           .SelectMany(c => c.TestValues, (c, tv) => new { c.Body, Label = tv })
                           .Reverse()
                           .Aggregate(
                               node.DefaultBody,
                               (expression, arm) => expression is null
                                   ? E.IfThen(E.Equal(node.SwitchValue, arm.Label), arm.Body)
                                   : E.IfThenElse(E.Equal(node.SwitchValue, arm.Label), arm.Body, expression))
                       ?? throw new NotImplementedException("Empty switch statement"));
            }

            Check.DebugAssert(node.DefaultBody is not null, "Switch expression with non-void return type but no default body");

            return (ConditionalExpression)node.Cases
                .SelectMany(c => c.TestValues, (c, tv) => new { c.Body, Label = tv })
                .Reverse()
                .Aggregate(
                    node.DefaultBody,
                    (expression, arm) => E.Condition(
                        E.Equal(node.SwitchValue, arm.Label),
                        arm.Body,
                        expression));
        }
    }

    /// <inheritdoc />
    protected override Expression VisitTry(TryExpression tryNode)
    {
        var translatedBody = Translate(tryNode.Body) switch
        {
            BlockSyntax b => (IEnumerable<SyntaxNode>)b.Statements,
            var n => new[] { n }
        };

        var translatedFinally = Translate(tryNode.Finally) switch
        {
            BlockSyntax b => (IEnumerable<SyntaxNode>)b.Statements,
            null => null,
            var n => new[] { n }
        };

        switch (_context)
        {
            case ExpressionContext.Statement:
                if (tryNode.Fault is not null)
                {
                    Check.DebugAssert(
                        tryNode.Finally is null && tryNode.Handlers.Count == 0,
                        "tryNode.Finally is null && tryNode.Handlers.Count == 0");

                    Result = _g.TryCatchStatement(
                        translatedBody,
                        catchClauses: new[] { TranslateCatchBlock(E.Catch(typeof(Exception), tryNode.Fault), noType: true) });

                    return tryNode;
                }

                Result = _g.TryCatchStatement(
                    translatedBody,
                    catchClauses: tryNode.Handlers.Select(h => TranslateCatchBlock(h)),
                    translatedFinally);

                return tryNode;

            case ExpressionContext.Expression:
            case ExpressionContext.ExpressionLambda:
                throw new NotImplementedException();

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <inheritdoc />
    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        var visitedExpression = Translate<ExpressionSyntax>(node.Expression);

        Result = node.NodeType switch
        {
            ExpressionType.TypeIs
                => BinaryExpression(SyntaxKind.IsExpression, visitedExpression, Translate(node.TypeOperand)),

            ExpressionType.TypeEqual
                => BinaryExpression(SyntaxKind.EqualsExpression, visitedExpression, TypeOfExpression(Translate(node.TypeOperand))),

            _ => throw new ArgumentOutOfRangeException()
        };

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression unary)
    {
        if (unary.Method is not null
            && !unary.Method.IsHideBySig
            && !unary.Method.IsSpecialName
            && unary.Method.Name != "op_Implicit"
            && unary.Method.Name != "op_Explicit")
        {
            throw new NotImplementedException("Unary node with non-null method");
        }

        using var _ = ChangeContext(ExpressionContext.Expression);

        var operand = Translate<ExpressionSyntax>(unary.Operand);

        // TODO: Confirm what to do with the checked expression types

        Result = unary.NodeType switch
        {
            ExpressionType.Negate => _g.NegateExpression(operand),
            ExpressionType.NegateChecked => _g.NegateExpression(operand),
            ExpressionType.Not when unary.Type == typeof(bool) => _g.LogicalNotExpression(operand),
            ExpressionType.Not => _g.BitwiseNotExpression(operand),
            ExpressionType.OnesComplement => _g.BitwiseNotExpression(operand),
            ExpressionType.IsFalse => _g.LogicalNotExpression(operand),
            ExpressionType.IsTrue => operand,
            ExpressionType.ArrayLength => _g.MemberAccessExpression(operand, "Length"),
            ExpressionType.Convert => ParenthesizedExpression((ExpressionSyntax)_g.ConvertExpression(Translate(unary.Type), operand)),
            ExpressionType.ConvertChecked => ParenthesizedExpression((ExpressionSyntax)_g.ConvertExpression(Translate(unary.Type), operand)),
            ExpressionType.Throw when unary.Type == typeof(void) => _g.ThrowStatement(operand),
            ExpressionType.Throw => _g.ThrowExpression(operand),
            ExpressionType.TypeAs => BinaryExpression(SyntaxKind.AsExpression, operand, Translate(unary.Type)),
            ExpressionType.Quote => operand,
            ExpressionType.UnaryPlus => PrefixUnaryExpression(SyntaxKind.UnaryPlusExpression, operand),
            ExpressionType.Unbox => operand,
            ExpressionType.Increment => Translate(E.Add(unary.Operand, E.Constant(1))),
            ExpressionType.Decrement => Translate(E.Subtract(unary.Operand, E.Constant(1))),
            ExpressionType.PostIncrementAssign => PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, operand),
            ExpressionType.PostDecrementAssign => PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, operand),
            ExpressionType.PreIncrementAssign => PrefixUnaryExpression(SyntaxKind.PreIncrementExpression, operand),
            ExpressionType.PreDecrementAssign => PrefixUnaryExpression(SyntaxKind.PreDecrementExpression, operand),

            _ => throw new ArgumentOutOfRangeException("Unsupported LINQ unary node: " + unary.NodeType)
        };

        return unary;
    }

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression memberInit)
    {
        var objectCreation = Translate<ObjectCreationExpressionSyntax>(memberInit.NewExpression);

        List<MemberListBinding>? incompatibleListBindings = null;

        var initializerExpressions = new List<AssignmentExpressionSyntax>(memberInit.Bindings.Count);

        foreach (var binding in memberInit.Bindings)
        {
            // C# collection initialization syntax only works when Add is called on an IEnumerable, but LINQ supports arbitrary add
            // methods. Skip these, we'll add them later outside the initializer
            if (binding is MemberListBinding listBinding
                && (!listBinding.Member.GetMemberType().IsAssignableTo(typeof(IEnumerable))
                    || listBinding.Initializers.Any(e => e.AddMethod.Name != "Add" || e.Arguments.Count != 1)))
            {
                incompatibleListBindings ??= new();
                incompatibleListBindings.Add(listBinding);
                continue;
            }

            var liftedStatementsPosition = _liftedState.Statements.Count;

            VisitMemberBinding(binding);

            initializerExpressions.Add((AssignmentExpressionSyntax)Result!);

            if (_liftedState.Statements.Count > liftedStatementsPosition)
            {
                // TODO: This is tricky because of the recursive nature of MemberMemberBinding
                throw new NotImplementedException("MemberInit: lifted statements");
            }
        }

        if (incompatibleListBindings is not null)
        {
            // TODO: Lift the instantiation and add extra statements to add the incompatible bindings after that
            throw new NotImplementedException("MemberInit: incompatible MemberListBinding");
        }

        Result = objectCreation.WithInitializer(
            InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList<ExpressionSyntax>(initializerExpressions)));

        return memberInit;
    }

    /// <inheritdoc />
    protected override Expression VisitListInit(ListInitExpression listInit)
    {
        var objectCreation = Translate<ObjectCreationExpressionSyntax>(listInit.NewExpression);

        List<ElementInit>? incompatibleListBindings = null;

        var initializerExpressions = new List<ExpressionSyntax>(listInit.Initializers.Count);

        foreach (var initializer in listInit.Initializers)
        {
            // C# collection initialization syntax only works when Add is called on an IEnumerable, but LINQ supports arbitrary add
            // methods. Skip these, we'll add them later outside the initializer
            if (!listInit.NewExpression.Type.IsAssignableTo(typeof(IEnumerable))
                || listInit.Initializers.Any(e => e.AddMethod.Name != "Add" || e.Arguments.Count != 1))
            {
                incompatibleListBindings ??= new();
                incompatibleListBindings.Add(initializer);
                continue;
            }

            var liftedStatementsPosition = _liftedState.Statements.Count;

            VisitElementInit(initializer);

            initializerExpressions.Add((ExpressionSyntax)Result!);

            if (_liftedState.Statements.Count > liftedStatementsPosition)
            {
                throw new NotImplementedException("ListInit: lifted statements");
            }
        }

        if (incompatibleListBindings is not null)
        {
            // TODO: This requires lifting statements to *after* the instantiation - we usually lift to before.
            // This is problematic: if such an expression is passed as an argument to a method, there's no way to faithfully translate it
            // while preserving evaluation order.
            throw new NotImplementedException("ListInit: incompatible ElementInit");
        }

        Result = objectCreation.WithInitializer(
            InitializerExpression(
                SyntaxKind.CollectionInitializerExpression,
                SeparatedList(initializerExpressions)));

        return listInit;
    }

    /// <inheritdoc />
    protected override ElementInit VisitElementInit(ElementInit elementInit)
    {
        Check.DebugAssert(elementInit.Arguments.Count == 1, "elementInit.Arguments.Count == 1");

        Visit(elementInit.Arguments.Single());

        return elementInit;
    }

    /// <inheritdoc />
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
    {
        Result = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(memberAssignment.Member.Name),
            Translate<ExpressionSyntax>(memberAssignment.Expression));

        return memberAssignment;
    }

    /// <inheritdoc />
    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding memberMemberBinding)
    {
        Result = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(memberMemberBinding.Member.Name),
            InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList(
                    memberMemberBinding.Bindings.Select(
                        b =>
                        {
                            VisitMemberBinding(b);
                            return (ExpressionSyntax)Result!;
                        }))));

        return memberMemberBinding;
    }

    /// <inheritdoc />
    protected override MemberListBinding VisitMemberListBinding(MemberListBinding memberListBinding)
    {
        Result = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(memberListBinding.Member.Name),
            InitializerExpression(
                SyntaxKind.CollectionInitializerExpression,
                SeparatedList(
                    memberListBinding.Initializers.Select(i =>
                    {
                        VisitElementInit(i);
                        return (ExpressionSyntax)Result!;
                    }))));

        return memberListBinding;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        // TODO: Remove any EF-specific code from this visitor (extend if needed)
        // TODO: Hack mode. Visit the expression beforehand to replace EntityQueryRootExpression with context.Set<>(), or receive it in this visitor as a replacement or something.
        if (node is EntityQueryRootExpression entityQueryRoot)
        {
            // TODO: STET
            Result = ParseExpression($"context.Set<{entityQueryRoot.EntityType.ClrType.Name}>()");
            return node;
        }

        throw new NotSupportedException(
            $"Encountered non-quotable expression of type {node.GetType()} when translating expression tree to C#");
    }

    private ArgumentSyntax[] TranslateMethodArguments(ParameterInfo[] parameters, IReadOnlyList<Expression> arguments)
    {
        var translatedExpressions = TranslateList(arguments);
        var translatedArguments = new ArgumentSyntax[arguments.Count];

        for (var i = 0; i < translatedExpressions.Length; i++)
        {
            var parameter = parameters[i];
            var argument = Argument(translatedExpressions[i]);

            if (parameter.IsOut)
            {
                argument = argument.WithRefKindKeyword(Token(SyntaxKind.OutKeyword));
            }
            else if (parameter.IsIn)
            {
                argument = argument.WithRefKindKeyword(Token(SyntaxKind.InKeyword));
            }
            else if (parameter.ParameterType.IsByRef)
            {
                argument = argument.WithRefKindKeyword(Token(SyntaxKind.RefKeyword));
            }

            translatedArguments[i] = argument;
        }

        return translatedArguments;
    }

    private ExpressionSyntax[] TranslateList(IReadOnlyList<Expression> list)
    {
        Check.DebugAssert(_context == ExpressionContext.Expression, "_context == ExpressionContext.Expression");

        var translatedList = new ExpressionSyntax[list.Count];
        var lastLiftedArgumentPosition = 0;

        for (var i = 0; i < list.Count; i++)
        {
            var expression = list[i];

            var liftedStatementsPosition = _liftedState.Statements.Count;

            var translated = Translate<ExpressionSyntax>(expression);

            if (_liftedState.Statements.Count > liftedStatementsPosition)
            {
                // This argument contained lifted statements. In order to preserve evaluation order, we must also lift out all preceding
                // arguments to before this argument's lifted statements.
                for (; lastLiftedArgumentPosition < i; lastLiftedArgumentPosition++)
                {
                    var argumentExpression = translatedList[lastLiftedArgumentPosition];

                    if (!_sideEffectDetector.CanBeReordered(argumentExpression, translated))
                    {
                        var name = UniquifyVariableName("liftedArg");

                        _liftedState.Statements.Insert(
                            liftedStatementsPosition++,
                            GenerateVarDeclaration(name, argumentExpression));
                        _liftedState.VariableNames.Add(name);

                        translatedList[lastLiftedArgumentPosition] = IdentifierName(name);
                    }
                }
            }

            translatedList[i] = translated;
        }

        return translatedList;
    }

    private StackFrame PushNewStackFrame()
    {
        var previousFrame = _stack.Peek();
        var newFrame = new StackFrame(
            new(previousFrame.Variables),
            new(previousFrame.VariableNames),
            new(previousFrame.Labels),
            new(previousFrame.UnnamedLabelNames));

        _stack.Push(newFrame);

        return newFrame;
    }

    private string LookupVariableName(ParameterExpression parameter)
        => _stack.Peek().Variables.TryGetValue(parameter, out var name)
            ? name
            : _liftedState.Variables[parameter];

    private string UniquifyVariableName(string? name)
    {
        var isUnnamed = name is null;
        name ??= "unnamed";

        var parameterNames = _stack.Peek().VariableNames;

        if (parameterNames.Contains(name) || _liftedState.VariableNames.Contains(name))
        {
            var baseName = name;
            for (var j = isUnnamed ? _unnamedParameterCounter++ : 0;
                 parameterNames.Contains(name) || _liftedState.VariableNames.Contains(name);
                 j++)
            {
                name = baseName + j;
            }
        }

        return name;
    }

    private static LocalDeclarationStatementSyntax GenerateVarDeclaration(string variableIdentifier, ExpressionSyntax initializer)
        => LocalDeclarationStatement(
            VariableDeclaration(
                IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())),
                SingletonSeparatedList(
                    VariableDeclarator(Identifier(variableIdentifier))
                        .WithInitializer(
                            EqualsValueClause(
                                initializer)))));

    private ContextChanger ChangeContext(ExpressionContext newContext)
        => new(this, newContext);

    private readonly struct ContextChanger : IDisposable
    {
        private readonly LinqToCSharpSyntaxTranslator _translator;
        private readonly ExpressionContext _oldContext;

        public ContextChanger(LinqToCSharpSyntaxTranslator translator, ExpressionContext newContext)
        {
            _translator = translator;
            _oldContext = translator._context;
            translator._context = newContext;
        }

        public void Dispose()
            => _translator._context = _oldContext;
    }

    private enum ExpressionContext
    {
        Expression,
        Statement,
        ExpressionLambda
    }

    private sealed class ConstantDetectionSyntaxWalker : SyntaxWalker
    {
        private bool _isConstant;

        public bool IsConstant(SyntaxNode node)
        {
            _isConstant = true;

            Visit(node);

            return _isConstant;
        }

        public override void Visit(SyntaxNode node)
        {
            _isConstant &= IsConstantCore(node);

            base.Visit(node);
        }

        private static bool IsConstantCore(SyntaxNode node)
            => node switch
            {
                LiteralExpressionSyntax => true,

                // Binary/unary expressions over constants are also constant
                BinaryExpressionSyntax or PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax => true,

                _ => false
            };
    }

    private sealed class SideEffectDetectionSyntaxWalker : SyntaxWalker
    {
        private bool _mayHaveSideEffects;

        /// <summary>
        ///     Returns whether the two provided nodes can be re-ordered without the reversed evaluation order having any effect.
        ///     For example, two literal expressions can be safely ordered, while two invocations cannot.
        /// </summary>
        public bool CanBeReordered(SyntaxNode first, SyntaxNode second)
            => first is LiteralExpressionSyntax || (!MayHaveSideEffects(first) && !MayHaveSideEffects(second));

        public bool MayHaveSideEffects(SyntaxNode node)
        {
            _mayHaveSideEffects = false;

            Visit(node);

            return _mayHaveSideEffects;
        }

        public override void Visit(SyntaxNode node)
        {
            _mayHaveSideEffects |= MayHaveSideEffectsCore(node);

            base.Visit(node);
        }

        private static bool MayHaveSideEffectsCore(SyntaxNode node)
            => node switch
            {
                IdentifierNameSyntax or LiteralExpressionSyntax => false,
                ExpressionStatementSyntax e => MayHaveSideEffectsCore(e.Expression),
                EmptyStatementSyntax => false,

                // TODO: we can exempt most binary and unary expressions as well, e.g. i + 5, but not anything involving assignment
                _ => true
            };
    }
}
