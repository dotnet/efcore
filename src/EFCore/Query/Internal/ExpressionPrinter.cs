// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq.Clauses;
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
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " }
        };

        private bool _highlightNonreducibleNodes;
        private bool _reduceBeforePrinting;

        private const string HighlightLeft = " ---> ";
        private const string HighlightRight = " <--- ";

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
        protected List<ConstantPrinterBase> ConstantPrinters = new List<ConstantPrinterBase>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ExpressionPrinter(List<ConstantPrinterBase> additionalConstantPrinters)
        {
            _stringBuilder = new IndentedStringBuilder();
            _parametersInScope = new Dictionary<ParameterExpression, string>();

            ConstantPrinters.AddRange(additionalConstantPrinters);

            ConstantPrinters.AddRange(
                new List<ConstantPrinterBase>
                {
                    new EntityQueryableConstantPrinter(),
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
        public virtual bool PrintConnections { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool GenerateUniqueQsreIds { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IQuerySource> VisitedQuerySources { get; } = new List<IQuerySource>();

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
        public virtual string Print(
            Expression expression,
            bool removeFormatting = false,
            int? characterLimit = null,
            bool printConnections = true)
        {
            return PrintInternal(
                expression,
                removeFormatting,
                characterLimit,
                highlightNonreducibleNodes: false,
                reduceBeforePrinting: false,
                generateUniqueQsreIds: false,
                printConnections: printConnections);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string PrintDebug(
            Expression expression,
            bool highlightNonreducibleNodes = true,
            bool reduceBeforePrinting = true,
            bool generateUniqueQsreIds = true,
            bool printConnections = true)
        {
            return PrintInternal(
                expression,
                removeFormatting: false,
                characterLimit: null,
                highlightNonreducibleNodes: highlightNonreducibleNodes,
                reduceBeforePrinting: reduceBeforePrinting,
                generateUniqueQsreIds: generateUniqueQsreIds,
                printConnections: printConnections);
        }

        private string PrintInternal(
            Expression expression,
            bool removeFormatting,
            int? characterLimit,
            bool highlightNonreducibleNodes,
            bool reduceBeforePrinting,
            bool generateUniqueQsreIds,
            bool printConnections)
        {
            _stringBuilder.Clear();
            _parametersInScope.Clear();

            RemoveFormatting = removeFormatting;
            CharacterLimit = characterLimit;
            GenerateUniqueQsreIds = generateUniqueQsreIds;
            PrintConnections = printConnections;

            _highlightNonreducibleNodes = highlightNonreducibleNodes;
            _reduceBeforePrinting = reduceBeforePrinting;

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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBlock(BlockExpression blockExpression)
        {
            AppendLine();

            if (PrintConnections)
            {
                _stringBuilder.SuspendCurrentNode();
            }

            AppendLine("{");
            _stringBuilder.IncrementIndent();

            foreach (var variable in blockExpression.Variables)
            {
                if (!_parametersInScope.ContainsKey(variable))
                {
                    _parametersInScope.Add(variable, variable.Name);
                }
            }

            var expressions = blockExpression.Result != null
                ? blockExpression.Expressions.Except(new[] { blockExpression.Result })
                : blockExpression.Expressions;

            foreach (var expression in expressions)
            {
                Visit(expression);
                AppendLine();
            }

            if (blockExpression.Result != null)
            {
                Append("return ");
                Visit(blockExpression.Result);
                AppendLine();
            }

            _stringBuilder.DecrementIndent();
            Append("}");

            if (PrintConnections)
            {
                _stringBuilder.ReconnectCurrentNode();
            }

            return blockExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Visit(conditionalExpression.Test);

            _stringBuilder.Append(" ? ");

            Visit(conditionalExpression.IfTrue);

            _stringBuilder.Append(" : ");

            Visit(conditionalExpression.IfFalse);

            return conditionalExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (PrintConnections)
            {
                _stringBuilder.SuspendCurrentNode();
            }

            foreach (var constantPrinter in ConstantPrinters)
            {
                if (constantPrinter.TryPrintConstant(constantExpression, _stringBuilder, RemoveFormatting))
                {
                    break;
                }
            }

            if (PrintConnections)
            {
                _stringBuilder.ReconnectCurrentNode();
            }

            return constantExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitGoto(GotoExpression gotoExpression)
        {
            AppendLine("return (" + gotoExpression.Target.Type.ShortDisplayName() + ")" + gotoExpression.Target + " {");
            _stringBuilder.IncrementIndent();

            Visit(gotoExpression.Value);

            _stringBuilder.DecrementIndent();
            _stringBuilder.Append("}");

            return gotoExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitLabel(LabelExpression labelExpression)
        {
            _stringBuilder.Append(labelExpression.Target.ToString());

            return labelExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            _stringBuilder.Append("(");

            foreach (var parameter in lambdaExpression.Parameters)
            {
                var parameterName = parameter.Name ?? parameter.ToString();

                if (!_parametersInScope.ContainsKey(parameter))
                {
                    _parametersInScope.Add(parameter, parameterName);
                }

                _stringBuilder.Append(parameter.Type.ShortDisplayName() + " " + parameterName);

                if (parameter != lambdaExpression.Parameters.Last())
                {
                    _stringBuilder.Append(" | ");
                }
            }

            _stringBuilder.Append(") => ");

            Visit(lambdaExpression.Body);

            foreach (var parameter in lambdaExpression.Parameters)
            {
                _parametersInScope.Remove(parameter);
            }

            return lambdaExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression != null)
            {
                if (memberExpression.Expression.NodeType == ExpressionType.Convert)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMemberInit(MemberInitExpression memeberInitExpression)
        {
            _stringBuilder.Append("new " + memeberInitExpression.Type.ShortDisplayName());

            var appendAction = memeberInitExpression.Bindings.Count > 1 ? (Action<string>)AppendLine : Append;
            appendAction("{ ");
            _stringBuilder.IncrementIndent();

            for (var i = 0; i < memeberInitExpression.Bindings.Count; i++)
            {
                if (memeberInitExpression.Bindings[i] is MemberAssignment assignment)
                {
                    _stringBuilder.Append(assignment.Member.Name + " = ");
                    Visit(assignment.Expression);
                    appendAction(i == memeberInitExpression.Bindings.Count - 1 ? " " : ", ");
                }
                else
                {
                    ////throw new NotSupportedException(CoreStrings.InvalidMemberInitBinding);
                    AppendLine(CoreStrings.InvalidMemberInitBinding);
                }
            }

            _stringBuilder.DecrementIndent();
            AppendLine("}");

            return memeberInitExpression;
        }

        private static readonly List<string> _simpleMethods = new List<string>
        {
            "get_Item",
            "TryReadValue",
            "ReferenceEquals"
        };

        private static readonly List<string> _nonConnectableMethods = new List<string>
        {
            "GetValueFromEntity",
            "StartTracking",
            "SetRelationshipSnapshotValue",
            "SetRelationshipIsLoaded",
            "Add"
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (!methodCallExpression.Method.IsEFPropertyMethod())
            {
                _stringBuilder.Append(methodCallExpression.Method.ReturnType.ShortDisplayName() + " ");
            }

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

            _stringBuilder.Append(methodCallExpression.Method.Name + "(");

            var isSimpleMethodOrProperty = _simpleMethods.Contains(methodCallExpression.Method.Name)
                                           || methodCallExpression.Arguments.Count < 2
                                           || methodCallExpression.Method.IsEFPropertyMethod();

            var appendAction = isSimpleMethodOrProperty ? (Action<string>)Append : AppendLine;

            if (methodCallExpression.Arguments.Count > 0)
            {
                appendAction("");

                var argumentNames
                    = !isSimpleMethodOrProperty
                        ? methodCallExpression.Method.GetParameters().Select(p => p.Name).ToList()
                        : new List<string>();

                if (!isSimpleMethodOrProperty)
                {
                    var shouldPrintConnections = PrintConnections && !_nonConnectableMethods.Contains(methodCallExpression.Method.Name);
                    _stringBuilder.IncrementIndent(shouldPrintConnections);
                }

                for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
                {
                    var argument = methodCallExpression.Arguments[i];

                    if (!isSimpleMethodOrProperty)
                    {
                        _stringBuilder.Append(argumentNames[i] + ": ");
                    }

                    if (i == methodCallExpression.Arguments.Count - 1
                        && !isSimpleMethodOrProperty)
                    {
                        _stringBuilder.DisconnectCurrentNode();
                    }

                    Visit(argument);

                    if (i < methodCallExpression.Arguments.Count - 1)
                    {
                        appendAction(", ");
                    }
                }

                if (!isSimpleMethodOrProperty)
                {
                    _stringBuilder.DecrementIndent();
                }
            }

            Append(")");

            return methodCallExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

            if (isComplex)
            {
                _stringBuilder.IncrementIndent();
            }

            VisitArguments(newExpression.Arguments, appendAction);

            if (isComplex)
            {
                _stringBuilder.DecrementIndent();
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
        {
            var isComplex = newArrayExpression.Expressions.Count > 1;
            var appendAction = isComplex ? (Action<string>)AppendLine : Append;

            appendAction("new " + newArrayExpression.Type.GetElementType().ShortDisplayName() + "[]");

            if (PrintConnections)
            {
                _stringBuilder.SuspendCurrentNode();
            }

            appendAction("{ ");

            if (isComplex)
            {
                _stringBuilder.IncrementIndent();
            }

            VisitArguments(newArrayExpression.Expressions, appendAction, lastSeparator: " ");

            if (isComplex)
            {
                _stringBuilder.DecrementIndent();
            }

            Append("}");

            if (PrintConnections)
            {
                _stringBuilder.ReconnectCurrentNode();
            }

            return newArrayExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (_parametersInScope.ContainsKey(parameterExpression))
            {
                _stringBuilder.Append(_parametersInScope[parameterExpression]);
            }
            else
            {
                _stringBuilder.Append("Unhandled parameter: " + parameterExpression);
            }

            return parameterExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Convert:
                    _stringBuilder.Append("(" + unaryExpression.Type.ShortDisplayName() + ")");
                    Visit(unaryExpression.Operand);
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

                default:
                    UnhandledExpressionType(unaryExpression);
                    break;
            }

            return unaryExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitDefault(DefaultExpression defaultExpression)
        {
            _stringBuilder.Append("default(" + defaultExpression.Type.ShortDisplayName() + ")");

            return defaultExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitIndex(IndexExpression indexExpression)
        {
            Visit(indexExpression.Object);
            _stringBuilder.Append("[");
            VisitArguments(indexExpression.Arguments, s => _stringBuilder.Append(s));
            _stringBuilder.Append("]");

            return indexExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            _stringBuilder.Append("(");
            Visit(typeBinaryExpression.Expression);
            _stringBuilder.Append(" is " + typeBinaryExpression.TypeOperand.ShortDisplayName() + ")");

            return typeBinaryExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (_highlightNonreducibleNodes && !extensionExpression.CanReduce)
            {
                StringBuilder.Append(HighlightLeft);
            }

            if (_reduceBeforePrinting && extensionExpression.CanReduce)
            {
                var reduced = extensionExpression.Reduce();
                Visit(reduced);

                return extensionExpression;
            }

            if (extensionExpression is IPrintable printable)
            {
                printable.Print(this);
            }
            else
            {
                switch (extensionExpression)
                {
                    case QuerySourceReferenceExpression qsre:
                        if (GenerateUniqueQsreIds)
                        {
                            var index = VisitedQuerySources.IndexOf(qsre.ReferencedQuerySource);
                            StringBuilder.Append("[" + qsre.ReferencedQuerySource.ItemName + "{" + index + "}]");
                        }
                        else
                        {
                            StringBuilder.Append(qsre);
                        }

                        break;

                    case SubQueryExpression subqueryExpression:
                        VisitSubqueryExpression(subqueryExpression);
                        break;

                    default:
                        UnhandledExpressionType(extensionExpression);
                        break;
                }
            }

            if (_highlightNonreducibleNodes && !extensionExpression.CanReduce)
            {
                StringBuilder.Append(HighlightRight);
            }

            return extensionExpression;
        }

        private void VisitSubqueryExpression(SubQueryExpression subqueryExpression)
        {
            _stringBuilder.Append(subqueryExpression.QueryModel.Print());
        }

        private void VisitArguments(IList<Expression> arguments, Action<string> appendAction, string lastSeparator = "", bool areConnected = false)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                if (areConnected && i == arguments.Count - 1)
                {
                    Append("");
                    _stringBuilder.DisconnectCurrentNode();
                }

                Visit(arguments[i]);
                appendAction(i == arguments.Count - 1 ? lastSeparator : ", ");
            }
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

        private void UnhandledExpressionType(Expression expression)
            => AppendLine(expression.ToString());

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
            public abstract bool TryPrintConstant(
                [NotNull] ConstantExpression constantExpression,
                [NotNull] IndentedStringBuilder stringBuilder,
                bool removeFormatting);

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
            public override bool TryPrintConstant(
                ConstantExpression constantExpression,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                if (constantExpression.IsEntityQueryable())
                {
                    stringBuilder.Append($"DbSet<{constantExpression.Type.GetTypeInfo().GenericTypeArguments.First().ShortDisplayName()}>");
                    return true;
                }

                return false;
            }
        }

        private class MetadataPropertyPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(
                ConstantExpression constantExpression,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                if (constantExpression.Value is PropertyBase property)
                {
                    stringBuilder.Append(property.DeclaringType.ClrType.Name + "." + property.Name);

                    return true;
                }

                return false;
            }
        }

        private class DefaultConstantPrinter : ConstantPrinterBase
        {
            public override bool TryPrintConstant(
                ConstantExpression constantExpression,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                Print(constantExpression.Value, stringBuilder, removeFormatting);

                return true;
            }

            private void Print(
                object value,
                IndentedStringBuilder stringBuilder,
                bool removeFormatting)
            {
                if (value is IEnumerable enumerable
                    && !(value is string))
                {
                    var appendAction = value is byte[] || removeFormatting ? Append : AppendLine;

                    appendAction(stringBuilder, value.GetType().ShortDisplayName() + " ");
                    appendAction(stringBuilder, "{ ");
                    stringBuilder.IncrementIndent();
                    foreach (var item in enumerable)
                    {
                        Print(item, stringBuilder, removeFormatting);

                        appendAction(stringBuilder, ", ");
                    }

                    stringBuilder.DecrementIndent();

                    stringBuilder.Append("}");

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

                stringBuilder.Append(stringValue);
            }
        }
    }
}
