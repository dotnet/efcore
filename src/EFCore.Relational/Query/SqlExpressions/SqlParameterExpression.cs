// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a parameter in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This is a simple wrapper around a <see cref="ParameterExpression" /> in the SQL tree.
    ///         Instances of this type cannot be constructed by application or database provider code. If this is a problem for your
    ///         application or provider, then please file an issue at https://github.com/dotnet/efcore.
    ///     </para>
    /// </summary>
    public sealed class SqlParameterExpression : SqlExpression
    {
        private readonly ParameterExpression _parameterExpression;
        private readonly string _name;

        internal SqlParameterExpression(ParameterExpression parameterExpression, RelationalTypeMapping? typeMapping)
            : base(parameterExpression.Type.UnwrapNullableType(), typeMapping)
        {
            Check.DebugAssert(parameterExpression.Name != null, "Parameter must have name.");

            _parameterExpression = parameterExpression;
            _name = parameterExpression.Name;
            IsNullable = parameterExpression.Type.IsNullableType();
        }

        /// <summary>
        ///     The name of the parameter.
        /// </summary>
        public string Name
            => _name;

        /// <summary>
        ///     The bool value indicating if this parameter can have null values.
        /// </summary>
        public bool IsNullable { get; }

        /// <summary>
        ///     Applies supplied type mapping to this expression.
        /// </summary>
        /// <param name="typeMapping"> A relational type mapping to apply. </param>
        /// <returns> A new expression which has supplied type mapping. </returns>
        public SqlExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
            => new SqlParameterExpression(_parameterExpression, typeMapping);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("@" + _parameterExpression.Name);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlParameterExpression sqlParameterExpression
                    && Equals(sqlParameterExpression));

        private bool Equals(SqlParameterExpression sqlParameterExpression)
            => base.Equals(sqlParameterExpression)
                && Name == sqlParameterExpression.Name;

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Name);
    }
}
