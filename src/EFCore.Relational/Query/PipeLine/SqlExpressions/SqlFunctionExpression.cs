// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlFunctionExpression : SqlExpression
    {
        #region Fields & Constructors
        public SqlFunctionExpression(
            string functionName,
            bool niladic,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(null, null, functionName, niladic, null, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            string schema,
            string functionName,
            bool niladic,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(null, schema, functionName, niladic, null, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            SqlExpression instance,
            string functionName,
            bool niladic,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance, null, functionName, niladic, null, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(null, null, functionName, false, arguments, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            string schema,
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(null, schema, functionName, false, arguments, type, typeMapping)
        {
        }

        public SqlFunctionExpression(
            SqlExpression instance,
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping)
            : this(instance, null, functionName, false, arguments, type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            Expression instance,
            string schema,
            string functionName,
            bool niladic,
            IEnumerable<SqlExpression> arguments,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Instance = instance;
            FunctionName = functionName;
            Schema = schema;
            IsNiladic = niladic;
            Arguments = (arguments ?? Array.Empty<SqlExpression>()).ToList();
        }
        #endregion

        #region Public Properties
        public string FunctionName { get; }
        public string Schema { get; }
        public bool IsNiladic { get; }
        public IReadOnlyList<SqlExpression> Arguments { get; }
        public Expression Instance { get; }
        #endregion

        #region Expression-based methods
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
                    Type,
                    TypeMapping)
                : this;
        }

        public SqlFunctionExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
        {
            return new SqlFunctionExpression(
                Instance,
                Schema,
                FunctionName,
                IsNiladic,
                Arguments,
                Type,
                typeMapping ?? TypeMapping);
        }

        public SqlFunctionExpression Update(SqlExpression instance, IReadOnlyList<SqlExpression> arguments)
        {
            return instance != Instance || arguments != Arguments
                ? new SqlFunctionExpression(instance, Schema, FunctionName, IsNiladic, arguments, Type, TypeMapping)
                : this;
        }
        #endregion

        #region Equality & HashCode
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
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ FunctionName.GetHashCode();
                hashCode = (hashCode * 397) ^ (Schema?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Instance?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Arguments.Aggregate(
                    0, (current, value) => current + ((current * 397) ^ value.GetHashCode()));


                return hashCode;
            }
        }
        #endregion
    }
}
