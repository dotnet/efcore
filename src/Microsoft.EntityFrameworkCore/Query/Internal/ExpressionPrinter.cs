// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ExpressionPrinter : ExpressionVisitorBase, IExpressionPrinter
    {
        private readonly IndentedStringBuilder _stringBuilder;
        private readonly List<ConstantPrinterBase> _constantPrinters;
        private readonly Dictionary<ParameterExpression, string> _parametersInScope;

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
            { ExpressionType.Divide, " * " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ExpressionPrinter()
            : this(new List<ConstantPrinterBase>())
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ExpressionPrinter(List<ConstantPrinterBase> constantPrinters)
        {
            _stringBuilder = new IndentedStringBuilder();
            _parametersInScope = new Dictionary<ParameterExpression, string>();
            _constantPrinters = new List<ConstantPrinterBase>(constantPrinters);
            _constantPrinters.AddRange(
                new List<ConstantPrinterBase>
                {
                    new EntityQueryableConstantPrinter(),
                    new CollectionConstantPrinter(),
                    new MetadataPropertyPrinter(),
                    new DefaultConstantPrinter()
                });
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IndentedStringBuilder StringBuilder => _stringBuilder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RemoveFormatting { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int? CharacterLimit { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void Append([NotNull] string message) => _stringBuilder.Append(message);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void AppendLine([NotNull] string message = "")
        {
            if (RemoveFormatting)
            {
                _stringBuilder.Append(string.IsNullOrEmpty(message) ? " " : message);
            }

            _stringBuilder.AppendLine(message);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Print(Expression expression, bool removeFormatting = false, int? characterLimit = null)
        {
            _stringBuilder.Clear();
            _parametersInScope.Clear();
                 
            RemoveFormatting = removeFormatting;
            CharacterLimit = characterLimit;

            Visit(expression);

            var queryPlan = PostProcess(_stringBuilder.ToString());

            var result = "TRACKED: " + TrackedQuery + (removeFormatting ? " " : Environment.NewLine);
            result += queryPlan;

            if (characterLimit != null && characterLimit.Value > 0)
            {
                result = result.Length > characterLimit
                    ? result.Substring(0, characterLimit.Value) + "..."
                    : result;
            }

            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TrackedQuery { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            if (CharacterLimit != null && _stringBuilder.Length > CharacterLimit.Value)
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
                    VisitNewArray((NewArrayExpression)node);
                    break;

                case ExpressionType.Parameter:
                    VisitParameter((ParameterExpression)node);
                    break;

                case ExpressionType.Convert:
                case ExpressionType.Throw:
                case ExpressionType.Not:
                    VisitUnary((UnaryExpression)node);
                    break;

                case ExpressionType.Default:
                    VisitDefault((DefaultExpression)node);
                    break;

                case ExpressionType.Try:
                    VisitTry((TryExpression)node);
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                string operand;
                if (!_binaryOperandMap.TryGetValue(node.NodeType, out operand))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBlock(BlockExpression node)
        {
            AppendLine();
            AppendLine("{");
            _stringBuilder.IncrementIndent();

            foreach (var variable in node.Variables)
            {
                if (!_parametersInScope.ContainsKey(variable))
                {
                    _parametersInScope.Add(variable, "var" + _parametersInScope.Count);
                }
            }

            var expressions = node.Result != null
                ? node.Expressions.Except(new[] { node.Result })
                : node.Expressions;

            foreach (var expression in expressions)
            {
                Visit(expression);
                AppendLine();
            }

            if (node.Result != null)
            {
                AppendLine("return " + node.Result);
            }

            _stringBuilder.DecrementIndent();
            AppendLine("}");

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.Test);

            _stringBuilder.Append(" ? ");

            Visit(node.IfTrue);

            _stringBuilder.Append(" : ");

            Visit(node.IfFalse);

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            foreach (var constantPrinter in _constantPrinters)
            {
                if (constantPrinter.TryPrintConstant(node.Value, _stringBuilder, RemoveFormatting))
                {
                    break;
                }
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitGoto(GotoExpression node)
        {
            AppendLine("return (" + node.Target.Type.ShortDisplayName() + ")" + node.Target + " {");
            _stringBuilder.IncrementIndent();

            Visit(node.Value);

            _stringBuilder.DecrementIndent();
            _stringBuilder.Append("}");

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitLabel(LabelExpression node)
        {
            _stringBuilder.Append(node.Target.ToString());

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _stringBuilder.Append("(");

            foreach (var parameter in node.Parameters)
            {
                _parametersInScope.Add(parameter, parameter.Name);
                _stringBuilder.Append(parameter.Type.ShortDisplayName() + " " + parameter.Name);

                if (parameter != node.Parameters.Last())
                {
                    _stringBuilder.Append(" | ");
                }
            }

            _stringBuilder.Append(") => ");

            Visit(node.Body);

            foreach (var parameter in node.Parameters)
            {
                _parametersInScope.Remove(parameter);
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
            {
                Visit(node.Expression);
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                _stringBuilder.Append(node.Member.DeclaringType.Name);
            }

            _stringBuilder.Append("." + node.Member.Name);

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            _stringBuilder.Append("new " + node.Type.ShortDisplayName());

            var appendAction = node.Bindings.Count > 1 ? (Action<string>)AppendLine : Append;
            appendAction("{ ");
            _stringBuilder.IncrementIndent();

            for (var i = 0; i < node.Bindings.Count; i++)
            {
                var assignment = node.Bindings[i] as MemberAssignment;
                if (assignment != null)
                {
                    _stringBuilder.Append(assignment.Member.Name + " = " + Visit(assignment.Expression));
                    appendAction(i == node.Bindings.Count - 1 ? " " : ", ");
                }
                else
                {
                    ////throw new NotSupportedException(CoreStrings.InvalidMemberInitBinding);
                    AppendLine(CoreStrings.InvalidMemberInitBinding);
                }
            }

            _stringBuilder.DecrementIndent();
            AppendLine("}");

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var simpleMethods = new List<string>
            {
                "get_Item",
                "TryReadValue"
            };

            switch (node.Method.Name)
            {
                case "_InterceptExceptions":
                    Visit(node.Arguments[0]);

                    return node;
                case "_TrackEntities":
                    TrackedQuery = true;
                    Visit(node.Arguments[0]);

                    return node;
            }

            if (!EntityQueryModelVisitor.IsPropertyMethod(node.Method))
            {
                _stringBuilder.Append(node.Method.ReturnType.ShortDisplayName() + " ");
            }

            if (node.Object != null)
            {
                Visit(node.Object);
                _stringBuilder.Append(".");
            }

            _stringBuilder.Append(node.Method.Name + "(");

            var appendAction = simpleMethods.Contains(node.Method.Name) || EntityQueryModelVisitor.IsPropertyMethod(node.Method)
                ? (Action<string>)Append
                : AppendLine;

            if (node.Arguments.Count > 0)
            {
                appendAction("");

                var showArgumentNames = !simpleMethods.Contains(node.Method.Name) && !EntityQueryModelVisitor.IsPropertyMethod(node.Method);
                var argumentNames = showArgumentNames ? node.Method.GetParameters().Select(p => p.Name).ToList() : new List<string>();

                _stringBuilder.IncrementIndent();
                for (var i = 0; i < node.Arguments.Count; i++)
                {
                    var argument = node.Arguments[i];

                    if (showArgumentNames)
                    {
                        _stringBuilder.Append(argumentNames[i] + ": ");
                    }

                    Visit(argument);

                    appendAction(i == node.Arguments.Count - 1 ? "" : ", ");
                }

                _stringBuilder.DecrementIndent();
            }

            appendAction(")");

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitNew(NewExpression node)
        {
            _stringBuilder.Append("new ");
            _stringBuilder.Append(node.Type.ShortDisplayName());

            var appendAction = node.Arguments.Count > 1 ? (Action<string>)AppendLine : Append;
            appendAction("(");
            _stringBuilder.IncrementIndent();

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                Visit(node.Arguments[i]);
                appendAction(i == node.Arguments.Count - 1 ? "" : ", ");
            }

            _stringBuilder.DecrementIndent();
            _stringBuilder.Append(")");

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var appendAction = node.Expressions.Count > 1 ? (Action<string>)AppendLine : Append;
            appendAction("new " + node.Type.GetElementType().ShortDisplayName() + "[]");
            appendAction("{ ");
            _stringBuilder.IncrementIndent();

            for (var i = 0; i < node.Expressions.Count; i++)
            {
                Visit(node.Expressions[i]);
                appendAction(i == node.Expressions.Count - 1 ? " " : ", ");
            }

            _stringBuilder.DecrementIndent();
            AppendLine("}");

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_parametersInScope.ContainsKey(node))
            {
                _stringBuilder.Append(_parametersInScope[node]);
            }
            else
            {
                _stringBuilder.Append("Unhandled parameter: " + node);
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                    _stringBuilder.Append("(" + node.Type.ShortDisplayName() + ") ");

                    Visit(node.Operand);

                    return node;
                case ExpressionType.Throw:
                    _stringBuilder.Append("throw ");
                    Visit(node.Operand);

                    return node;
                case ExpressionType.Not:
                    _stringBuilder.Append("!(");
                    Visit(node.Operand);
                    _stringBuilder.Append(")");

                    return node;
            }

            UnhandledExpressionType(node);

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitDefault(DefaultExpression node)
        {
            _stringBuilder.Append("default(" + node.Type + ")");

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            var qsre = node as QuerySourceReferenceExpression;
            if (qsre != null)
            {
                StringBuilder.Append(qsre.ReferencedQuerySource.ItemName);

                return node;
            }

            var nullConditional = node as NullConditionalExpression;
            if (nullConditional != null)
            {
                StringBuilder.Append(nullConditional.ToString());

                return node;
            }

            UnhandledExpressionType(node);

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual string PostProcess([NotNull] string queryPlan)
        {
            var processedPlan = queryPlan
                .Replace("Microsoft.EntityFrameworkCore.Query.", "")
                .Replace("Microsoft.EntityFrameworkCore.", "")
                .Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            return processedPlan;
        }

        private void UnhandledExpressionType(Expression e)
            => AppendLine(e.ToString());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract class ConstantPrinterBase
        {
            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public abstract bool TryPrintConstant([CanBeNull] object value, [NotNull] IndentedStringBuilder stringBuilder, bool removeFormatting);

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            protected virtual Action<IndentedStringBuilder, string> Append => (sb, s) => sb.Append(s);

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            protected virtual Action<IndentedStringBuilder, string> AppendLine => (sb, s) => sb.AppendLine(s);
        }

        private class EntityQueryableConstantPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder, bool removeFormatting)
            {
                if (value != null
                    && value.GetType().GetTypeInfo().IsGenericType
                    && value.GetType().GetTypeInfo().GetGenericTypeDefinition() == typeof(EntityQueryable<>))
                {
                    stringBuilder.Append($"DbSet<{value.GetType().GetTypeInfo().GenericTypeArguments.First().ShortDisplayName()}>");
                    return true;
                }

                return false;
            }
        }

        private class CollectionConstantPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder, bool removeFormatting)
            {
                var enumerable = value as IEnumerable;
                if ((enumerable != null)
                    && !(value is string))
                {
                    var appendAction = value is byte[] || removeFormatting ? Append : AppendLine;

                    appendAction(stringBuilder, value.GetType().ShortDisplayName() + " ");
                    appendAction(stringBuilder, "{ ");
                    stringBuilder.IncrementIndent();
                    foreach (var item in enumerable)
                    {
                        appendAction(stringBuilder, item + ", ");
                    }

                    stringBuilder.DecrementIndent();
                    appendAction(stringBuilder, "}");

                    return true;
                }

                return false;
            }
        }

        private class MetadataPropertyPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder, bool removeFormatting)
            {
                var property = value as Property;
                if (property != null)
                {
                    stringBuilder.Append(property.Name);

                    return true;
                }

                return false;
            }
        }

        private class DefaultConstantPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder, bool removeFormatting)
            {
                var stringValue = "null";

                if (value != null)
                {
                    stringValue = value.ToString() != value.GetType().ToString()
                        ? value.ToString()
                        : value.GetType().Name;

                    if (value is string)
                    {
                        stringValue = $@"""{stringValue}""";
                    }
                }

                stringBuilder.Append(stringValue);

                return true;
            }
        }
    }
}
