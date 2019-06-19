// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlFunctionExpression : SqlExpression
    {
        // niladic
        public SqlFunctionExpression(
            string functionName,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance: null, schema: null, functionName, niladic: true, arguments: null, builtIn: true, type, typeMapping)
        {
        }

        // niladic
        public SqlFunctionExpression(
            string schema,
            string functionName,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance: null, schema, functionName, niladic: true, arguments: null, builtIn: true, type, typeMapping)
        {
        }

        // niladic
        public SqlFunctionExpression(
            SqlExpression instance,
            string functionName,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance, schema: null, functionName, niladic: true, arguments: null, builtIn: true, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            SqlExpression instance,
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance, schema: null, functionName, niladic: false, arguments, builtIn: true, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance: null, schema: null, functionName, niladic: false, arguments, builtIn: true, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            string schema,
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance: null, schema, functionName, niladic: false, arguments, builtIn: false, type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            Expression instance,
            string schema,
            string functionName,
            bool niladic,
            IEnumerable<SqlExpression> arguments,
            bool builtIn,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Instance = instance;
            FunctionName = functionName;
            Schema = schema;
            IsNiladic = niladic;
            IsBuiltIn = builtIn;
            Arguments = (arguments ?? Array.Empty<SqlExpression>()).ToList();
        }

        public string FunctionName { get; }
        public string Schema { get; }
        public bool IsNiladic { get; }
        public bool IsBuiltIn { get; }
        public IReadOnlyList<SqlExpression> Arguments { get; }
        public Expression Instance { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var changed = false;
            var instance = (SqlExpression)visitor.Visit(Instance);
            changed |= instance != Instance;
            var arguments = new SqlExpression[Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
                changed |= arguments[i] != Arguments[i];
            }

            return changed
                ? new SqlFunctionExpression(
                    instance,
                    Schema,
                    FunctionName,
                    IsNiladic,
                    arguments,
                    IsBuiltIn,
                    Type,
                    TypeMapping)
                : this;
        }

        public SqlFunctionExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(
                Instance,
                Schema,
                FunctionName,
                IsNiladic,
                Arguments,
                IsBuiltIn,
                Type,
                typeMapping ?? TypeMapping);

        public SqlFunctionExpression Update(SqlExpression instance, IReadOnlyList<SqlExpression> arguments)
            => instance != Instance || !arguments.SequenceEqual(Arguments)
                ? new SqlFunctionExpression(instance, Schema, FunctionName, IsNiladic, arguments, IsBuiltIn, Type, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.StringBuilder
                    .Append(Schema)
                    .Append(".")
                    .Append(FunctionName);
            }
            else
            {
                if (Instance != null)
                {
                    expressionPrinter.Visit(Instance);
                    expressionPrinter.StringBuilder.Append(".");
                }

                expressionPrinter.StringBuilder.Append(FunctionName);
            }

            if (!IsNiladic)
            {
                expressionPrinter.StringBuilder.Append("(");
                expressionPrinter.VisitList(Arguments);
                expressionPrinter.StringBuilder.Append(")");
            }
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlFunctionExpression sqlFunctionExpression
                    && Equals(sqlFunctionExpression));

        private bool Equals(SqlFunctionExpression sqlFunctionExpression)
            => base.Equals(sqlFunctionExpression)
            && string.Equals(FunctionName, sqlFunctionExpression.FunctionName)
            && string.Equals(Schema, sqlFunctionExpression.Schema)
            && ((Instance == null && sqlFunctionExpression.Instance == null)
                || (Instance != null && Instance.Equals(sqlFunctionExpression.Instance)))
            && Arguments.SequenceEqual(sqlFunctionExpression.Arguments);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(FunctionName);
            hash.Add(IsNiladic);
            hash.Add(Schema);
            hash.Add(Instance);
            for (var i = 0; i < Arguments.Count; i++)
            {
                hash.Add(Arguments[i]);
            }
            return hash.ToHashCode();
        }
    }
}
