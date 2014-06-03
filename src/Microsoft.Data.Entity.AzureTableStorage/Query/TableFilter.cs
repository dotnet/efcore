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
        protected string _storageName;

        private TableFilter(string storageName, FilterComparisonOperator op)
        {
            ComparisonOperator = op;
            _storageName = storageName;
        }

        public object Right { get; protected set; }

        public FilterComparisonOperator ComparisonOperator { get; protected set; }

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


        internal class ConstantTableFilter : TableFilter
        {
            private readonly MethodInfo _stringMethod;

            public ConstantTableFilter(string storageName, FilterComparisonOperator op, ConstantExpression right)
                : base(storageName, op)
            {
                Right = right.Value;
                _stringMethod = FilterMethodForConstant(right.Type);
            }

            public override string ToString()
            {
                return (string)_stringMethod.Invoke(null, new object[] { _storageName, FilterComparison.ToString(ComparisonOperator), Right });
            }
        }

        internal class MemberTableFilter : TableFilter
        {
            private readonly MethodInfo _stringMethod;
            private readonly Func<object> _getRightValue;
            public new MemberInfo Right { get; protected set; }

            [UsedImplicitly]
            public MemberTableFilter(string storageName, FilterComparisonOperator op, MemberExpression right)
                : base(storageName, op)
            {
                Right = right.Member;
                Func<object> getTarget;
                var constantTarget = right.Expression is ConstantExpression;
                if (constantTarget)
                {
                    getTarget = () => ((ConstantExpression)right.Expression).Value;
                }
                else
                {
                    getTarget = () => null;
                }

                if (right.Member is FieldInfo)
                {
                    var fieldInfo = (FieldInfo)Right;
                    _stringMethod = FilterMethodForConstant(fieldInfo.FieldType);
                    _getRightValue = () => fieldInfo.GetValue(getTarget());
                }
                else if (right.Member is PropertyInfo)
                {
                    var propInfo = (PropertyInfo)Right;
                    _stringMethod = FilterMethodForConstant(propInfo.PropertyType);

                    if (!constantTarget)
                    {
                        throw new ArgumentException("Cannot get static property info", "right");
                    }
                    _getRightValue = () => propInfo.GetValue(getTarget());
                }
                else
                {
                    throw new ArgumentException("Cannot get member info", "right");
                }

            }

            public override string ToString()
            {
                return (string)_stringMethod.Invoke(null, new[] { _storageName, FilterComparison.ToString(ComparisonOperator), _getRightValue() });
            }
        }

        internal class NewObjTableFilter : TableFilter
        {
            private readonly ConstructorInfo objCtor;
            private readonly IReadOnlyCollection<Expression> _args;
            private readonly MethodInfo _stringMethod;

            [UsedImplicitly]
            public NewObjTableFilter(string storageName, FilterComparisonOperator op, NewExpression right)
                : base(storageName, op)
            {
                objCtor = right.Constructor;
                _args = right.Arguments;
                _stringMethod = FilterMethodForConstant(right.Type);
            }

            public override string ToString()
            {
                return (string)_stringMethod.Invoke(null, new[]
                    {
                        _storageName,
                        FilterComparison.ToString(ComparisonOperator),
                        objCtor.Invoke(
                            _args.Select(a => ((ConstantExpression)a).Value).ToArray()
                            )
                    });
            }
        }
    }
}
