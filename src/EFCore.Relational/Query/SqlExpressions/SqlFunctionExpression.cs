// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a function call in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class SqlFunctionExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class which represents a built-in niladic function.
        /// </summary>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlFunctionExpression(
            [NotNull] string functionName,
            bool nullable,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                instance: null, schema: null, functionName, nullable, instancePropagatesNullability: null, builtIn: true, type, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class which represents a niladic function.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlFunctionExpression(
            [NotNull] string schema,
            [NotNull] string functionName,
            bool nullable,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                instance: null, Check.NotEmpty(schema, nameof(schema)), functionName, nullable, instancePropagatesNullability: null,
                builtIn: false, type, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class which represents a niladic function which is invoked on an
        ///     instance.
        /// </summary>
        /// <param name="instance"> An expression on which the function is defined. </param>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="instancePropagatesNullability"> A value indicating if instance propagates null to result. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlFunctionExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string functionName,
            bool nullable,
            bool instancePropagatesNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                Check.NotNull(instance, nameof(instance)), schema: null, functionName, nullable, instancePropagatesNullability,
                builtIn: true, type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [CanBeNull] string schema,
            [NotNull] string name,
            bool nullable,
            bool? instancePropagatesNullability,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                instance, schema, name, niladic: true, arguments: null, nullable, instancePropagatesNullability,
                argumentsPropagateNullability: null, builtIn, type, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class which represents a built-in function.
        /// </summary>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="argumentsPropagateNullability"> A list of bool values indicating whether individual arguments propagate null to result. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlFunctionExpression(
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                instance: null, schema: null, functionName, arguments, nullable, instancePropagatesNullability: null,
                argumentsPropagateNullability, builtIn: true, type, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class which represents a function.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="argumentsPropagateNullability"> A list of bool values indicating whether individual arguments propagate null to result. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlFunctionExpression(
            [CanBeNull] string schema,
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                instance: null, Check.NullButNotEmpty(schema, nameof(schema)), functionName, arguments, nullable,
                instancePropagatesNullability: null, argumentsPropagateNullability, builtIn: false, type, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class which represents a function which is invoked on an instance.
        /// </summary>
        /// <param name="instance"> An expression on which the function is applied. </param>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="instancePropagatesNullability"> A value indicating if instance propagates null to result. </param>
        /// <param name="argumentsPropagateNullability"> A list of bool values indicating whether individual arguments propagate null to result. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlFunctionExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool instancePropagatesNullability,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                Check.NotNull(instance, nameof(instance)), schema: null, functionName, arguments, nullable, instancePropagatesNullability,
                argumentsPropagateNullability, builtIn: true, type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool? instancePropagatesNullability,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(
                instance, schema, name, niladic: false, Check.NotNull(arguments, nameof(arguments)), nullable,
                instancePropagatesNullability, Check.NotNull(argumentsPropagateNullability, nameof(argumentsPropagateNullability)), builtIn,
                type, typeMapping)
        {
        }

        private SqlFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [CanBeNull] string schema,
            [NotNull] string name,
            bool niladic,
            [CanBeNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool? instancePropagatesNullability,
            [CanBeNull] IEnumerable<bool> argumentsPropagateNullability,
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
            Arguments = arguments?.ToList();
            IsNullable = nullable;
            InstancePropagatesNullability = instancePropagatesNullability;
            ArgumentsPropagateNullability = argumentsPropagateNullability?.ToList();
        }

        /// <summary>
        ///     The name of the function.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     The schema in which the function is defined, if any.
        /// </summary>
        public virtual string Schema { get; }

        /// <summary>
        ///     A bool value indicating if the function is niladic.
        /// </summary>
        public virtual bool IsNiladic { get; }

        /// <summary>
        ///     A bool value indicating if the function is built-in.
        /// </summary>
        public virtual bool IsBuiltIn { get; }

        /// <summary>
        ///     The list of arguments of this function.
        /// </summary>
        public virtual IReadOnlyList<SqlExpression> Arguments { get; }

        /// <summary>
        ///     The instance on which this function is applied.
        /// </summary>
        public virtual SqlExpression Instance { get; }

        /// <summary>
        ///     A bool value indicating if the function can return null result.
        /// </summary>
        public virtual bool IsNullable { get; }

        /// <summary>
        ///     A bool value indicating if the instance propagates null to the result.
        /// </summary>
        public virtual bool? InstancePropagatesNullability { get; }

        /// <summary>
        ///     A list of bool values indicating whether individual argument propagate null to the result.
        /// </summary>

        public virtual IReadOnlyList<bool> ArgumentsPropagateNullability { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var instance = (SqlExpression)visitor.Visit(Instance);
            changed |= instance != Instance;

            SqlExpression[] arguments = default;
            if (!IsNiladic)
            {
                arguments = new SqlExpression[Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
                    changed |= arguments[i] != Arguments[i];
                }
            }

            return changed
                ? new SqlFunctionExpression(
                    instance,
                    Schema,
                    Name,
                    IsNiladic,
                    arguments,
                    IsNullable,
                    InstancePropagatesNullability,
                    ArgumentsPropagateNullability,
                    IsBuiltIn,
                    Type,
                    TypeMapping)
                : this;
        }

        /// <summary>
        ///     Applies supplied type mapping to this expression.
        /// </summary>
        /// <param name="typeMapping"> A relational type mapping to apply. </param>
        /// <returns> A new expression which has supplied type mapping. </returns>
        public virtual SqlFunctionExpression ApplyTypeMapping([CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(
                Instance,
                Schema,
                Name,
                IsNiladic,
                Arguments,
                IsNullable,
                InstancePropagatesNullability,
                ArgumentsPropagateNullability,
                IsBuiltIn,
                Type,
                typeMapping ?? TypeMapping);

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="instance"> The <see cref="Instance" /> property of the result. </param>
        /// <param name="arguments"> The <see cref="Arguments" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual SqlFunctionExpression Update([CanBeNull] SqlExpression instance, [CanBeNull] IReadOnlyList<SqlExpression> arguments)
        {
            return instance != Instance || !arguments?.SequenceEqual(Arguments) == true
                ? new SqlFunctionExpression(
                    instance,
                    Schema,
                    Name,
                    IsNiladic,
                    arguments,
                    IsNullable,
                    InstancePropagatesNullability,
                    ArgumentsPropagateNullability,
                    IsBuiltIn,
                    Type,
                    TypeMapping)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
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
                expressionPrinter.VisitCollection(Arguments);
                expressionPrinter.Append(")");
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        #region ObsoleteMethods

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use new SqlFunctionExpression(...) with appropriate arguments.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SqlFunctionExpression CreateNiladic(
            [NotNull] string name,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(name, nullable: true, type, typeMapping);

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use new SqlFunctionExpression(...) with appropriate arguments.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SqlFunctionExpression CreateNiladic(
            [NotNull] string schema,
            [NotNull] string name,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(schema, name, nullable: true, type, typeMapping);

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance"> An expression on which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use new SqlFunctionExpression(...) with appropriate arguments.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SqlFunctionExpression CreateNiladic(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(instance, name, nullable: true, instancePropagatesNullability: false, type, typeMapping);

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance"> An expression on which the function is applied. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use new SqlFunctionExpression(...) with appropriate arguments.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SqlFunctionExpression Create(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(
                instance,
                name,
                arguments,
                nullable: true,
                instancePropagatesNullability: false,
                argumentsPropagateNullability: arguments.Select(a => false),
                type,
                typeMapping);

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use new SqlFunctionExpression(...) with appropriate arguments.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SqlFunctionExpression Create(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(
                name, arguments, nullable: true, argumentsPropagateNullability: arguments.Select(a => false), type, typeMapping);

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use new SqlFunctionExpression(...) with appropriate arguments.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SqlFunctionExpression Create(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlFunctionExpression(
                schema, name, arguments, nullable: true, argumentsPropagateNullability: arguments.Select(a => false), type, typeMapping);

        #endregion
    }
}
