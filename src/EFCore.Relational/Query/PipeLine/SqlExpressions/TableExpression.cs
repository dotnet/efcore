// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class TableExpression : TableExpressionBase
    {
        #region Fields & Constructors
        public TableExpression(string table, string schema, string alias)
            : base(alias)
        {
            Table = table;
            Schema = schema;
        }
        #endregion

        #region Public Properties
        public string Table { get; }
        public string Schema { get; }
        #endregion

        #region Equality & HashCode

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TableExpression tableExpression
                    && Equals(tableExpression));

        private bool Equals(TableExpression tableExpression)
            => base.Equals(tableExpression)
            && string.Equals(Table, tableExpression.Table)
            && string.Equals(Schema, tableExpression.Schema);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Table.GetHashCode();
                hashCode = (hashCode * 397) ^ Schema.GetHashCode();

                return hashCode;
            }
        }
        #endregion
    }
}
