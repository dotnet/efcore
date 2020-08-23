// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
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

        internal SqlParameterExpression(ParameterExpression parameterExpression, RelationalTypeMapping typeMapping)
            : base(parameterExpression.Type.UnwrapNullableType(), typeMapping)
        {
            _parameterExpression = parameterExpression;
            IsNullable = parameterExpression.Type.IsNullableType();
        }

        /// <summary>
        ///     The name of the parameter.
        /// </summary>
        public string Name
            => _parameterExpression.Name;

        /// <summary>
        ///     The bool value indicating if this parameter can have null values.
        /// </summary>
        public bool IsNullable { get; }

        /// <summary>
        ///     Applies supplied type mapping to this expression.
        /// </summary>
        /// <param name="typeMapping"> A relational type mapping to apply. </param>
        /// <returns> A new expression which has supplied type mapping. </returns>
        public SqlExpression ApplyTypeMapping([CanBeNull] RelationalTypeMapping typeMapping)
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
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlParameterExpression sqlParameterExpression
                    && Equals(sqlParameterExpression));

        private bool Equals(SqlParameterExpression sqlParameterExpression)
            => base.Equals(sqlParameterExpression)
                && string.Equals(Name, sqlParameterExpression.Name);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Name);
    }
}
