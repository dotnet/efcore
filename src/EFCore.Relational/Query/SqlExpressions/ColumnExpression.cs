// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a column in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public abstract class ColumnExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ColumnExpression" /> class.
        /// </summary>
        /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
        /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
        protected ColumnExpression(Type type, RelationalTypeMapping? typeMapping)
            : base(type, typeMapping)
        {
        }

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     The table from which column is being referenced.
        /// </summary>
        public abstract TableExpressionBase Table { get; }

        /// <summary>
        ///     The alias of the table from which column is being referenced.
        /// </summary>
        public abstract string TableAlias { get; }

        /// <summary>
        ///     The bool value indicating if this column can have null values.
        /// </summary>
        public abstract bool IsNullable { get; }

        /// <summary>
        ///     Makes this column nullable.
        /// </summary>
        /// <returns>A new expression which has <see cref="IsNullable" /> property set to true.</returns>
        public abstract ColumnExpression MakeNullable();

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append(TableAlias).Append(".");
            expressionPrinter.Append(Name);
        }

        private string DebuggerDisplay()
            => $"{TableAlias}.{Name}";
    }
}
