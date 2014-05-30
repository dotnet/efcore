// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    [DebuggerDisplay("TableFilter")]
    public class TableFilter
    {
        protected TableFilter()
        {
        }

        public MemberExpression Left { get; private set; }

        public object Right { get; protected set; }

        public FilterComparisonOperator ComparisonOperator { get; protected set; }

        public static TableFilter FromBinaryExpression([NotNull] BinaryExpression expression)
        {
            try
            {
                Check.NotNull(expression, "expression");

                var op = FilterComparison.FromNodeType(expression.NodeType);
                var tableFilterCtor = GetCtor(expression);
                try
                {
                    return tableFilterCtor.Invoke(new object[] { expression.Left, op, expression.Right }) as TableFilter;
                }
                catch (Exception)
                {
                    // swallow and try the reversed order
                    FilterComparison.FlipInequalities(ref op);
                    return tableFilterCtor.Invoke(new object[] { expression.Right, op, expression.Left }) as TableFilter;
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static TableFilter FromMemberExpression([NotNull] MemberExpression expression)
        {
            Check.NotNull(expression, "expression");
            if (expression.Type != typeof(bool))
            {
                return null;
            }
            try
            {
                return new ConstantTableFilter(expression, FilterComparisonOperator.Equal, Expression.Constant(true));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static TableFilter FromUnaryExpression(UnaryExpression expression)
        {
            Check.NotNull(expression, "expression");
            try
            {
                if (expression.Operand is MemberExpression
                    && expression.Operand.Type == typeof(bool))
                {
                    return new ConstantTableFilter((MemberExpression)expression.Operand, FilterComparisonOperator.Equal, Expression.Constant(expression.NodeType != ExpressionType.Not));
                }
                return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }

        private static ConstructorInfo GetCtor(BinaryExpression expression)
        {
            var rightExpression = expression.Left is MemberExpression ? expression.Right : expression.Left;
            Type ctorType, ctorParamType;
            if (rightExpression is ConstantExpression)
            {
                ctorType = typeof(ConstantTableFilter);
                ctorParamType = typeof(ConstantExpression);
            }
            else if (rightExpression is MemberExpression)
            {
                ctorType = typeof(MemberTableFilter);
                ctorParamType = typeof(MemberExpression);
            }
            else if (rightExpression is NewExpression)
            {
                ctorType = typeof(NewObjTableFilter<>).MakeGenericType(rightExpression.Type);
                ctorParamType = typeof(NewExpression);
            }
            else
            {
                throw new ArgumentOutOfRangeException("expression", "Cannot find a matching constructor");
            }

            return ctorType.GetConstructor(new[] { typeof(MemberExpression), typeof(FilterComparisonOperator), ctorParamType });
        }

        protected static MethodInfo FilterMethodForConstant(Type type)
        {
            if (type.IsAssignableFrom(typeof(string)))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterCondition");
            }
            else if (type.IsAssignableFrom(typeof(double)))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterConditionForDouble");
            }
            else if (type.IsAssignableFrom(typeof(int)))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterConditionForInt");
            }
            else if (type.IsAssignableFrom(typeof(long)))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterConditionForLong");
            }
            else if (type.IsAssignableFrom(typeof(byte[])))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterConditionForBinary");
            }
            else if (type.IsAssignableFrom(typeof(bool)))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterConditionForBool");
            }
            else if (type.IsAssignableFrom(typeof(DateTimeOffset)))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterConditionForDate");
            }
            else if (type.IsAssignableFrom(typeof(DateTime)))
            {
                var action = new Func<string, string, DateTime, string>(
                    (prop, op, time) => TableQuery.GenerateFilterConditionForDate(prop, op, new DateTimeOffset(time))
                    );
                return action.Method;
            }
            else if (type.IsAssignableFrom(typeof(Guid)))
            {
                return typeof(TableQuery).GetMethod("GenerateFilterConditionForGuid");
            }
            throw new ArgumentOutOfRangeException("type", "Cannot generate filter method for this type");
        }

        protected TableFilter(MemberExpression left, FilterComparisonOperator op)
        {
            if (left.Expression.NodeType == ExpressionType.MemberAccess)
            {
                throw new ArgumentException("Nested member access not supported", "left");
            }
            Left = left;
            ComparisonOperator = op;
        }
    }

    internal class ConstantTableFilter : TableFilter
    {
        private readonly MethodInfo _stringMethod;

        public ConstantTableFilter(MemberExpression left, FilterComparisonOperator op, ConstantExpression right)
            : base(left, op)
        {
            Right = right.Value;
            _stringMethod = FilterMethodForConstant(right.Type);
        }

        public override string ToString()
        {
            return (string)_stringMethod.Invoke(null, new object[] { Left.Member.Name, FilterComparison.ToString(ComparisonOperator), Right });
        }
    }

    internal class MemberTableFilter : TableFilter
    {
        private readonly MethodInfo _stringMethod;
        private readonly Func<object> _getTarget;
        public new FieldInfo Right { get; protected set; }

        public MemberTableFilter(MemberExpression left, FilterComparisonOperator op, MemberExpression right)
            : base(left, op)
        {
            Right = (FieldInfo)right.Member;
            _stringMethod = FilterMethodForConstant(Right.FieldType);
            if (right.Expression is ConstantExpression)
            {
                _getTarget = () => ((ConstantExpression)right.Expression).Value;
            }
            else
            {
                _getTarget = () => null;
            }
        }

        public override string ToString()
        {
            return (string)_stringMethod.Invoke(null, new[] { Left.Member.Name, FilterComparison.ToString(ComparisonOperator), Right.GetValue(_getTarget()) });
        }
    }

    internal class NewObjTableFilter<T> : TableFilter
    {
        private readonly ConstructorInfo objCtor;
        private readonly IReadOnlyCollection<Expression> _args;
        private readonly MethodInfo _stringMethod;

        public NewObjTableFilter(MemberExpression left, FilterComparisonOperator op, NewExpression right)
            : base(left, op)
        {
            objCtor = right.Constructor;
            _args = right.Arguments;
            _stringMethod = FilterMethodForConstant(typeof(T));
        }

        public override string ToString()
        {
            return (string)_stringMethod.Invoke(null, new[]
                {
                    Left.Member.Name,
                    FilterComparison.ToString(ComparisonOperator),
                    objCtor.Invoke(
                        _args.Select(a => ((ConstantExpression)a).Value).ToArray()
                        )
                });
        }
    }
}
