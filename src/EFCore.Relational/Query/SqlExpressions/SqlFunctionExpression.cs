// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class SqlFunctionExpression : SqlExpression
    {
        public static SqlFunctionExpression CreateNiladic(
            [NotNull] string name,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            return new SqlFunctionExpression(
                instance: null, schema: null, name, niladic: true, arguments: null, builtIn: true, type, typeMapping);
        }

        public static SqlFunctionExpression CreateNiladic(
            [NotNull] string schema,
            [NotNull] string name,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(schema, nameof(schema));
            Check.NotNull(type, nameof(type));

            return new SqlFunctionExpression(
                instance: null, schema, name, niladic: true, arguments: null, builtIn: true, type, typeMapping);
        }

        public static SqlFunctionExpression CreateNiladic(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(instance, nameof(instance));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            return new SqlFunctionExpression(
                instance, schema: null, name, niladic: true, arguments: null, builtIn: true, type, typeMapping);
        }

        public static SqlFunctionExpression Create(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(instance, nameof(instance));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(type, nameof(type));

            return new SqlFunctionExpression(instance, schema: null, name, niladic: false, arguments, builtIn: true, type, typeMapping);
        }

        public static SqlFunctionExpression Create(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(type, nameof(type));

            return new SqlFunctionExpression(
                instance: null, schema: null, name, niladic: false, arguments, builtIn: true, type, typeMapping);
        }

        public static SqlFunctionExpression Create(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(type, nameof(type));

            return new SqlFunctionExpression(instance: null, schema, name, niladic: false, arguments, builtIn: false, type, typeMapping);
        }

        public SqlFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [CanBeNull] string schema,
            [NotNull] string name,
            bool niladic,
            [CanBeNull] IEnumerable<SqlExpression> arguments,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            Instance = instance;
            Name = name;
            Schema = schema;
            IsNiladic = niladic;
            IsBuiltIn = builtIn;
            Arguments = (arguments ?? Array.Empty<SqlExpression>()).ToList();
        }

        public virtual string Name { get; }
        public virtual string Schema { get; }
        public virtual bool IsNiladic { get; }
        public virtual bool IsBuiltIn { get; }
        public virtual IReadOnlyList<SqlExpression> Arguments { get; }
        public virtual SqlExpression Instance { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

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
                    Name,
                    IsNiladic,
                    arguments,
                    IsBuiltIn,
                    Type,
                    TypeMapping)
                : this;
        }

        public virtual SqlFunctionExpression ApplyTypeMapping([CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(
                Instance,
                Schema,
                Name,
                IsNiladic,
                Arguments,
                IsBuiltIn,
                Type,
                typeMapping ?? TypeMapping);

        public virtual SqlFunctionExpression Update([CanBeNull] SqlExpression instance, [CanBeNull] IReadOnlyList<SqlExpression> arguments)
        {
            return instance != Instance || !arguments.SequenceEqual(Arguments)
                ? new SqlFunctionExpression(instance, Schema, Name, IsNiladic, arguments, IsBuiltIn, Type, TypeMapping)
                : this;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".").Append(Name);
            }
            else
            {
                if (Instance != null)
                {
                    expressionPrinter.Visit(Instance);
                    expressionPrinter.Append(".");
                }

                expressionPrinter.Append(Name);
            }

            if (!IsNiladic)
            {
                expressionPrinter.Append("(");
                expressionPrinter.VisitList(Arguments);
                expressionPrinter.Append(")");
            }
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlFunctionExpression sqlFunctionExpression
                    && Equals(sqlFunctionExpression));

        private bool Equals(SqlFunctionExpression sqlFunctionExpression)
            => base.Equals(sqlFunctionExpression)
                && string.Equals(Name, sqlFunctionExpression.Name)
                && string.Equals(Schema, sqlFunctionExpression.Schema)
                && ((Instance == null && sqlFunctionExpression.Instance == null)
                    || (Instance != null && Instance.Equals(sqlFunctionExpression.Instance)))
                && Arguments.SequenceEqual(sqlFunctionExpression.Arguments);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Name);
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
