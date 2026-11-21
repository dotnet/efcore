// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
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
        { ExpressionType.ExclusiveOr, " ^ " },
        { ExpressionType.LeftShift, " << " },
        { ExpressionType.RightShift, " >> " }
    };

    /// <summary>
    ///     Creates a new instance of the <see cref="ExpressionPrinter" /> class.
    /// </summary>
    public ExpressionPrinter()
    {
        _stringBuilder = new IndentedStringBuilder();
        _parametersInScope = [];
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
    public virtual ExpressionPrinter AppendLine(string value)
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
    [return: NotNullIfNotNull(nameof(node))]
    public override Expression? Visit(Expression? node)
    {
        if (node == null)
        {
            return null;
        }

        if (CharacterLimit != null
            && _stringBuilder.Length > CharacterLimit.Value)
        {
            return node;
        }

        switch (node.NodeType)
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
            case ExpressionType.LeftShift:
            case ExpressionType.RightShift:
                VisitBinary((BinaryExpression)node);
                break;

            case ExpressionType.Block:
                VisitBlock((BlockExpression)node);
                break;

            case ExpressionType.Conditional:
                VisitConditional((ConditionalExpression)node);
                break;

            case ExpressionType.Constant:
                VisitConstant((ConstantExpression)node);
                break;

            case ExpressionType.Lambda:
                base.Visit(node);
                break;

            case ExpressionType.Goto:
                VisitGoto((GotoExpression)node);
                break;

            case ExpressionType.Label:
                VisitLabel((LabelExpression)node);
                break;

            case ExpressionType.MemberAccess:
                VisitMember((MemberExpression)node);
                break;

            case ExpressionType.MemberInit:
                VisitMemberInit((MemberInitExpression)node);
                break;

            case ExpressionType.Call:
                VisitMethodCall((MethodCallExpression)node);
                break;

            case ExpressionType.New:
                VisitNew((NewExpression)node);
                break;

            case ExpressionType.NewArrayInit:
            case ExpressionType.NewArrayBounds:
                VisitNewArray((NewArrayExpression)node);
                break;

            case ExpressionType.Parameter:
                VisitParameter((ParameterExpression)node);
                break;

            case ExpressionType.Convert:
            case ExpressionType.Throw:
            case ExpressionType.Not:
            case ExpressionType.TypeAs:
            case ExpressionType.Quote:
                VisitUnary((UnaryExpression)node);
                break;

            case ExpressionType.Default:
                VisitDefault((DefaultExpression)node);
                break;

            case ExpressionType.Try:
                VisitTry((TryExpression)node);
                break;

            case ExpressionType.Index:
                VisitIndex((IndexExpression)node);
                break;

            case ExpressionType.TypeIs:
                VisitTypeBinary((TypeBinaryExpression)node);
                break;

            case ExpressionType.Switch:
                VisitSwitch((SwitchExpression)node);
                break;

            case ExpressionType.Invoke:
                VisitInvocation((InvocationExpression)node);
                break;

            case ExpressionType.Loop:
                VisitLoop((LoopExpression)node);
                break;

            case ExpressionType.Extension:
                VisitExtension(node);
                break;

            default:
                UnhandledExpressionType(node);
                break;
        }

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitBinary(BinaryExpression node)
    {
        Visit(node.Left);

        if (node.NodeType == ExpressionType.ArrayIndex)
        {
            _stringBuilder.Append("[");

            Visit(node.Right);

            _stringBuilder.Append("]");
        }
        else
        {
            if (!_binaryOperandMap.TryGetValue(node.NodeType, out var operand))
            {
                UnhandledExpressionType(node);
            }
            else
            {
                _stringBuilder.Append(operand);
            }

            Visit(node.Right);
        }

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitBlock(BlockExpression node)
    {
        AppendLine();
        AppendLine("{");

        using (_stringBuilder.Indent())
        {
            foreach (var variable in node.Variables)
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

            var expressions = node.Expressions.Count > 0
                ? node.Expressions.Take(node.Expressions.Count - 1)
                : node.Expressions;

            foreach (var expression in expressions)
            {
                Visit(expression);

                if (expression is not BlockExpression and not LoopExpression and not SwitchExpression)
                {
                    AppendLine(";");
                }
            }

            if (node.Expressions.Count > 0)
            {
                if (node.Result.Type != typeof(void))
                {
                    Append("return ");
                }

                if (node.Result is not DefaultExpression)
                {
                    Visit(node.Result);

                    if (node.Result is not (BlockExpression or LoopExpression or SwitchExpression))
                    {
                        AppendLine(";");
                    }
                }
            }
        }

        Append("}");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitConditional(ConditionalExpression node)
    {
        Visit(node.Test);

        _stringBuilder.Append(" ? ");

        Visit(node.IfTrue);

        _stringBuilder.Append(" : ");

        Visit(node.IfFalse);

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression node)
    {
        switch (node.Value)
        {
            case IPrintableExpression printable:
                printable.Print(this);
                break;

            // EnumerableQuery is returned by AsQueryable(), and can be a constant as a result of funcletization.
            // We need to print these as a regular enumerable as it doesn't represent a query tree.
            case IQueryable queryable and not EnumerableQuery:
                Visit(queryable.Expression);
                break;

            default:
                PrintValue(node.Value);
                break;
        }

        return node;

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
    protected override Expression VisitGoto(GotoExpression node)
    {
        Append("Goto(" + node.Kind.ToString().ToLower() + " ");

        if (node.Kind == GotoExpressionKind.Break)
        {
            Append(node.Target.Name!);
        }
        else
        {
            AppendLine("(" + node.Target.Type.ShortDisplayName() + ")" + node.Target + " {");
            using (_stringBuilder.Indent())
            {
                Visit(node.Value);
            }

            _stringBuilder.Append("}");
        }

        AppendLine(")");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitLabel(LabelExpression node)
    {
        _stringBuilder.Append(node.Target.ToString());

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        if (node.Parameters.Count != 1)
        {
            _stringBuilder.Append("(");
        }

        foreach (var parameter in node.Parameters)
        {
            var parameterName = parameter.Name;

            _parametersInScope.TryAdd(parameter, parameterName);

            Visit(parameter);

            if (parameter != node.Parameters.Last())
            {
                _stringBuilder.Append(", ");
            }
        }

        if (node.Parameters.Count != 1)
        {
            _stringBuilder.Append(")");
        }

        _stringBuilder.Append(" => ");

        Visit(node.Body);

        foreach (var parameter in node.Parameters)
        {
            // however we don't remove nameless parameters so that they are unique globally, not just within the scope
            _parametersInScope.Remove(parameter);
        }

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null)
        {
            if (node.Expression.NodeType == ExpressionType.Convert
                || node.Expression is BinaryExpression)
            {
                _stringBuilder.Append("(");
                Visit(node.Expression);
                _stringBuilder.Append(")");
            }
            else
            {
                Visit(node.Expression);
            }
        }
        else
        {
            // ReSharper disable once PossibleNullReferenceException
            _stringBuilder.Append(node.Member.DeclaringType?.Name ?? "MethodWithoutDeclaringType");
        }

        _stringBuilder.Append("." + node.Member.Name);

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        _stringBuilder.Append("new " + node.Type.ShortDisplayName());

        var appendAction = node.Bindings.Count > 1 ? (Func<string, ExpressionVisitor>)AppendLine : Append;
        appendAction("{ ");
        using (_stringBuilder.Indent())
        {
            for (var i = 0; i < node.Bindings.Count; i++)
            {
                var binding = node.Bindings[i];
                if (binding is MemberAssignment assignment)
                {
                    _stringBuilder.Append(assignment.Member.Name + " = ");
                    Visit(assignment.Expression);
                    appendAction(i == node.Bindings.Count - 1 ? " " : ", ");
                }
                else
                {
                    AppendLine(CoreStrings.UnhandledMemberBinding(binding.BindingType));
                }
            }
        }

        AppendLine("}");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Object != null)
        {
            switch (node.Object)
            {
                case BinaryExpression:
                case UnaryExpression:
                    _stringBuilder.Append("(");
                    Visit(node.Object);
                    _stringBuilder.Append(")");
                    break;
                default:
                    Visit(node.Object);
                    break;
            }

            _stringBuilder.Append(".");
        }

        var methodArguments = node.Arguments.ToList();
        var method = node.Method;

        var extensionMethod = !Verbose
            && node.Arguments.Count > 0
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

        return node;

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
    protected override Expression VisitNew(NewExpression node)
    {
        _stringBuilder.Append("new ");

        var isComplex = node.Arguments.Count > 1;
        var appendAction = isComplex ? (Func<string, ExpressionVisitor>)AppendLine : Append;

        var isAnonymousType = node.Type.IsAnonymousType();
        if (!isAnonymousType)
        {
            _stringBuilder.Append(node.Type.ShortDisplayName());
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

        for (var i = 0; i < node.Arguments.Count; i++)
        {
            if (node.Members != null)
            {
                Append(node.Members[i].Name + " = ");
            }

            Visit(node.Arguments[i]);
            appendAction(i == node.Arguments.Count - 1 ? "" : ", ");
        }

        if (isComplex)
        {
            indent?.Dispose();
        }

        _stringBuilder.Append(!isAnonymousType ? ")" : " }");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression node)
    {
        if (node.NodeType == ExpressionType.NewArrayBounds)
        {
            Append("new " + node.Type.GetElementType()!.ShortDisplayName() + "[");
            VisitArguments(node.Expressions, s => Append(s));
            Append("]");

            return node;
        }

        var isComplex = node.Expressions.Count > 1;
        var appendAction = isComplex ? s => AppendLine(s) : (Action<string>)(s => Append(s));

        appendAction("new " + node.Type.GetElementType()!.ShortDisplayName() + "[]");
        appendAction("{ ");

        IDisposable? indent = null;
        if (isComplex)
        {
            indent = _stringBuilder.Indent();
        }

        VisitArguments(node.Expressions, appendAction, lastSeparator: " ");

        if (isComplex)
        {
            indent?.Dispose();
        }

        Append("}");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (_parametersInScope.TryGetValue(node, out var parameterName))
        {
            if (parameterName == null)
            {
                if (!_namelessParameters.Contains(node))
                {
                    _namelessParameters.Add(node);
                }

                Append("namelessParameter{");
                Append(_namelessParameters.IndexOf(node).ToString());
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
                Append(node.Name ?? "NoNameParameter");
                Append(")");
            }
            else
            {
                Append(node.Name ?? "NoNameParameter");
            }
        }

        if (Verbose)
        {
            var parameterIndex = _encounteredParameters.Count;
            if (_encounteredParameters.Contains(node))
            {
                parameterIndex = _encounteredParameters.IndexOf(node);
            }
            else
            {
                _encounteredParameters.Add(node);
            }

            _stringBuilder.Append("{" + parameterIndex + "}");
        }

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression node)
    {
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (node.NodeType)
        {
            case ExpressionType.Convert:
                _stringBuilder.Append("(" + node.Type.ShortDisplayName() + ")");

                if (node.Operand is BinaryExpression)
                {
                    _stringBuilder.Append("(");
                    Visit(node.Operand);
                    _stringBuilder.Append(")");
                }
                else
                {
                    Visit(node.Operand);
                }

                break;

            case ExpressionType.Throw:
                _stringBuilder.Append("throw ");
                Visit(node.Operand);
                break;

            case ExpressionType.Not:
                _stringBuilder.Append("!(");
                Visit(node.Operand);
                _stringBuilder.Append(")");
                break;

            case ExpressionType.TypeAs:
                _stringBuilder.Append("(");
                Visit(node.Operand);
                _stringBuilder.Append(" as " + node.Type.ShortDisplayName() + ")");
                break;

            case ExpressionType.Quote:
                Visit(node.Operand);
                break;

            default:
                UnhandledExpressionType(node);
                break;
        }

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitDefault(DefaultExpression node)
    {
        _stringBuilder.Append("default(" + node.Type.ShortDisplayName() + ")");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitTry(TryExpression node)
    {
        _stringBuilder.Append("try { ");
        Visit(node.Body);
        _stringBuilder.Append(" } ");

        foreach (var handler in node.Handlers)
        {
            _stringBuilder.Append("catch (" + handler.Test.Name + ") { ... } ");
        }

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitIndex(IndexExpression node)
    {
        Visit(node.Object);
        _stringBuilder.Append("[");
        VisitArguments(
            node.Arguments, s => _stringBuilder.Append(s));
        _stringBuilder.Append("]");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        _stringBuilder.Append("(");
        Visit(node.Expression);
        _stringBuilder.Append(" is " + node.TypeOperand.ShortDisplayName() + ")");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitSwitch(SwitchExpression node)
    {
        _stringBuilder.Append("switch (");
        Visit(node.SwitchValue);
        _stringBuilder.AppendLine(")");
        _stringBuilder.AppendLine("{");
        _stringBuilder.IncrementIndent();

        foreach (var @case in node.Cases)
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

        if (node.DefaultBody != null)
        {
            _stringBuilder.AppendLine("default: ");
            using (_stringBuilder.Indent())
            {
                Visit(node.DefaultBody);
            }

            _stringBuilder.AppendLine();
        }

        _stringBuilder.DecrementIndent();
        _stringBuilder.AppendLine("}");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitInvocation(InvocationExpression node)
    {
        _stringBuilder.Append("Invoke(");
        Visit(node.Expression);

        foreach (var argument in node.Arguments)
        {
            _stringBuilder.Append(", ");
            Visit(argument);
        }

        _stringBuilder.Append(")");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitLoop(LoopExpression node)
    {
        _stringBuilder.AppendLine($"Loop(Break: {node.BreakLabel?.Name} Continue: {node.ContinueLabel?.Name})");
        _stringBuilder.AppendLine("{");

        using (_stringBuilder.Indent())
        {
            Visit(node.Body);
        }

        _stringBuilder.AppendLine("}");

        return node;
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression node)
    {
        if (node is IPrintableExpression printable)
        {
            printable.Print(this);
        }
        else
        {
            UnhandledExpressionType(node);
        }

        return node;
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
