// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class to create a printable string representation of expression.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class ExpressionPrinter : ExpressionVisitor
{
    private static readonly List<string> SimpleMethods =
    [
        "get_Item",
        "TryReadValue",
        "ReferenceEquals"
    ];

    private readonly IndentedStringBuilder _stringBuilder;
    private readonly Dictionary<ParameterExpression, string?> _parametersInScope;
    private readonly List<ParameterExpression> _namelessParameters;
    private readonly List<ParameterExpression> _encounteredParameters;

    private readonly Dictionary<ExpressionType, string> _binaryOperandMap = new()
    {
        { ExpressionType.Assign, " = " },
        { ExpressionType.Equal, " == " },
        { ExpressionType.NotEqual, " != " },
        { ExpressionType.GreaterThan, " > " },
        { ExpressionType.GreaterThanOrEqual, " >= " },
        { ExpressionType.LessThan, " < " },
        { ExpressionType.LessThanOrEqual, " <= " },
        { ExpressionType.OrElse, " || " },
        { ExpressionType.AndAlso, " && " },
        { ExpressionType.Coalesce, " ?? " },
        { ExpressionType.Add, " + " },
        { ExpressionType.Subtract, " - " },
        { ExpressionType.Multiply, " * " },
        { ExpressionType.Divide, " / " },
        { ExpressionType.Modulo, " % " },
        { ExpressionType.And, " & " },
        { ExpressionType.Or, " | " },
        { ExpressionType.ExclusiveOr, " ^ " }
    };

    /// <summary>
    ///     Creates a new instance of the <see cref="ExpressionPrinter" /> class.
    /// </summary>
    public ExpressionPrinter()
    {
        _stringBuilder = new IndentedStringBuilder();
        _parametersInScope = new Dictionary<ParameterExpression, string?>();
        _namelessParameters = [];
        _encounteredParameters = [];
    }

    private int? CharacterLimit { get; set; }
    private bool Verbose { get; set; }

    /// <summary>
    ///     Appends a new line to current output being built.
    /// </summary>
    /// <returns>This printer so additional calls can be chained.</returns>
    public virtual ExpressionPrinter AppendLine()
    {
        _stringBuilder.AppendLine();
        return this;
    }

    /// <summary>
    ///     Appends the given string and a new line to current output being built.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>This printer so additional calls can be chained.</returns>
    public virtual ExpressionVisitor AppendLine(string value)
    {
        _stringBuilder.AppendLine(value);
        return this;
    }

    /// <summary>
    ///     Appends all the lines to current output being built.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="skipFinalNewline">If true, then a terminating new line is not added.</param>
    /// <returns>This printer so additional calls can be chained.</returns>
    public virtual ExpressionPrinter AppendLines(string value, bool skipFinalNewline = false)
    {
        _stringBuilder.AppendLines(value, skipFinalNewline);
        return this;
    }

    /// <summary>
    ///     Creates a scoped indenter that will increment the indent, then decrement it when disposed.
    /// </summary>
    /// <returns>An indenter.</returns>
    public virtual IDisposable Indent()
        => _stringBuilder.Indent();

    /// <summary>
    ///     Appends the given string to current output being built.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>This printer so additional calls can be chained.</returns>
    public virtual ExpressionPrinter Append(string value)
    {
        _stringBuilder.Append(value);
        return this;
    }

    /// <summary>
    ///     Creates a printable string representation of the given expression.
    /// </summary>
    /// <param name="expression">The expression to print.</param>
    /// <returns>The printable representation.</returns>
    public static string Print(Expression expression)
        => new ExpressionPrinter().PrintCore(expression);

    /// <summary>
    ///     Creates a printable verbose string representation of the given expression.
    /// </summary>
    /// <param name="expression">The expression to print.</param>
    /// <returns>The printable representation.</returns>
    public static string PrintDebug(Expression expression)
        => new ExpressionPrinter().PrintCore(expression, verbose: true);

    /// <summary>
    ///     Creates a printable string representation of the given expression.
    /// </summary>
    /// <param name="expression">The expression to print.</param>
    /// <param name="characterLimit">An optional limit to the number of characters included. Additional output will be truncated.</param>
    /// <returns>The printable representation.</returns>
    public virtual string PrintExpression(Expression expression, int? characterLimit = null)
        => PrintCore(expression, characterLimit);

    /// <summary>
    ///     Creates a printable verbose string representation of the given expression.
    /// </summary>
    /// <param name="expression">The expression to print.</param>
    /// <returns>The printable representation.</returns>
    public virtual string PrintExpressionDebug(Expression expression)
        => PrintCore(expression, verbose: true);

    private string PrintCore(Expression expression, int? characterLimit = null, bool verbose = false)
    {
        _stringBuilder.Clear();
        _parametersInScope.Clear();
        _namelessParameters.Clear();
        _encounteredParameters.Clear();

        CharacterLimit = characterLimit;
        Verbose = verbose;

        Visit(expression);

        return ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var printed = PostProcess(_stringBuilder.ToString());

        if (CharacterLimit is > 0)
        {
            printed = printed.Length > CharacterLimit
                ? printed[..CharacterLimit.Value] + "..."
                : printed;
        }

        return printed;
    }

    /// <summary>
    ///     Returns binary operator string corresponding to given <see cref="ExpressionType" />.
    /// </summary>
    /// <param name="expressionType">The expression type to generate binary operator for.</param>
    /// <returns>The binary operator string.</returns>
    public virtual string GenerateBinaryOperator(ExpressionType expressionType)
        => _binaryOperandMap[expressionType];

    /// <summary>
    ///     Visit given readonly collection of expression for printing.
    /// </summary>
    /// <param name="items">A collection of items to print.</param>
    /// <param name="joinAction">A join action to use when joining printout of individual item in the collection.</param>
    public virtual void VisitCollection<T>(IReadOnlyCollection<T> items, Action<ExpressionPrinter>? joinAction = null)
        where T : Expression
    {
        joinAction ??= (p => p.Append(", "));

        var first = true;
        foreach (var item in items)
        {
            if (!first)
            {
                joinAction(this);
            }
            else
            {
                first = false;
            }

            Visit(item);
        }
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
    {
        if (expression == null)
        {
            return null;
        }

        if (CharacterLimit != null
            && _stringBuilder.Length > CharacterLimit.Value)
        {
            return expression;
        }

        switch (expression.NodeType)
        {
            case ExpressionType.AndAlso:
            case ExpressionType.ArrayIndex:
            case ExpressionType.Assign:
            case ExpressionType.Equal:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.NotEqual:
            case ExpressionType.OrElse:
            case ExpressionType.Coalesce:
            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
                VisitBinary((BinaryExpression)expression);
                break;

            case ExpressionType.Block:
                VisitBlock((BlockExpression)expression);
                break;

            case ExpressionType.Conditional:
                VisitConditional((ConditionalExpression)expression);
                break;

            case ExpressionType.Constant:
                VisitConstant((ConstantExpression)expression);
                break;

            case ExpressionType.Lambda:
                base.Visit(expression);
                break;

            case ExpressionType.Goto:
                VisitGoto((GotoExpression)expression);
                break;

            case ExpressionType.Label:
                VisitLabel((LabelExpression)expression);
                break;

            case ExpressionType.MemberAccess:
                VisitMember((MemberExpression)expression);
                break;

            case ExpressionType.MemberInit:
                VisitMemberInit((MemberInitExpression)expression);
                break;

            case ExpressionType.Call:
                VisitMethodCall((MethodCallExpression)expression);
                break;

            case ExpressionType.New:
                VisitNew((NewExpression)expression);
                break;

            case ExpressionType.NewArrayInit:
            case ExpressionType.NewArrayBounds:
                VisitNewArray((NewArrayExpression)expression);
                break;

            case ExpressionType.Parameter:
                VisitParameter((ParameterExpression)expression);
                break;

            case ExpressionType.Convert:
            case ExpressionType.Throw:
            case ExpressionType.Not:
            case ExpressionType.TypeAs:
            case ExpressionType.Quote:
                VisitUnary((UnaryExpression)expression);
                break;

            case ExpressionType.Default:
                VisitDefault((DefaultExpression)expression);
                break;

            case ExpressionType.Try:
                VisitTry((TryExpression)expression);
                break;

            case ExpressionType.Index:
                VisitIndex((IndexExpression)expression);
                break;

            case ExpressionType.TypeIs:
                VisitTypeBinary((TypeBinaryExpression)expression);
                break;

            case ExpressionType.Switch:
                VisitSwitch((SwitchExpression)expression);
                break;

            case ExpressionType.Invoke:
                VisitInvocation((InvocationExpression)expression);
                break;

            case ExpressionType.Loop:
                VisitLoop((LoopExpression)expression);
                break;

            case ExpressionType.Extension:
                VisitExtension(expression);
                break;

            default:
                UnhandledExpressionType(expression);
                break;
        }

        return expression;
    }

    /// <inheritdoc />
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        Visit(binaryExpression.Left);

        if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
        {
            _stringBuilder.Append("[");

            Visit(binaryExpression.Right);

            _stringBuilder.Append("]");
        }
        else
        {
            if (!_binaryOperandMap.TryGetValue(binaryExpression.NodeType, out var operand))
            {
                UnhandledExpressionType(binaryExpression);
            }
            else
            {
                _stringBuilder.Append(operand);
            }

            Visit(binaryExpression.Right);
        }

        return binaryExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitBlock(BlockExpression blockExpression)
    {
        AppendLine();
        AppendLine("{");

        using (_stringBuilder.Indent())
        {
            foreach (var variable in blockExpression.Variables)
            {
                if (!_parametersInScope.ContainsKey(variable))
                {
                    _parametersInScope.Add(variable, variable.Name);
                    Append(variable.Type.ShortDisplayName());
                    Append(" ");
                    VisitParameter(variable);
                    AppendLine(";");
                }
            }

            var expressions = blockExpression.Expressions.Count > 0
                ? blockExpression.Expressions.Except(new[] { blockExpression.Result })
                : blockExpression.Expressions;

            foreach (var expression in expressions)
            {
                Visit(expression);

                if (expression is not BlockExpression and not LoopExpression and not SwitchExpression)
                {
                    AppendLine(";");
                }
            }

            if (blockExpression.Expressions.Count > 0)
            {
                if (blockExpression.Result.Type != typeof(void))
                {
                    Append("return ");
                }

                if (blockExpression.Result is not DefaultExpression)
                {
                    Visit(blockExpression.Result);

                    if (blockExpression.Result is not (BlockExpression or LoopExpression or SwitchExpression))
                    {
                        AppendLine(";");
                    }
                }
            }
        }

        Append("}");

        return blockExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
    {
        Visit(conditionalExpression.Test);

        _stringBuilder.Append(" ? ");

        Visit(conditionalExpression.IfTrue);

        _stringBuilder.Append(" : ");

        Visit(conditionalExpression.IfFalse);

        return conditionalExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression constantExpression)
    {
        if (constantExpression.Value is IPrintableExpression printable)
        {
            printable.Print(this);
        }
        else
        {
            PrintValue(constantExpression.Value);
        }

        return constantExpression;

        void PrintValue(object? value)
        {
            if (value is IEnumerable enumerable and not string)
            {
                _stringBuilder.Append(value.GetType().ShortDisplayName() + " { ");

                var first = true;
                foreach (var item in enumerable)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        _stringBuilder.Append(", ");
                    }

                    PrintValue(item);
                }

                _stringBuilder.Append(" }");
                return;
            }

            var stringValue = value == null
                ? "null"
                : value.ToString() != value.GetType().ToString()
                    ? value.ToString()
                    : value.GetType().ShortDisplayName();

            if (value is string)
            {
                stringValue = $@"""{stringValue}""";
            }

            _stringBuilder.Append(stringValue ?? "Unknown");
        }
    }

    /// <inheritdoc />
    protected override Expression VisitGoto(GotoExpression gotoExpression)
    {
        Append("Goto(" + gotoExpression.Kind.ToString().ToLower() + " ");

        if (gotoExpression.Kind == GotoExpressionKind.Break)
        {
            Append(gotoExpression.Target.Name!);
        }
        else
        {
            AppendLine("(" + gotoExpression.Target.Type.ShortDisplayName() + ")" + gotoExpression.Target + " {");
            using (_stringBuilder.Indent())
            {
                Visit(gotoExpression.Value);
            }

            _stringBuilder.Append("}");
        }

        AppendLine(")");

        return gotoExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitLabel(LabelExpression labelExpression)
    {
        _stringBuilder.Append(labelExpression.Target.ToString());

        return labelExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
    {
        if (lambdaExpression.Parameters.Count != 1)
        {
            _stringBuilder.Append("(");
        }

        foreach (var parameter in lambdaExpression.Parameters)
        {
            var parameterName = parameter.Name;

            _parametersInScope.TryAdd(parameter, parameterName);

            Visit(parameter);

            if (parameter != lambdaExpression.Parameters.Last())
            {
                _stringBuilder.Append(", ");
            }
        }

        if (lambdaExpression.Parameters.Count != 1)
        {
            _stringBuilder.Append(")");
        }

        _stringBuilder.Append(" => ");

        Visit(lambdaExpression.Body);

        foreach (var parameter in lambdaExpression.Parameters)
        {
            // however we don't remove nameless parameters so that they are unique globally, not just within the scope
            _parametersInScope.Remove(parameter);
        }

        return lambdaExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        if (memberExpression.Expression != null)
        {
            if (memberExpression.Expression.NodeType == ExpressionType.Convert
                || memberExpression.Expression is BinaryExpression)
            {
                _stringBuilder.Append("(");
                Visit(memberExpression.Expression);
                _stringBuilder.Append(")");
            }
            else
            {
                Visit(memberExpression.Expression);
            }
        }
        else
        {
            // ReSharper disable once PossibleNullReferenceException
            _stringBuilder.Append(memberExpression.Member.DeclaringType?.Name ?? "MethodWithoutDeclaringType");
        }

        _stringBuilder.Append("." + memberExpression.Member.Name);

        return memberExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
    {
        _stringBuilder.Append("new " + memberInitExpression.Type.ShortDisplayName());

        var appendAction = memberInitExpression.Bindings.Count > 1 ? (Func<string, ExpressionVisitor>)AppendLine : Append;
        appendAction("{ ");
        using (_stringBuilder.Indent())
        {
            for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
            {
                var binding = memberInitExpression.Bindings[i];
                if (binding is MemberAssignment assignment)
                {
                    _stringBuilder.Append(assignment.Member.Name + " = ");
                    Visit(assignment.Expression);
                    appendAction(i == memberInitExpression.Bindings.Count - 1 ? " " : ", ");
                }
                else
                {
                    AppendLine(CoreStrings.UnhandledMemberBinding(binding.BindingType));
                }
            }
        }

        AppendLine("}");

        return memberInitExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Object != null)
        {
            switch (methodCallExpression.Object)
            {
                case BinaryExpression:
                case UnaryExpression:
                    _stringBuilder.Append("(");
                    Visit(methodCallExpression.Object);
                    _stringBuilder.Append(")");
                    break;
                default:
                    Visit(methodCallExpression.Object);
                    break;
            }

            _stringBuilder.Append(".");
        }

        var methodArguments = methodCallExpression.Arguments.ToList();
        var method = methodCallExpression.Method;

        var extensionMethod = !Verbose
            && methodCallExpression.Arguments.Count > 0
            && method.IsDefined(typeof(ExtensionAttribute), inherit: false);

        if (extensionMethod)
        {
            Visit(methodArguments[0]);
            _stringBuilder.IncrementIndent();
            _stringBuilder.AppendLine();
            _stringBuilder.Append($".{method.Name}");
            methodArguments = methodArguments.Skip(1).ToList();
            if (method.Name is nameof(Enumerable.Cast) or nameof(Enumerable.OfType))
            {
                PrintGenericArguments(method, _stringBuilder);
            }
        }
        else
        {
            if (method.IsStatic)
            {
                _stringBuilder.Append(method.DeclaringType!.ShortDisplayName()).Append(".");
            }

            _stringBuilder.Append(method.Name);
            PrintGenericArguments(method, _stringBuilder);
        }

        _stringBuilder.Append("(");

        var isSimpleMethodOrProperty = SimpleMethods.Contains(method.Name)
            || methodArguments.Count < 2
            || method.IsEFPropertyMethod();

        var appendAction = isSimpleMethodOrProperty ? (Func<string, ExpressionVisitor>)Append : AppendLine;

        if (methodArguments.Count > 0)
        {
            appendAction("");

            var argumentNames
                = !isSimpleMethodOrProperty
                    ? extensionMethod
                        ? method.GetParameters().Skip(1).Select(p => p.Name).ToList()
                        : method.GetParameters().Select(p => p.Name).ToList()
                    : [];

            IDisposable? indent = null;

            if (!isSimpleMethodOrProperty)
            {
                indent = _stringBuilder.Indent();
            }

            for (var i = 0; i < methodArguments.Count; i++)
            {
                var argument = methodArguments[i];

                if (!isSimpleMethodOrProperty)
                {
                    _stringBuilder.Append(argumentNames[i] + ": ");
                }

                Visit(argument);

                if (i < methodArguments.Count - 1)
                {
                    appendAction(", ");
                }
            }

            if (!isSimpleMethodOrProperty)
            {
                indent?.Dispose();
            }
        }

        Append(")");

        if (extensionMethod)
        {
            _stringBuilder.DecrementIndent();
        }

        return methodCallExpression;

        static void PrintGenericArguments(MethodInfo method, IndentedStringBuilder stringBuilder)
        {
            if (method.IsGenericMethod)
            {
                stringBuilder.Append("<");
                var first = true;
                foreach (var genericArgument in method.GetGenericArguments())
                {
                    if (!first)
                    {
                        stringBuilder.Append(", ");
                    }

                    stringBuilder.Append(genericArgument.ShortDisplayName());
                    first = false;
                }

                stringBuilder.Append(">");
            }
        }
    }

    /// <inheritdoc />
    protected override Expression VisitNew(NewExpression newExpression)
    {
        _stringBuilder.Append("new ");

        var isComplex = newExpression.Arguments.Count > 1;
        var appendAction = isComplex ? (Func<string, ExpressionVisitor>)AppendLine : Append;

        var isAnonymousType = newExpression.Type.IsAnonymousType();
        if (!isAnonymousType)
        {
            _stringBuilder.Append(newExpression.Type.ShortDisplayName());
            appendAction("(");
        }
        else
        {
            appendAction("{ ");
        }

        IDisposable? indent = null;
        if (isComplex)
        {
            indent = _stringBuilder.Indent();
        }

        for (var i = 0; i < newExpression.Arguments.Count; i++)
        {
            if (newExpression.Members != null)
            {
                Append(newExpression.Members[i].Name + " = ");
            }

            Visit(newExpression.Arguments[i]);
            appendAction(i == newExpression.Arguments.Count - 1 ? "" : ", ");
        }

        if (isComplex)
        {
            indent?.Dispose();
        }

        _stringBuilder.Append(!isAnonymousType ? ")" : " }");

        return newExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        if (newArrayExpression.NodeType == ExpressionType.NewArrayBounds)
        {
            Append("new " + newArrayExpression.Type.GetElementType()!.ShortDisplayName() + "[");
            VisitArguments(newArrayExpression.Expressions, s => Append(s));
            Append("]");

            return newArrayExpression;
        }

        var isComplex = newArrayExpression.Expressions.Count > 1;
        var appendAction = isComplex ? s => AppendLine(s) : (Action<string>)(s => Append(s));

        appendAction("new " + newArrayExpression.Type.GetElementType()!.ShortDisplayName() + "[]");
        appendAction("{ ");

        IDisposable? indent = null;
        if (isComplex)
        {
            indent = _stringBuilder.Indent();
        }

        VisitArguments(newArrayExpression.Expressions, appendAction, lastSeparator: " ");

        if (isComplex)
        {
            indent?.Dispose();
        }

        Append("}");

        return newArrayExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression parameterExpression)
    {
        if (_parametersInScope.TryGetValue(parameterExpression, out var parameterName))
        {
            if (parameterName == null)
            {
                if (!_namelessParameters.Contains(parameterExpression))
                {
                    _namelessParameters.Add(parameterExpression);
                }

                Append("namelessParameter{");
                Append(_namelessParameters.IndexOf(parameterExpression).ToString());
                Append("}");
            }
            else if (parameterName.Contains('.'))
            {
                Append("[");
                Append(parameterName);
                Append("]");
            }
            else
            {
                Append(parameterName);
            }
        }
        else
        {
            if (Verbose)
            {
                Append("(Unhandled parameter: ");
                Append(parameterExpression.Name ?? "NoNameParameter");
                Append(")");
            }
            else
            {
                Append(parameterExpression.Name ?? "NoNameParameter");
            }
        }

        if (Verbose)
        {
            var parameterIndex = _encounteredParameters.Count;
            if (_encounteredParameters.Contains(parameterExpression))
            {
                parameterIndex = _encounteredParameters.IndexOf(parameterExpression);
            }
            else
            {
                _encounteredParameters.Add(parameterExpression);
            }

            _stringBuilder.Append("{" + parameterIndex + "}");
        }

        return parameterExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (unaryExpression.NodeType)
        {
            case ExpressionType.Convert:
                _stringBuilder.Append("(" + unaryExpression.Type.ShortDisplayName() + ")");

                if (unaryExpression.Operand is BinaryExpression)
                {
                    _stringBuilder.Append("(");
                    Visit(unaryExpression.Operand);
                    _stringBuilder.Append(")");
                }
                else
                {
                    Visit(unaryExpression.Operand);
                }

                break;

            case ExpressionType.Throw:
                _stringBuilder.Append("throw ");
                Visit(unaryExpression.Operand);
                break;

            case ExpressionType.Not:
                _stringBuilder.Append("!(");
                Visit(unaryExpression.Operand);
                _stringBuilder.Append(")");
                break;

            case ExpressionType.TypeAs:
                _stringBuilder.Append("(");
                Visit(unaryExpression.Operand);
                _stringBuilder.Append(" as " + unaryExpression.Type.ShortDisplayName() + ")");
                break;

            case ExpressionType.Quote:
                Visit(unaryExpression.Operand);
                break;

            default:
                UnhandledExpressionType(unaryExpression);
                break;
        }

        return unaryExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitDefault(DefaultExpression defaultExpression)
    {
        _stringBuilder.Append("default(" + defaultExpression.Type.ShortDisplayName() + ")");

        return defaultExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitTry(TryExpression tryExpression)
    {
        _stringBuilder.Append("try { ");
        Visit(tryExpression.Body);
        _stringBuilder.Append(" } ");

        foreach (var handler in tryExpression.Handlers)
        {
            _stringBuilder.Append("catch (" + handler.Test.Name + ") { ... } ");
        }

        return tryExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitIndex(IndexExpression indexExpression)
    {
        Visit(indexExpression.Object);
        _stringBuilder.Append("[");
        VisitArguments(
            indexExpression.Arguments, s => { _stringBuilder.Append(s); });
        _stringBuilder.Append("]");

        return indexExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
    {
        _stringBuilder.Append("(");
        Visit(typeBinaryExpression.Expression);
        _stringBuilder.Append(" is " + typeBinaryExpression.TypeOperand.ShortDisplayName() + ")");

        return typeBinaryExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitSwitch(SwitchExpression switchExpression)
    {
        _stringBuilder.Append("switch (");
        Visit(switchExpression.SwitchValue);
        _stringBuilder.AppendLine(")");
        _stringBuilder.AppendLine("{");
        _stringBuilder.IncrementIndent();

        foreach (var @case in switchExpression.Cases)
        {
            foreach (var testValue in @case.TestValues)
            {
                _stringBuilder.Append("case ");
                Visit(testValue);
                _stringBuilder.AppendLine(": ");
            }

            using (_stringBuilder.Indent())
            {
                Visit(@case.Body);
            }

            _stringBuilder.AppendLine();
        }

        if (switchExpression.DefaultBody != null)
        {
            _stringBuilder.AppendLine("default: ");
            using (_stringBuilder.Indent())
            {
                Visit(switchExpression.DefaultBody);
            }

            _stringBuilder.AppendLine();
        }

        _stringBuilder.DecrementIndent();
        _stringBuilder.AppendLine("}");

        return switchExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitInvocation(InvocationExpression invocationExpression)
    {
        _stringBuilder.Append("Invoke(");
        Visit(invocationExpression.Expression);

        foreach (var argument in invocationExpression.Arguments)
        {
            _stringBuilder.Append(", ");
            Visit(argument);
        }

        _stringBuilder.Append(")");

        return invocationExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitLoop(LoopExpression loopExpression)
    {
        _stringBuilder.AppendLine($"Loop(Break: {loopExpression.BreakLabel?.Name} Continue: {loopExpression.ContinueLabel?.Name})");
        _stringBuilder.AppendLine("{");

        using (_stringBuilder.Indent())
        {
            Visit(loopExpression.Body);
        }

        _stringBuilder.AppendLine("}");

        return loopExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is IPrintableExpression printable)
        {
            printable.Print(this);
        }
        else
        {
            UnhandledExpressionType(extensionExpression);
        }

        return extensionExpression;
    }

    private void VisitArguments(
        IReadOnlyList<Expression> arguments,
        Action<string> appendAction,
        string lastSeparator = "",
        bool areConnected = false)
    {
        for (var i = 0; i < arguments.Count; i++)
        {
            if (areConnected && i == arguments.Count - 1)
            {
                Append("");
            }

            Visit(arguments[i]);
            appendAction(i == arguments.Count - 1 ? lastSeparator : ", ");
        }
    }

    private static string PostProcess(string printedExpression)
    {
        var processedPrintedExpression = printedExpression
            .Replace("Microsoft.EntityFrameworkCore.Query.", "")
            .Replace("Microsoft.EntityFrameworkCore.", "")
            .Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

        return processedPrintedExpression;
    }

    private void UnhandledExpressionType(Expression expression)
        => AppendLine(expression.ToString());
}
