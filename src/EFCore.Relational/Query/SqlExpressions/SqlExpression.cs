// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a scalar value or a SQL token in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class SqlExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="SqlExpression" /> class.
        /// </summary>
        /// <param name="type"> The <see cref="System.Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        protected SqlExpression([NotNull] Type type, [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(type, nameof(type));

            Check.DebugAssert(!type.IsNullableValueType(), "SqlExpression.Type must be reference type or non-nullable value type");

            Type = type;
            TypeMapping = typeMapping;
        }

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        ///     The <see cref="RelationalTypeMapping" /> associated with this expression.
        /// </summary>
        public virtual RelationalTypeMapping TypeMapping { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => throw new InvalidOperationException(RelationalStrings.VisitChildrenMustBeOverridden);

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <summary>
        ///     Creates a printable string representation of the given expression using <see cref="ExpressionPrinter" />.
        /// </summary>
        /// <param name="expressionPrinter"> The expression printer to use. </param>
        protected abstract void Print([NotNull] ExpressionPrinter expressionPrinter);

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
            => Print(expressionPrinter);

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlExpression sqlExpression
                    && Equals(sqlExpression));

        private bool Equals(SqlExpression sqlExpression)
            => Type == sqlExpression.Type
                && ((TypeMapping == null && sqlExpression.TypeMapping == null)
                    || TypeMapping?.Equals(sqlExpression.TypeMapping) == true);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(Type, TypeMapping);
    }
}
