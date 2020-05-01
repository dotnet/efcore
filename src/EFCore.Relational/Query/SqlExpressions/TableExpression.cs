// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a table or view in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    // Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
    public sealed class TableExpression : TableExpressionBase
    {
        internal TableExpression([NotNull] ITableBase table)
            : base(table.Name.Substring(0, 1).ToLower())
        {
            Name = table.Name;
            Schema = table.Schema;
        }

        /// <inheritdoc />
        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }

            expressionPrinter.Append(Name).Append(" AS ").Append(Alias);
        }

        /// <summary>
        ///     The name of the table or view.
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///     The schema of the table or view.
        /// </summary>
        public string Schema { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
            // This should be reference equal only.
            => obj != null && ReferenceEquals(this, obj);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, Schema);
    }
}
