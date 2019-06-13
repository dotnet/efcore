// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class TableExpression : TableExpressionBase
    {
        internal TableExpression(string table, string schema, [NotNull] string alias)
            : base(alias)
        {
            Table = table;
            Schema = schema;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.StringBuilder
                    .Append(Schema)
                    .Append(".");
            }

            expressionPrinter.StringBuilder
                .Append(Table)
                .Append(" AS ")
                .Append(Alias);
        }

        public string Table { get; }
        public string Schema { get; }

        public override bool Equals(object obj)
            // This should be reference equal only.
            => obj != null && ReferenceEquals(this, obj);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Table, Schema);
    }
}
