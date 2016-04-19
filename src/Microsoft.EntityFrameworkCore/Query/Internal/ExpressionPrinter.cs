// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class ExpressionPrinter : ExpressionVisitor, IExpressionPrinter
    {
        private IndentedStringBuilder _stringBuilder;
        private List<IConstantPrinter> _constantPrinters;
        private Dictionary<ParameterExpression, string> _parametersInScope;

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
            { ExpressionType.Modulo, " % " }
        };

        protected static Action<IndentedStringBuilder, string> Append
        {
            get { return (sb, s) => sb.Append(s); }
        }

        protected static Action<IndentedStringBuilder, string> AppendLine
        {
            get { return (sb, s) => sb.AppendLine(s); }
        }

        public virtual string Print(Expression expression)
        {
            _stringBuilder = new IndentedStringBuilder();
            _parametersInScope = new Dictionary<ParameterExpression, string>();
            _constantPrinters = GetConstantPrinters();

            Visit(expression);

            var queryPlan = PostProcess(_stringBuilder.ToString());

            var result = "TRACKED: " + TrackedQuery + Environment.NewLine;
            result += queryPlan;

            return result;
        }

        protected virtual List<IConstantPrinter> GetConstantPrinters()
            => new List<IConstantPrinter>
            {
                new CollectionConstantPrinter(),
                new MetadataPropertyPrinter(),
                new DefaultConstantPrinter()
            };

        public virtual bool TrackedQuery { get; private set; }

        public override Expression Visit([NotNull] Expression node)
        {
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
                    VisitUnary((UnaryExpression)node);
                    break;

                case ExpressionType.Default:
                    VisitDefault((DefaultExpression)node);
                    break;

                default:
                    UnhandledExpressionType(node.NodeType);
                    break;
            }

            return node;
        }

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
                    UnhandledExpressionType(node.NodeType);
                }
                else
                {
                    _stringBuilder.Append(operand);
                }

                Visit(node.Right);
            }

            return node;
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            _stringBuilder.AppendLine();
            _stringBuilder.AppendLine("{");
            _stringBuilder.IncrementIndent();

            foreach (var variable in node.Variables)
            {
                string variableName;
                if (_parametersInScope.ContainsKey(variable))
                {
                    variableName = _parametersInScope[variable];
                }
                else
                {
                    variableName = "var" + _parametersInScope.Count;
                    _parametersInScope.Add(variable, variableName);
                }

                _stringBuilder.Append("var " + variableName);
            }

            if (node.Variables.Count > 0)
            {
                _stringBuilder.AppendLine();
            }

            foreach (var expression in node.Expressions)
            {
                Visit(expression);
                _stringBuilder.AppendLine();
            }

            _stringBuilder.DecrementIndent();
            _stringBuilder.AppendLine("}");

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.Test);

            _stringBuilder.Append(" ? ");

            Visit(node.IfTrue);

            _stringBuilder.Append(" : ");

            Visit(node.IfFalse);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            foreach (var constantPrinter in _constantPrinters)
            {
                if (constantPrinter.TryPrintConstant(node.Value, _stringBuilder))
                {
                    break;
                }
            }

            return node;
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            _stringBuilder.AppendLine("return (" + node.Target.Type.DisplayName(fullName: false) + ")" + node.Target + " {");
            _stringBuilder.IncrementIndent();

            Visit(node.Value);

            _stringBuilder.DecrementIndent();
            _stringBuilder.Append("}");

            return node;
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            _stringBuilder.Append(node.Target.ToString());

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _stringBuilder.Append("(");

            foreach (var parameter in node.Parameters)
            {
                _parametersInScope.Add(parameter, parameter.Name);
                _stringBuilder.Append(parameter.Type.DisplayName(fullName: false) + " " + parameter.Name);

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

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
            {
                Visit(node.Expression);
            }
            else
            {
                _stringBuilder.Append(node.Member.DeclaringType.Name);
            }

            _stringBuilder.Append("." + node.Member.Name);

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            _stringBuilder.Append("new " + node.Type.DisplayName(fullName: false));

            var appendAction = node.Bindings.Count > 1 ? AppendLine : Append;
            appendAction(_stringBuilder, "{ ");
            _stringBuilder.IncrementIndent();

            for (var i = 0; i < node.Bindings.Count; i++)
            {
                var assignment = node.Bindings[i] as MemberAssignment;
                if (assignment != null)
                {
                    _stringBuilder.Append(assignment.Member.Name + " = " + Visit(assignment.Expression));
                    appendAction(_stringBuilder, i == node.Bindings.Count - 1 ? " " : ", ");
                }
                else
                {
                    ////throw new NotSupportedException(CoreStrings.InvalidMemberInitBinding);
                    _stringBuilder.AppendLine(CoreStrings.InvalidMemberInitBinding);
                }
            }

            _stringBuilder.DecrementIndent();
            _stringBuilder.AppendLine("}");

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var simpleMethods = new List<string>
            {
                "get_Item"
            };

            if (node.Method.Name == "_InterceptExceptions")
            {
                Visit(node.Arguments[0]);

                return node;
            }

            if (node.Method.Name == "_TrackEntities")
            {
                TrackedQuery = true;
                Visit(node.Arguments[0]);

                return node;
            }

            if (node.Method.ReturnType != null)
            {
                _stringBuilder.Append(node.Method.ReturnType.DisplayName(fullName: false) + " ");
            }

            if (node.Object != null)
            {
                Visit(node.Object);
                _stringBuilder.Append(".");
            }

            _stringBuilder.Append(node.Method.Name + "(");

            var appendAction = simpleMethods.Contains(node.Method.Name) ? Append : AppendLine;
            if (node.Arguments.Count > 0)
            {
                appendAction(_stringBuilder, "");

                var showArgumentNames = !simpleMethods.Contains(node.Method.Name);
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

                    appendAction(_stringBuilder, i == node.Arguments.Count - 1 ? "" : ", ");
                }

                _stringBuilder.DecrementIndent();
            }

            appendAction(_stringBuilder, ")");

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _stringBuilder.Append("new ");
            _stringBuilder.Append(node.Type.DisplayName(fullName: false));

            var appendAction = node.Arguments.Count > 1 ? AppendLine : Append;
            appendAction(_stringBuilder, "(");
            _stringBuilder.IncrementIndent();

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                Visit(node.Arguments[i]);
                appendAction(_stringBuilder, i == node.Arguments.Count - 1 ? "" : ", ");
            }

            _stringBuilder.DecrementIndent();
            _stringBuilder.Append(")");

            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var appendAction = node.Expressions.Count > 1 ? AppendLine : Append;
            appendAction(_stringBuilder, "new " + node.Type.GetElementType().DisplayName(fullName: false) + "[]");
            appendAction(_stringBuilder, "{ ");
            _stringBuilder.IncrementIndent();

            for (var i = 0; i < node.Expressions.Count; i++)
            {
                Visit(node.Expressions[i]);
                appendAction(_stringBuilder, i == node.Expressions.Count - 1 ? " " : ", ");
            }

            _stringBuilder.DecrementIndent();
            _stringBuilder.AppendLine("}");

            return node;
        }

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

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert)
            {
                _stringBuilder.Append("(" + node.Type.DisplayName(fullName: false) + ") ");

                Visit(node.Operand);

                return node;
            }

            if (node.NodeType == ExpressionType.Throw)
            {
                _stringBuilder.Append("throw ");
                Visit(node.Operand);

                return node;
            }

            _stringBuilder.AppendLine(CoreStrings.UnhandledNodeType(node.NodeType));

            return node;
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            _stringBuilder.Append("default(" + node.Type + ")");

            return node;
        }

        protected virtual string PostProcess([NotNull] string queryPlan)
        {
            var processedPlan = queryPlan
                .Replace("Microsoft.EntityFrameworkCore.Query.", "")
                .Replace("Microsoft.EntityFrameworkCore.", "")
                .Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            return processedPlan;
        }

        private void UnhandledExpressionType(ExpressionType expressionType)
            => _stringBuilder.AppendLine(CoreStrings.UnhandledExpressionType(expressionType));

        protected interface IConstantPrinter
        {
            bool TryPrintConstant([NotNull] object value, [NotNull] IndentedStringBuilder stringBuilder);
        }

        private class CollectionConstantPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
            {
                var enumerable = value as IEnumerable;
                if ((enumerable != null)
                    && !(value is string))
                {
                    var appendAction = value is byte[] ? Append : AppendLine;

                    appendAction(stringBuilder, value.GetType().DisplayName(fullName: false) + " ");
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

        private class MetadataPropertyPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
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

        private class DefaultConstantPrinter : IConstantPrinter
        {
            public bool TryPrintConstant(object value, IndentedStringBuilder stringBuilder)
            {
                var stringValue = "null";
                if (value != null)
                {
                    stringValue = value.ToString() != value.GetType().ToString()
                        ? value.ToString()
                        : value.GetType().Name;
                }

                stringBuilder.Append(stringValue);

                return true;
            }
        }
    }
}
