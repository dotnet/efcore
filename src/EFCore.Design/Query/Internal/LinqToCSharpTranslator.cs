// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
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
public class LinqToCSharpTranslator : ExpressionVisitor, ILinqToCSharpTranslator
{
    private record StackFrame(
        Dictionary<ParameterExpression, string> Variables,
        HashSet<string> VariableNames,
        Dictionary<LabelTarget, string> Labels,
        HashSet<string> UnnamedLabelNames);

    private readonly Stack<StackFrame> _stack
        = new(new[] { new StackFrame(new(), new(), new(), new()) });

    private int _unnamedParameterCounter;

    private record LiftedState(
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
    public LinqToCSharpTranslator(SyntaxGenerator syntaxGenerator)
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
    protected SyntaxNode Result { get; set; } = null!;

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

        return Result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SyntaxNode Translate(Expression? node)
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
    protected virtual T Translate<T>(Expression node) where T : CSharpSyntaxNode
    {
        Visit(node);

        return Result as T
               ?? throw new InvalidOperationException(
                   $"Got translated node of type {Result?.GetType().Name} instead of the expected {typeof(T)}");
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
        => node is null ? null : base.Visit(node);

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
        if (_liftedState.Statements.Count > liftedStatementLeftPosition
            && liftedStatementLeftPosition == liftedStatementOrigPosition
            && _sideEffectDetector.MayHaveSideEffects(left))
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
                // Identify assignment where the RHS is a switch expression, and pass the LHS for possible lowering. If the switch
                // expression is lifted out (e.g. because some arm contains a block), this will lower the variable to be assigned inside
                // the resulting switch statement, rather then adding another useless temporary variable.
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

            // Now visit the expressions, applying any lifted expressions
            for (var i = 0; i < block.Expressions.Count; i++)
            {
                var expression = block.Expressions[i];
                var onLastBlockLine = i == block.Expressions.Count - 1;
                _onLastLambdaLine = parentOnLastLambdaLine && onLastBlockLine;

                // Any lines before the last are evaluated in statement context (they aren't returned); the last line is evaluated in the
                // context of the block as a whole. _context now refers to the statement's context, blockContext to the block's.
                var statementContext = onLastBlockLine ? _context : ExpressionContext.Statement;

                SyntaxNode translated;
                using (ChangeContext(onLastBlockLine ? _context : ExpressionContext.Statement))
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
                // ... except for the last line, where we just return the value if needed.
                if (expression is BinaryExpression { NodeType: ExpressionType.Assign, Left: ParameterExpression lValue }
                    && translated is AssignmentExpressionSyntax { Right: var valueSyntax }
                    && (!onLastBlockLine || statementContext == ExpressionContext.Statement)
                    && unassignedVariables.Remove(lValue))
                {
                    var useExplicitVariableType = valueSyntax.Kind() == SyntaxKind.NullLiteralExpression;

                    translated = LocalDeclarationStatement(
                        VariableDeclaration(
                            useExplicitVariableType
                                ? lValue.Type.GetTypeSyntax()
                                : IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())),
                            SingletonSeparatedList(
                                VariableDeclarator(Identifier(LookupVariableName(lValue)))
                                    .WithInitializer(EqualsValueClause(valueSyntax)))));
                }

                if (statementContext == ExpressionContext.Expression)
                {
                    // We're on the last line of a block in expression context - the block is being lifted out.
                    // All statements before the last line (this one) have already been added to _liftedStatements, just return the last
                    // E.
                    Check.DebugAssert(onLastBlockLine, "onLastBlockLine");
                    Result = translated;
                    break;
                }

                if (blockContext != ExpressionContext.Expression)
                {
                    if (_liftedState.Statements.Count > 0)
                    {
                        // If any expressions were lifted out of the current expression, flatten them into our own block, just before the
                        // statement from which it was lifted. Note that we don't do this in Expression context, since our own block is
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
                    // TODO: We can also elide expressions with no side effects in stand-alone context
                    ExpressionSyntax e
                        when statementContext == ExpressionContext.Statement
                        && !IsExpressionValidAsStatement(e)
                        => ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(Identifier(TriviaList(), SyntaxKind.UnderscoreToken, "_", "_", TriviaList())),
                                e)),

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

            // If a label existed on the last line, add an empty statement (since C# requires it); for expression blocks we'd have to
            // lift that, not supported for now.
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
                    v => LocalDeclarationStatement(
                        VariableDeclaration(
                            v.Type.GetTypeSyntax(),
                            SingletonSeparatedList(
                                VariableDeclarator(Identifier(LookupVariableName(v)))))));

            if (blockContext == ExpressionContext.Expression)
            {
                _liftedState.UnassignedVariableDeclarations.AddRange(unassignedVariableDeclarations);
            }
            else
            {
                statements.InsertRange(0, unassignedVariableDeclarations.Concat(_liftedState.UnassignedVariableDeclarations));
                _liftedState.UnassignedVariableDeclarations.Clear();

                // We're done. If the block is in an expression context, it needs to be lifted out; but not if it's in a lambda (in that case we
                // just added return above).
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
    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
        throw new NotImplementedException();
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
    protected virtual CSharpSyntaxNode TranslateConditional(ConditionalExpression conditional, IdentifierNameSyntax? lowerableAssignmentVariable)
    {
        // ConditionalExpression can be an expression or an if/else statement.
        var test = Translate<ExpressionSyntax>(conditional.Test);

        var isFalseAbsent = conditional.IfFalse is DefaultExpression defaultIfFalse && defaultIfFalse.Type == typeof(void);

        switch (_context)
        {
            case ExpressionContext.Statement:
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

                var ifTrue = Translate(conditional.IfTrue);
                var ifFalse = Translate(conditional.IfFalse);

                if (ifTrue is not ExpressionSyntax ifTrueExpression
                    || ifFalse is not ExpressionSyntax ifFalseExpression)
                {
                    throw new NotSupportedException("Trying to evaluate a non-expression condition in expression context");
                }

                // There were no lifted expressions inside either arm - we can translate directly to a C# conditional expression
                if (_liftedState.Statements.Count == 0)
                {
                    _liftedState = parentLiftedState;
                    return ConditionalExpression(test, ifTrueExpression, ifFalseExpression);
                }

                // There are lifted expressions inside one of the arms, we must lift the entire conditional expression, rewriting it to
                // a an if/else statement.
                _liftedState = new(new(), new(), new(), new());

                IdentifierNameSyntax assignmentVariable;
                TypeSyntax? loweredAssignmentVariableType = null;

                if (lowerableAssignmentVariable is null)
                {
                    var name = UniquifyVariableName("liftedConditional");
                    var parameter = E.Parameter(conditional.Type, name);
                    assignmentVariable = IdentifierName(name);
                    loweredAssignmentVariableType = parameter.Type.GetTypeSyntax();
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
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression constant)
    {
        Result = GenerateValue(constant.Value);

        return constant;

        ExpressionSyntax GenerateValue(object? value)
            => value switch
            {
                int or long or uint or ulong or short or sbyte or ushort or byte or double or float or decimal
                    => (ExpressionSyntax)_g.LiteralExpression(constant.Value),

                string or bool or null => (ExpressionSyntax)_g.LiteralExpression(constant.Value),

                Type t => TypeOfExpression(t.GetTypeSyntax()),
                Enum e => HandleEnum(e),

                ITuple tuple
                    when tuple.GetType() is { IsGenericType: true } tupleType
                         && tupleType.Name.StartsWith("ValueTuple`", StringComparison.Ordinal)
                         && tupleType.Namespace == "System"
                    => HandleValueTuple(tuple),

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
                        enumType.GetTypeSyntax(),
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
        Result = DefaultExpression(node.Type.GetTypeSyntax());

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
        // order.
        // So we have to lift such arguments.
        var arguments = new Expression[invocation.Arguments.Count];

        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = invocation.Arguments[i];
            if (!MayHaveSideEffects(argument))
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

        Result = lambda.Parameters.Count switch
        {
            0 => ParenthesizedLambdaExpression(body),
            1 => SimpleLambdaExpression(Parameter(Identifier(stackFrame.Variables[lambda.Parameters[0]])), body),
            _ => ParenthesizedLambdaExpression(
                ParameterList(SeparatedList(lambda.Parameters.Select(p => Parameter(Identifier(LookupVariableName(p)))))),
                body)
        };

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
                        ? member.Type.GetTypeSyntax() // static
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

        var parameters = call.Method.GetParameters();
        var arguments = new ArgumentSyntax[parameters.Length];
        var lastLiftedArgumentPosition = 0;

        for (var i = 0; i < arguments.Length; i++)
        {
            var (argument, parameter) = (call.Arguments[i], parameters[i]);

            var liftedStatementsPosition = _liftedState.Statements.Count;

            var translated = Argument(Translate<ExpressionSyntax>(argument));

            if (parameter.IsOut)
            {
                translated = translated.WithRefKindKeyword(Token(SyntaxKind.OutKeyword));
            }
            else if (parameter.IsIn)
            {
                translated = translated.WithRefKindKeyword(Token(SyntaxKind.InKeyword));
            }
            else if (parameter.ParameterType.IsByRef)
            {
                translated = translated.WithRefKindKeyword(Token(SyntaxKind.RefKeyword));
            }

            if (_liftedState.Statements.Count > liftedStatementsPosition)
            {
                // This argument contained lifted statements. In order to preserve evaluation order, we must also lift out all preceding
                // arguments to before this argument's lifted statements.
                for (; lastLiftedArgumentPosition < i; lastLiftedArgumentPosition++)
                {
                    var argumentExpression = arguments[lastLiftedArgumentPosition].Expression;

                    if (_sideEffectDetector.MayHaveSideEffects(argumentExpression))
                    {
                        var name = UniquifyVariableName("liftedArg");

                        _liftedState.Statements.Insert(
                            liftedStatementsPosition++,
                            GenerateVarDeclaration(name, argumentExpression));
                        _liftedState.VariableNames.Add(name);

                        arguments[lastLiftedArgumentPosition] = Argument(IdentifierName(name));
                    }
                }
            }

            arguments[i] = translated;
        }

        // TODO: don't specify generic parameters if they can all be inferred
        SimpleNameSyntax methodIdentifier = call.Method.IsGenericMethod
            ? GenericName(
                Identifier(call.Method.Name),
                TypeArgumentList(
                    SeparatedList(
                        call.Method.GetGenericArguments().Select(ga => ga.GetTypeSyntax()))))
            : IdentifierName(call.Method.Name);

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
        else
        {
            ExpressionSyntax expression;
            if (call.Object is null)
            {
                // Static method call. Recursively add MemberAccessExpressions for all declaring types (for methods on nested types)
                expression = GetMemberAccessesForAllDeclaringTypes(call.Method.DeclaringType);

                static ExpressionSyntax GetMemberAccessesForAllDeclaringTypes(Type type)
                {
                    return type.DeclaringType is null
                        ? type.GetTypeSyntax()
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

            Result = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    methodIdentifier),
                ArgumentList(SeparatedList(arguments)));
        }

        if (call.Method.DeclaringType.Namespace is { } ns)
        {
            _collectedNamespaces.Add(ns);
        }

        return call;
    }

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression newArray)
    {
        using var _ = ChangeContext(ExpressionContext.Expression);

        Result =
            ArrayCreationExpression(
                    ArrayType(newArray.Type.GetElementType()!.GetTypeSyntax())
                        .WithRankSpecifiers(
                            SingletonList(
                                ArrayRankSpecifier(
                                    SingletonSeparatedList<ExpressionSyntax>(
                                        OmittedArraySizeExpression())))))
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList(
                            newArray.Expressions.Select(Translate<ExpressionSyntax>))));

        return newArray;
    }

    /// <inheritdoc />
    protected override Expression VisitNew(NewExpression node)
    {
        using var _ = ChangeContext(ExpressionContext.Expression);

        if (node.Type.IsAnonymousType())
        {
            if (node.Members is null)
            {
                throw new NotSupportedException("Anonymous type creation without members");
            }

            Result = AnonymousObjectCreationExpression(
                SeparatedList(
                    node.Arguments.Select((arg, i) =>
                            AnonymousObjectMemberDeclarator(NameEquals(node.Members[i].Name), Translate<ExpressionSyntax>(arg)))
                        .ToArray()));
        }
        else
        {
            // If the type has any required properties and the constructor doesn't have [SetsRequiredMembers], we can't just generate an
            // instantiation E.
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
                Result = ObjectCreationExpression(node.Type.GetTypeSyntax())
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList(
                                node.Arguments.Select(arg => Argument(Translate<ExpressionSyntax>(arg))).ToArray())));
            }

            if (node.Constructor?.DeclaringType?.Namespace is not null)
            {
                _collectedNamespaces.Add(node.Constructor.DeclaringType.Namespace);
            }
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
                    loweredAssignmentVariableType = parameter.Type.GetTypeSyntax();
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
    protected override Expression VisitTry(TryExpression node)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        var visitedExpression = Translate<ExpressionSyntax>(node.Expression);

        Result = node.NodeType switch
        {
            ExpressionType.TypeIs
                => BinaryExpression(SyntaxKind.IsExpression, visitedExpression, node.TypeOperand.GetTypeSyntax()),

            ExpressionType.TypeEqual
                => BinaryExpression(SyntaxKind.EqualsExpression, visitedExpression, TypeOfExpression(node.TypeOperand.GetTypeSyntax())),

            _ => throw new ArgumentOutOfRangeException()
        };

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression unary)
    {
        if (unary.Method is not null)
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
            ExpressionType.Convert => _g.ConvertExpression(unary.Type.GetTypeSyntax(), operand),
            ExpressionType.ConvertChecked => _g.ConvertExpression(unary.Type.GetTypeSyntax(), operand),
            ExpressionType.Throw when unary.Type == (typeof(void)) => _g.ThrowStatement(operand),
            ExpressionType.Throw => _g.ThrowExpression(operand),
            ExpressionType.TypeAs => BinaryExpression(SyntaxKind.AsExpression, operand, unary.Type.GetTypeSyntax()),
            ExpressionType.Quote => operand,
            ExpressionType.UnaryPlus => PrefixUnaryExpression(SyntaxKind.UnaryPlusExpression, operand),
            ExpressionType.Unbox => operand,
            ExpressionType.Increment => Translate(Expression.Add(unary.Operand, Expression.Constant(1))),
            ExpressionType.Decrement => Translate(Expression.Subtract(unary.Operand, Expression.Constant(1))),
            ExpressionType.PostIncrementAssign => PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, operand),
            ExpressionType.PostDecrementAssign => PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, operand),
            ExpressionType.PreIncrementAssign => PrefixUnaryExpression(SyntaxKind.PreIncrementExpression, operand),
            ExpressionType.PreDecrementAssign => PrefixUnaryExpression(SyntaxKind.PreDecrementExpression, operand),

            _ => throw new ArgumentOutOfRangeException("Unsupported LINQ unary node: " + unary.NodeType)
        };

        return unary;
    }

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override Expression VisitListInit(ListInitExpression node)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override ElementInit VisitElementInit(ElementInit node)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override MemberBinding VisitMemberBinding(MemberBinding node)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
    {
        throw new NotImplementedException();
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

    private static bool MayHaveSideEffects(Expression expression)
        => expression is not ConstantExpression and not ParameterExpression;

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

    private ContextSwitcher ChangeContext(ExpressionContext newContext)
        => new(this, newContext);

    private readonly struct ContextSwitcher : IDisposable
    {
        private readonly LinqToCSharpTranslator _translator;
        private readonly ExpressionContext _oldContext;

        public ContextSwitcher(LinqToCSharpTranslator translator, ExpressionContext newContext)
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

    private class ConstantDetectionSyntaxWalker : SyntaxWalker
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

    private class SideEffectDetectionSyntaxWalker : SyntaxWalker
    {
        private bool _mayHaveSideEffects;

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
