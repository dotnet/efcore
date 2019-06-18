// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public interface ISqlExpressionFactory
    {
        SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, CoreTypeMapping typeMapping);
        SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression);
        //CoreTypeMapping GetTypeMappingForValue(object value);
        CoreTypeMapping FindMapping(Type type);

        SqlBinaryExpression MakeBinary(ExpressionType operatorType, SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping);
        // Comparison
        SqlBinaryExpression Equal(SqlExpression left, SqlExpression right);
        SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right);
        SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right);
        SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right);
        SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right);
        SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right);
        // Logical
        SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right);
        SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right);
        // Arithmetic
        SqlBinaryExpression Add(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);
        SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);
        SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);
        SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);
        SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);
        // Bitwise
        SqlBinaryExpression And(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);
        SqlBinaryExpression Or(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);
        // Other
        SqlBinaryExpression Coalesce(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null);

        SqlBinaryExpression IsNull(SqlExpression operand);
        SqlBinaryExpression IsNotNull(SqlExpression operand);
        SqlUnaryExpression Convert(SqlExpression operand, Type type, CoreTypeMapping typeMapping = null);
        SqlUnaryExpression Not(SqlExpression operand);
        SqlUnaryExpression Negate(SqlExpression operand);

        SqlFunctionExpression Function(
            string functionName, IEnumerable<SqlExpression> arguments, Type returnType, CoreTypeMapping typeMapping = null);
        SqlConditionalExpression Condition(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse);
        InExpression In(SqlExpression item, SqlExpression values, bool negated);
        SqlConstantExpression Constant(object value, CoreTypeMapping typeMapping = null);
        SelectExpression Select(IEntityType entityType);
    }
}
