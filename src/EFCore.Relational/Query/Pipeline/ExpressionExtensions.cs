// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public static class ExpressionExtensions
    {
        public static RelationalTypeMapping InferTypeMapping(
            IValueConverterSelector valueConverterSelector,
            params Expression[] expressions)
        {
            for (var i = 0; i < expressions.Length; i++)
            {
                if (expressions[i] is SqlExpression sqlExpression)
                {
                    var mapping = sqlExpression.TypeMapping;
                    if (mapping != null)
                    {
                        return mapping;
                    }

                    // This is to cover the cases of enums and char values being erased when using
                    // literals in the expression tree. It adds a new converter onto the type mapper
                    // that takes the literal and converts it back to the type that the type mapping
                    // is expecting.
                    // This code is temporary until more complete type inference is completed.
                    if (sqlExpression is SqlUnaryExpression sqlUnaryExpression
                        && sqlUnaryExpression.OperatorType == ExpressionType.Convert)
                    {
                        var operandType = sqlUnaryExpression.Operand.Type.UnwrapNullableType();
                        mapping = InferTypeMapping(valueConverterSelector, sqlUnaryExpression.Operand);

                        if (mapping != null
                            && (operandType.IsEnum
                            || operandType == typeof(char)
                            || mapping.Converter?.ProviderClrType == typeof(byte[])))
                        {
                            if (mapping.ClrType != sqlUnaryExpression.Type)
                            {
                                var converter = valueConverterSelector.Select(sqlUnaryExpression.Type, mapping.ClrType).ToList();

                                mapping = (RelationalTypeMapping)mapping.Clone(converter.First().Create());
                            }

                            return mapping;
                        }
                    }
                }
            }

            return null;
        }
    }
}
