// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    // Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
    public sealed class TableExpression : TableExpressionBase
    {
        internal TableExpression(string name, string schema, [NotNull] string alias)
            : base(alias)
        {
            Name = name;
            Schema = schema;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }

            expressionPrinter.Append(Name).Append(" AS ").Append(Alias);
        }

        public string Name { get; }
        public string Schema { get; }

        public override bool Equals(object obj)
            // This should be reference equal only.
            => obj != null && ReferenceEquals(this, obj);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, Schema);
    }
}
