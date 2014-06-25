// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    [DebuggerDisplay("TableFilter")]
    public class TableFilter
    {
        private readonly string _storageName;
        private readonly string _operator;
        protected Func<string, string, object, string> QueryStringMethod;

        private TableFilter(string storageName, FilterComparisonOperator op)
        {
            _operator = FilterComparison.ToString(op);
            _storageName = storageName;
        }


        private string CreateQueryStringFromObject(object obj)
        {
            if (obj == null)
            {
                return String.Empty;
            }
            if (_storageName == "PartitionKey"
                || _storageName == "RowKey")
            {
                return TableQuery.GenerateFilterCondition(_storageName, _operator, obj.ToString());
            }
            return QueryStringMethod(_storageName, _operator, obj);
        }

        internal class ConstantTableFilter : TableFilter
        {
            public object Right { get; protected set; }

            public ConstantTableFilter(string storageName, FilterComparisonOperator op, ConstantExpression right)
                : base(storageName, op)
            {
                Right = right.Value;
                QueryStringMethod = StringMethodFromType(right.Type);
            }

            public override string ToString()
            {
                return CreateQueryStringFromObject(Right);
            }
        }

        internal class MemberTableFilter : TableFilter
        {
            private readonly Func<object> _getRightValue;
            public MemberInfo Right { get; protected set; }

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
                    QueryStringMethod = StringMethodFromType(fieldInfo.FieldType);
                    _getRightValue = () => fieldInfo.GetValue(getTarget());
                }
                else if (right.Member is PropertyInfo)
                {
                    var propInfo = (PropertyInfo)Right;
                    QueryStringMethod = StringMethodFromType(propInfo.PropertyType);

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
                return CreateQueryStringFromObject(_getRightValue());
            }
        }

        internal class NewObjTableFilter : TableFilter
        {
            private readonly ConstructorInfo _objCtor;
            private readonly IReadOnlyCollection<Expression> _args;

            public NewObjTableFilter(string storageName, FilterComparisonOperator op, NewExpression right)
                : base(storageName, op)
            {
                _objCtor = right.Constructor;
                _args = right.Arguments;
                QueryStringMethod = StringMethodFromType(right.Type);
            }

            public override string ToString()
            {
                return CreateQueryStringFromObject(_objCtor.Invoke(_args.Select(a => ((ConstantExpression)a).Value).ToArray()));
            }
        }


        protected static Func<string, string, object, string> StringMethodFromType(Type type)
        {
            if (type.IsAssignableFrom(typeof(string)))
            {
                return (name, op, value) => TableQuery.GenerateFilterCondition(name, op, value.ToString());
            }
            else if (type.IsAssignableFrom(typeof(double)))
            {
                return (name, op, value) => TableQuery.GenerateFilterConditionForDouble(name, op, (double)value);
            }
            else if (type.IsAssignableFrom(typeof(int)))
            {
                return (name, op, value) => TableQuery.GenerateFilterConditionForInt(name, op, (int)value);
            }
            else if (type.IsAssignableFrom(typeof(long)))
            {
                return (name, op, value) => TableQuery.GenerateFilterConditionForLong(name, op, (long)value);
            }
            else if (type.IsAssignableFrom(typeof(byte[])))
            {
                return (name, op, value) => TableQuery.GenerateFilterConditionForBinary(name, op, value as byte[]);
            }
            else if (type.IsAssignableFrom(typeof(bool)))
            {
                return (name, op, value) => TableQuery.GenerateFilterConditionForBool(name, op, (bool)value);
                ;
            }
            else if (type.IsAssignableFrom(typeof(DateTimeOffset)))
            {
                return (name, op, value) => TableQuery.GenerateFilterConditionForDate(name, op, (DateTimeOffset)value);
                ;
            }
            else if (type.IsAssignableFrom(typeof(DateTime)))
            {
                return (prop, op, time) => TableQuery.GenerateFilterConditionForDate(prop, op, new DateTimeOffset((DateTime)time));
            }
            else if (type.IsAssignableFrom(typeof(Guid)))
            {
                return (name, op, value) => TableQuery.GenerateFilterConditionForGuid(name, op, (Guid)value);
            }
            throw new ArgumentOutOfRangeException("type", "Cannot generate filter method for this type");
        }

    }
}
