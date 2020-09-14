// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A class to create a printable string representation of expression.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ExpressionPrinter : ExpressionVisitor
    {
        private static readonly List<string> _simpleMethods = new List<string>
        {
            "get_Item",
            "TryReadValue",
            "ReferenceEquals"
        };

        private readonly IndentedStringBuilder _stringBuilder;
        private readonly Dictionary<ParameterExpression, string> _parametersInScope;
        private readonly List<ParameterExpression> _namelessParameters;
        private readonly List<ParameterExpression> _encounteredParameters;

        private readonly Dictionary<ExpressionType, string> _binaryOperandMap = new Dictionary<ExpressionType, string>
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
            _parametersInScope = new Dictionary<ParameterExpression, string>();
            _namelessParameters = new List<ParameterExpression>();
            _encounteredParameters = new List<ParameterExpression>();
        }

        private int? CharacterLimit { get; set; }
        private bool Verbose { get; set; }

        /// <summary>
        ///     Visit given readonly collection of expression for printing.
        /// </summary>
        /// <param name="items"> A collection of items to print. </param>
        /// <param name="joinAction"> A join action to use when joining printout of individual item in the collection. </param>
        public virtual void VisitCollection<T>(
            [NotNull] IReadOnlyCollection<T> items,
            [CanBeNull] Action<ExpressionPrinter> joinAction = null)
            where T : Expression
        {
            Check.NotNull(items, nameof(items));

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

        /// <summary>
        ///     Appends a new line to current output being built.
        /// </summary>
        /// <returns> This printer so additional calls can be chained. </returns>
        public virtual ExpressionPrinter AppendLine()
        {
            _stringBuilder.AppendLine();
            return this;
        }

        /// <summary>
        ///     Appends the given string and a new line to current output being built.
        /// </summary>
        /// <param name="value"> The string to append. </param>
        /// <returns> This printer so additional calls can be chained. </returns>
        public virtual ExpressionVisitor AppendLine([NotNull] string value)
        {
            _stringBuilder.AppendLine(value);
            return this;
        }

        /// <summary>
        ///     Appends all the lines to current output being built.
        /// </summary>
        /// <param name="value"> The string to append. </param>
        /// <param name="skipFinalNewline"> If true, then a terminating new line is not added. </param>
        /// <returns> This printer so additional calls can be chained. </returns>
        public virtual ExpressionPrinter AppendLines([NotNull] string value, bool skipFinalNewline = false)
        {
            _stringBuilder.AppendLines(value, skipFinalNewline);
            return this;
        }

        /// <summary>
        ///     Creates a scoped indenter that will increment the indent, then decrement it when disposed.
        /// </summary>
        /// <returns> An indenter. </returns>
        public virtual IDisposable Indent()
            => _stringBuilder.Indent();

        /// <summary>
        ///     Appends the given string to current output being built.
        /// </summary>
        /// <param name="value"> The string to append. </param>
        /// <returns> This printer so additional calls can be chained. </returns>
        public virtual ExpressionPrinter Append([NotNull] string value)
        {
            _stringBuilder.Append(value);
            return this;
        }

        /// <summary>
        ///     Creates a printable string representation of the given expression.
        /// </summary>
        /// <param name="expression"> The expression to print. </param>
        /// <param name="characterLimit"> An optional limit to the number of characters included. Additional output will be truncated. </param>
        /// <returns> The printable representation. </returns>
        public virtual string Print(
            [NotNull] Expression expression,
            int? characterLimit = null)
            => PrintCore(expression, characterLimit, verbose: false);

        /// <summary>
        ///     Creates a printable verbose string representation of the given expression.
        /// </summary>
        /// <param name="expression"> The expression to print. </param>
        /// <returns> The printable representation. </returns>
        public virtual string PrintDebug(
            [NotNull] Expression expression)
            => PrintCore(expression, characterLimit: null, verbose: true);

        private string PrintCore(
            [NotNull] Expression expression,
            int? characterLimit,
            bool verbose)
        {
            Check.NotNull(expression, nameof(expression));

            _stringBuilder.Clear();
            _parametersInScope.Clear();
            _namelessParameters.Clear();
            _encounteredParameters.Clear();

            CharacterLimit = characterLimit;
            Verbose = verbose;

            Visit(expression);

            var queryPlan = PostProcess(_stringBuilder.ToString());

            if (characterLimit != null
                && characterLimit.Value > 0)
            {
                queryPlan = queryPlan.Length > characterLimit
                    ? queryPlan.Substring(0, characterLimit.Value) + "..."
                    : queryPlan;
            }

            return queryPlan;
        }

        /// <summary>
        ///     Returns binary operator string corresponding to given <see cref="ExpressionType" />.
        /// </summary>
        /// <param name="expressionType"> The expression type to generate binary operator for. </param>
        /// <returns> The binary operator string. </returns>
        public virtual string GenerateBinaryOperator(ExpressionType expressionType)
        {
            return _binaryOperandMap[expressionType];
        }

        /// <inheritdoc />
        public override Expression Visit(Expression expression)
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
            Check.NotNull(binaryExpression, nameof(binaryExpression));

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
            Check.NotNull(blockExpression, nameof(blockExpression));

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

                var expressions = blockExpression.Result != null
                    ? blockExpression.Expressions.Except(new[] { blockExpression.Result })
                    : blockExpression.Expressions;

                foreach (var expression in expressions)
                {
                    Visit(expression);
                    AppendLine(";");
                }

                if (blockExpression.Result != null)
                {
                    Append("return ");
                    Visit(blockExpression.Result);
                    AppendLine(";");
                }
            }

            Append("}");

            return blockExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

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
            Check.NotNull(constantExpression, nameof(constantExpression));

            if (constantExpression.Value is IPrintableExpression printable)
            {
                printable.Print(this);
            }
            else
            {
                Print(constantExpression.Value);
            }

            return constantExpression;
        }

        private void Print(object value)
        {
            if (value is IEnumerable enumerable
                && !(value is string))
            {
                _stringBuilder.Append(value.GetType().ShortDisplayName() + " { ");
                foreach (var item in enumerable)
                {
                    Print(item);
                    _stringBuilder.Append(", ");
                }

                _stringBuilder.Append("}");
                return;
            }

            var stringValue = value == null
                ? "null"
                : value.ToString() != value.GetType().ToString()
                    ? value.ToString()
                    : value.GetType().ShortDisplayName();

            if (value != null
                && value is string)
            {
                stringValue = $@"""{stringValue}""";
            }

            _stringBuilder.Append(stringValue);
        }

        /// <inheritdoc />
        protected override Expression VisitGoto(GotoExpression gotoExpression)
        {
            Check.NotNull(gotoExpression, nameof(gotoExpression));

            AppendLine("return (" + gotoExpression.Target.Type.ShortDisplayName() + ")" + gotoExpression.Target + " {");
            using (_stringBuilder.Indent())
            {
                Visit(gotoExpression.Value);
            }

            _stringBuilder.Append("}");

            return gotoExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitLabel(LabelExpression labelExpression)
        {
            Check.NotNull(labelExpression, nameof(labelExpression));

            _stringBuilder.Append(labelExpression.Target.ToString());

            return labelExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            Check.NotNull(lambdaExpression, nameof(lambdaExpression));

            if (lambdaExpression.Parameters.Count != 1)
            {
                _stringBuilder.Append("(");
            }

            foreach (var parameter in lambdaExpression.Parameters)
            {
                var parameterName = parameter.Name;

                if (!_parametersInScope.ContainsKey(parameter))
                {
                    _parametersInScope.Add(parameter, parameterName);
                }

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
            Check.NotNull(memberExpression, nameof(memberExpression));

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
                _stringBuilder.Append(memberExpression.Member.DeclaringType.Name);
            }

            _stringBuilder.Append("." + memberExpression.Member.Name);

            return memberExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            Check.NotNull(memberInitExpression, nameof(memberInitExpression));

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
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Object != null)
            {
                if (methodCallExpression.Object is BinaryExpression)
                {
                    _stringBuilder.Append("(");
                    Visit(methodCallExpression.Object);
                    _stringBuilder.Append(")");
                }
                else
                {
                    Visit(methodCallExpression.Object);
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
                if (method.Name == nameof(Enumerable.Cast)
                    || method.Name == nameof(Enumerable.OfType))
                {
                    PrintGenericArguments(method, _stringBuilder);
                }
            }
            else
            {
                if (method.IsStatic)
                {
                    _stringBuilder.Append(method.DeclaringType.ShortDisplayName()).Append(".");
                }

                _stringBuilder.Append(method.Name);
                PrintGenericArguments(method, _stringBuilder);
            }

            _stringBuilder.Append("(");

            var isSimpleMethodOrProperty = _simpleMethods.Contains(method.Name)
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
                        : new List<string>();

                IDisposable indent = null;

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
            Check.NotNull(newExpression, nameof(newExpression));

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

            IDisposable indent = null;
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

            if (!isAnonymousType)
            {
                _stringBuilder.Append(")");
            }
            else
            {
                _stringBuilder.Append(" }");
            }

            return newExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
        {
            Check.NotNull(newArrayExpression, nameof(newArrayExpression));

            var isComplex = newArrayExpression.Expressions.Count > 1;
            var appendAction = isComplex ? (Func<string, ExpressionVisitor>)AppendLine : Append;

            appendAction("new " + newArrayExpression.Type.GetElementType().ShortDisplayName() + "[]");
            appendAction("{ ");

            IDisposable indent = null;
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
            Check.NotNull(parameterExpression, nameof(parameterExpression));

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
                else if (parameterName.Contains("."))
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
                    Append(parameterExpression.Name);
                    Append(")");
                }
                else
                {
                    Append(parameterExpression.Name);
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
            Check.NotNull(unaryExpression, nameof(unaryExpression));

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
            Check.NotNull(defaultExpression, nameof(defaultExpression));

            _stringBuilder.Append("default(" + defaultExpression.Type.ShortDisplayName() + ")");

            return defaultExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitTry(TryExpression tryExpression)
        {
            Check.NotNull(tryExpression, nameof(tryExpression));

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
            Check.NotNull(indexExpression, nameof(indexExpression));

            Visit(indexExpression.Object);
            _stringBuilder.Append("[");
            VisitArguments(
                indexExpression.Arguments, s =>
                {
                    _stringBuilder.Append(s);
                    return null;
                });
            _stringBuilder.Append("]");

            return indexExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            Check.NotNull(typeBinaryExpression, nameof(typeBinaryExpression));

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

                using (var indent = _stringBuilder.Indent())
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
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

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
            Func<string, ExpressionVisitor> appendAction,
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

        private string PostProcess([NotNull] string printedExpression)
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
}
