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
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ExpressionPrinter : ExpressionVisitor
    {
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

        public ExpressionPrinter()
        {
            _stringBuilder = new IndentedStringBuilder();
            _parametersInScope = new Dictionary<ParameterExpression, string>();
            _namelessParameters = new List<ParameterExpression>();
            _encounteredParameters = new List<ParameterExpression>();
        }

        private int? CharacterLimit { get; set; }

        private bool GenerateUniqueParameterIds { get; set; }

        public virtual void VisitList<T>(
            IReadOnlyList<T> items,
            Action<ExpressionPrinter> joinAction = null)
            where T : Expression
        {
            joinAction ??= (p => p.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(this);
                }

                Visit(items[i]);
            }
        }

        public virtual ExpressionPrinter Append([NotNull] object o)
        {
            _stringBuilder.Append(o);
            return this;
        }

        public virtual ExpressionPrinter AppendLine()
        {
            _stringBuilder.AppendLine();
            return this;
        }

        public virtual ExpressionVisitor AppendLine([NotNull] object o)
        {
            _stringBuilder.AppendLine(o);
            return this;
        }

        public virtual ExpressionPrinter AppendLines([NotNull] object o, bool skipFinalNewline = false)
        {
            _stringBuilder.AppendLines(o, skipFinalNewline);
            return this;
        }

        public virtual IDisposable Indent() => _stringBuilder.Indent();

        private void Append([NotNull] string message) => _stringBuilder.Append(message);

        private void AppendLine([NotNull] string message)
        {
            _stringBuilder.AppendLine(message);
        }

        public virtual string Print(
            Expression expression,
            int? characterLimit = null)
            => PrintCore(expression, characterLimit, generateUniqueParameterIds: false);

        public virtual string PrintDebug(
            Expression expression,
            int? characterLimit = null,
            bool generateUniqueParameterIds = true)
            => PrintCore(expression, characterLimit, generateUniqueParameterIds);

        protected virtual string PrintCore(
            Expression expression,
            int? characterLimit,
            bool generateUniqueParameterIds)
        {
            _stringBuilder.Clear();
            _parametersInScope.Clear();
            _namelessParameters.Clear();
            _encounteredParameters.Clear();

            CharacterLimit = characterLimit;
            GenerateUniqueParameterIds = generateUniqueParameterIds;

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

        public virtual string GenerateBinaryOperator(ExpressionType expressionType)
        {
            return _binaryOperandMap[expressionType];
        }

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

                case ExpressionType.Extension:
                    VisitExtension(expression);
                    break;

                default:
                    UnhandledExpressionType(expression);
                    break;
            }

            return expression;
        }

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

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Visit(conditionalExpression.Test);

            _stringBuilder.Append(" ? ");

            Visit(conditionalExpression.IfTrue);

            _stringBuilder.Append(" : ");

            Visit(conditionalExpression.IfFalse);

            return conditionalExpression;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value is IPrintableExpression printable)
            {
                printable.Print(this);
            }
            else if (constantExpression.IsEntityQueryable())
            {
                _stringBuilder.Append($"DbSet<{constantExpression.Type.GetTypeInfo().GenericTypeArguments.First().ShortDisplayName()}>");
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

        protected override Expression VisitGoto(GotoExpression gotoExpression)
        {
            AppendLine("return (" + gotoExpression.Target.Type.ShortDisplayName() + ")" + gotoExpression.Target + " {");
            using (_stringBuilder.Indent())
            {
                Visit(gotoExpression.Value);
            }

            _stringBuilder.Append("}");

            return gotoExpression;
        }

        protected override Expression VisitLabel(LabelExpression labelExpression)
        {
            _stringBuilder.Append(labelExpression.Target.ToString());

            return labelExpression;
        }

        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
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
                _stringBuilder.Append(memberExpression.Member.DeclaringType.Name);
            }

            _stringBuilder.Append("." + memberExpression.Member.Name);

            return memberExpression;
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            _stringBuilder.Append("new " + memberInitExpression.Type.ShortDisplayName());

            var appendAction = memberInitExpression.Bindings.Count > 1 ? (Action<string>)AppendLine : Append;
            appendAction("{ ");
            using (_stringBuilder.Indent())
            {
                for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                {
                    if (memberInitExpression.Bindings[i] is MemberAssignment assignment)
                    {
                        _stringBuilder.Append(assignment.Member.Name + " = ");
                        Visit(assignment.Expression);
                        appendAction(i == memberInitExpression.Bindings.Count - 1 ? " " : ", ");
                    }
                    else
                    {
                        AppendLine(CoreStrings.InvalidMemberInitBinding);
                    }
                }
            }

            AppendLine("}");

            return memberInitExpression;
        }

        private static readonly List<string> _simpleMethods = new List<string>
        {
            "get_Item",
            "TryReadValue",
            "ReferenceEquals"
        };

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
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

            // TODO: issue #18413
            var extensionMethod = !GenerateUniqueParameterIds
                && methodCallExpression.Arguments.Count > 0
                && method.IsDefined(typeof(ExtensionAttribute), inherit: false);

            if (extensionMethod)
            {
                Visit(methodArguments[0]);
                _stringBuilder.IncrementIndent();
                _stringBuilder.AppendLine();
                _stringBuilder.Append($".{method.Name}");
                methodArguments = methodArguments.Skip(1).ToList();
            }
            else
            {
                if (method.IsStatic)
                {
                    _stringBuilder.Append(method.DeclaringType.ShortDisplayName()).Append(".");
                }

                _stringBuilder.Append(method.Name);
                if (method.IsGenericMethod)
                {
                    _stringBuilder.Append("<");
                    var first = true;
                    foreach (var genericArgument in method.GetGenericArguments())
                    {
                        if (!first)
                        {
                            _stringBuilder.Append(", ");
                        }

                        _stringBuilder.Append(genericArgument.ShortDisplayName());
                        first = false;
                    }

                    _stringBuilder.Append(">");
                }
            }

            _stringBuilder.Append("(");

            var isSimpleMethodOrProperty = _simpleMethods.Contains(method.Name)
                || methodArguments.Count < 2
                || method.IsEFPropertyMethod();

            var appendAction = isSimpleMethodOrProperty ? (Action<string>)Append : AppendLine;

            if (methodArguments.Count > 0)
            {
                appendAction("");

                var argumentNames
                    = !isSimpleMethodOrProperty
                        ? method.GetParameters().Select(p => p.Name).ToList()
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
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            _stringBuilder.Append("new ");

            var isComplex = newExpression.Arguments.Count > 1;
            var appendAction = isComplex ? (Action<string>)AppendLine : Append;

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

        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
        {
            var isComplex = newArrayExpression.Expressions.Count > 1;
            var appendAction = isComplex ? (Action<string>)AppendLine : Append;

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

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (_parametersInScope.ContainsKey(parameterExpression))
            {
                var parameterName = _parametersInScope[parameterExpression];
                if (parameterName == null)
                {
                    if (!_namelessParameters.Contains(parameterExpression))
                    {
                        _namelessParameters.Add(parameterExpression);
                    }

                    Append("namelessParameter{");
                    Append(_namelessParameters.IndexOf(parameterExpression));
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
                // TODO: issue #18413
                if (GenerateUniqueParameterIds)
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

            if (GenerateUniqueParameterIds)
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

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
        {
            _stringBuilder.Append("default(" + defaultExpression.Type.ShortDisplayName() + ")");

            return defaultExpression;
        }

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

        protected override Expression VisitIndex(IndexExpression indexExpression)
        {
            Visit(indexExpression.Object);
            _stringBuilder.Append("[");
            VisitArguments(indexExpression.Arguments, s => _stringBuilder.Append(s));
            _stringBuilder.Append("]");

            return indexExpression;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            _stringBuilder.Append("(");
            Visit(typeBinaryExpression.Expression);
            _stringBuilder.Append(" is " + typeBinaryExpression.TypeOperand.ShortDisplayName() + ")");

            return typeBinaryExpression;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is IPrintableExpression printable)
            {
                _stringBuilder.Append("(");
                printable.Print(this);
                _stringBuilder.Append(")");
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

        protected virtual string PostProcess([NotNull] string printedExpression)
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
