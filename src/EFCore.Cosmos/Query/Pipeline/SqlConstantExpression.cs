// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class SqlConstantExpression : SqlExpression
    {
        private readonly ConstantExpression _constantExpression;

        public SqlConstantExpression(ConstantExpression constantExpression, CoreTypeMapping typeMapping)
            : base(constantExpression.Type, typeMapping)
        {
            _constantExpression = constantExpression;
        }

        public object Value => _constantExpression.Value;

        public SqlExpression ApplyTypeMapping(CoreTypeMapping typeMapping)
            => new SqlConstantExpression(_constantExpression, typeMapping);
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        public override void Print(ExpressionPrinter expressionPrinter) => Print(Value, expressionPrinter);

        private void Print(
            object value,
            ExpressionPrinter expressionPrinter)
        {
            if (value is IEnumerable enumerable
                && !(value is string)
                && !(value is byte[]))
            {
                bool first = true;
                foreach (var item in enumerable)
                {
                    if (!first)
                    {
                        expressionPrinter.StringBuilder.Append(", ");
                    }

                    first = false;
                    Print(item, expressionPrinter);
                }
            }
            else
            {
                var jToken = GenerateJToken(Value, Type, TypeMapping);

                expressionPrinter.StringBuilder.Append(jToken == null ? "null" : jToken.ToString(Formatting.None));
            }
        }

        private JToken GenerateJToken(object value, Type type, CoreTypeMapping typeMapping)
        {
            var mappingClrType = typeMapping.ClrType.UnwrapNullableType();
            if (value?.GetType().IsInteger() == true
                    && mappingClrType.IsEnum)
            {
                value = Enum.ToObject(mappingClrType, value);
            }

            var converter = typeMapping.Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            if (value == null)
            {
                return null;
            }

            return (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlConstantExpression sqlConstantExpression
                    && Equals(sqlConstantExpression));

        private bool Equals(SqlConstantExpression sqlConstantExpression)
            => base.Equals(sqlConstantExpression)
            && (Value == null
                ? sqlConstantExpression.Value == null
                : Value.Equals(sqlConstantExpression.Value));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Value);
    }
}
